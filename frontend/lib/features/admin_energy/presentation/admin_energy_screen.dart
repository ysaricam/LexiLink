import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_energy/application/admin_energy_cubit.dart';
import 'package:lexilink_app/features/admin_energy/data/admin_energy_repository.dart';
import 'package:lexilink_app/features/admin_energy/data/player_energy_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class AdminEnergyScreen extends StatefulWidget {
  const AdminEnergyScreen({super.key, this.cubitFactory});

  final AdminEnergyCubit Function()? cubitFactory;

  @override
  State<AdminEnergyScreen> createState() => _AdminEnergyScreenState();
}

class _AdminEnergyScreenState extends State<AdminEnergyScreen> {
  AdminEnergyCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminEnergyCubit cubit;
    if (widget.cubitFactory != null) {
      cubit = widget.cubitFactory!();
    } else {
      final tokenStore = await SharedPreferencesAdminTokenStore.create();
      cubit = _buildCubit(tokenStore);
    }
    if (!mounted) {
      await cubit.close();
      return;
    }
    setState(() {
      _cubit = cubit;
      _initializing = false;
    });
  }

  AdminEnergyCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminEnergyCubit(
      repository: AdminEnergyRepository(apiClient: apiClient),
    );
  }

  @override
  void dispose() {
    _cubit?.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_initializing || _cubit == null) {
      return const Center(child: CircularProgressIndicator());
    }
    return BlocProvider.value(value: _cubit!, child: const _AdminEnergyView());
  }
}

class _AdminEnergyView extends StatefulWidget {
  const _AdminEnergyView();

  @override
  State<_AdminEnergyView> createState() => _AdminEnergyViewState();
}

class _AdminEnergyViewState extends State<_AdminEnergyView> {
  late final TextEditingController _idController;

  @override
  void initState() {
    super.initState();
    _idController = TextEditingController();
  }

  @override
  void dispose() {
    _idController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<AdminEnergyCubit, AdminEnergyState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminEnergyStatus.failure,
      listener: (context, state) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(state.errorMessage!)),
        );
      },
      builder: (context, state) {
        return SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 32),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 720),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Energy console',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  'Lookup by player GUID, then snap / grant / reset. '
                  'Grant intentionally allows over-max balance.',
                  style: Theme.of(context).textTheme.bodySmall,
                ),
                const SizedBox(height: 16),
                _buildLookupRow(context, state),
                const SizedBox(height: 24),
                _buildBody(context, state),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildLookupRow(BuildContext context, AdminEnergyState state) {
    final busy = state.status == AdminEnergyStatus.loading ||
        state.status == AdminEnergyStatus.saving;
    return Row(
      children: [
        Expanded(
          child: TextField(
            controller: _idController,
            enabled: !busy,
            decoration: const InputDecoration(
              labelText: 'Player GUID',
              border: OutlineInputBorder(),
              isDense: true,
            ),
            onSubmitted: busy ? null : (_) => _submit(context),
          ),
        ),
        const SizedBox(width: 12),
        FilledButton.icon(
          onPressed: busy ? null : () => _submit(context),
          icon: const Icon(Icons.search),
          label: const Text('Look up'),
        ),
      ],
    );
  }

  Widget _buildBody(BuildContext context, AdminEnergyState state) {
    return switch (state.status) {
      AdminEnergyStatus.initial => const SizedBox.shrink(),
      AdminEnergyStatus.loading ||
      AdminEnergyStatus.saving =>
        const Center(child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        )),
      AdminEnergyStatus.notFound => Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            state.errorMessage ?? 'No energy aggregate.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
        ),
      AdminEnergyStatus.failure when state.snapshot == null => Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            state.errorMessage ?? 'Lookup failed.',
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      AdminEnergyStatus.failure || AdminEnergyStatus.loaded =>
        _EnergyCard(snapshot: state.snapshot!),
    };
  }

  void _submit(BuildContext context) {
    final id = _idController.text.trim();
    if (id.isEmpty) return;
    context.read<AdminEnergyCubit>().load(id);
  }
}

class _EnergyCard extends StatelessWidget {
  const _EnergyCard({required this.snapshot});

  final PlayerEnergySnapshot snapshot;

  @override
  Widget build(BuildContext context) {
    final overMax = snapshot.currentAmount > snapshot.maximumAmount;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(
                  snapshot.isFull ? Icons.bolt : Icons.bolt_outlined,
                  size: 28,
                  color: Theme.of(context).colorScheme.primary,
                ),
                const SizedBox(width: 8),
                Text(
                  '${snapshot.currentAmount} / ${snapshot.maximumAmount}',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                if (overMax) ...[
                  const SizedBox(width: 8),
                  _Badge(
                    label: 'Over max',
                    color: Theme.of(context).colorScheme.tertiary,
                  ),
                ],
                if (snapshot.isFull && !overMax) ...[
                  const SizedBox(width: 8),
                  const _Badge(label: 'Full', color: Colors.green),
                ],
              ],
            ),
            const SizedBox(height: 12),
            _kv(context, 'Player id', snapshot.playerId),
            _kv(
              context,
              'Recharge interval',
              '${snapshot.rechargeIntervalSeconds} s',
            ),
            _kv(
              context,
              'Last refilled',
              snapshot.lastRefilledOn.toUtc().toIso8601String(),
            ),
            if (snapshot.secondsUntilNextRefill != null)
              _kv(
                context,
                'Next refill in',
                '${snapshot.secondsUntilNextRefill} s',
              ),
            if (snapshot.fullyRefilledAt != null)
              _kv(
                context,
                'Fully refilled at',
                snapshot.fullyRefilledAt!.toUtc().toIso8601String(),
              ),
            const SizedBox(height: 20),
            Wrap(
              spacing: 12,
              runSpacing: 12,
              children: [
                FilledButton.icon(
                  onPressed: () => _promptSet(context),
                  icon: const Icon(Icons.edit),
                  label: const Text('Set amount'),
                ),
                FilledButton.tonalIcon(
                  onPressed: () => _promptGrant(context),
                  icon: const Icon(Icons.add),
                  label: const Text('Grant bonus'),
                ),
                OutlinedButton.icon(
                  onPressed: () => _confirmReset(context),
                  icon: const Icon(Icons.refresh),
                  label: const Text('Reset to full'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _kv(BuildContext context, String key, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 160,
            child: Text(key, style: Theme.of(context).textTheme.bodySmall),
          ),
          Expanded(child: SelectableText(value)),
        ],
      ),
    );
  }

  Future<void> _promptSet(BuildContext context) async {
    final cubit = context.read<AdminEnergyCubit>();
    final value = await _intDialog(
      context,
      title: 'Set energy amount',
      label: 'New current amount',
      helper: "Snaps the player's current energy to this value (>= 0).",
      validate: (v) => v < 0 ? 'Must be >= 0' : null,
    );
    if (value == null) return;
    await cubit.setAmount(value);
  }

  Future<void> _promptGrant(BuildContext context) async {
    final cubit = context.read<AdminEnergyCubit>();
    final value = await _intDialog(
      context,
      title: 'Grant bonus energy',
      label: 'Bonus amount',
      helper: 'Added on top — may push current above maximum.',
      validate: (v) => v <= 0 ? 'Must be greater than 0' : null,
    );
    if (value == null) return;
    await cubit.grant(value);
  }

  Future<void> _confirmReset(BuildContext context) async {
    final cubit = context.read<AdminEnergyCubit>();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Reset energy?'),
        content: const Text('Resets the player to maximum energy.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Cancel'),
          ),
          FilledButton.tonal(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('Reset'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    await cubit.reset();
  }
}

class _Badge extends StatelessWidget {
  const _Badge({required this.label, required this.color});
  final String label;
  final Color color;
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(label, style: TextStyle(color: color, fontSize: 12)),
    );
  }
}

Future<int?> _intDialog(
  BuildContext context, {
  required String title,
  required String label,
  required String helper,
  required String? Function(int) validate,
}) {
  final controller = TextEditingController();
  final formKey = GlobalKey<FormState>();
  return showDialog<int>(
    context: context,
    builder: (_) => AlertDialog(
      title: Text(title),
      content: Form(
        key: formKey,
        child: TextFormField(
          controller: controller,
          autofocus: true,
          keyboardType: TextInputType.number,
          decoration: InputDecoration(
            labelText: label,
            helperText: helper,
            border: const OutlineInputBorder(),
          ),
          validator: (raw) {
            final trimmed = raw?.trim() ?? '';
            if (trimmed.isEmpty) return 'Required';
            final parsed = int.tryParse(trimmed);
            if (parsed == null) return 'Must be a number';
            return validate(parsed);
          },
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          onPressed: () {
            if (formKey.currentState!.validate()) {
              Navigator.of(context).pop(int.parse(controller.text.trim()));
            }
          },
          child: const Text('Apply'),
        ),
      ],
    ),
  );
}
