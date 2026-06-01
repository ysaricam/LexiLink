import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_energy/application/admin_energy_cubit.dart';
import 'package:lexilink_app/features/admin_energy/data/admin_energy_repository.dart';
import 'package:lexilink_app/features/admin_energy/data/player_energy_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
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
                  context.l10n.adminEnergyConsoleTitle,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  context.l10n.adminEnergyConsoleHelp,
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
    final busy =
        state.status == AdminEnergyStatus.loading ||
        state.status == AdminEnergyStatus.saving;
    return Row(
      children: [
        Expanded(
          child: TextField(
            controller: _idController,
            enabled: !busy,
            decoration: InputDecoration(
              labelText: context.l10n.adminPlayerGuid,
              border: const OutlineInputBorder(),
              isDense: true,
            ),
            onSubmitted: busy ? null : (_) => _submit(context),
          ),
        ),
        const SizedBox(width: 12),
        FilledButton.icon(
          onPressed: busy ? null : () => _submit(context),
          icon: const Icon(Icons.search),
          label: Text(context.l10n.adminLookUp),
        ),
      ],
    );
  }

  Widget _buildBody(BuildContext context, AdminEnergyState state) {
    // Keep the card visible across saving transitions so the user can
    // watch the value flip from old to new. Only show a full-screen
    // spinner before any snapshot has loaded (initial lookup).
    if (state.snapshot != null) {
      return Stack(
        children: [
          _EnergyCard(snapshot: state.snapshot!),
          if (state.status == AdminEnergyStatus.saving)
            const Positioned.fill(
              child: ColoredBox(
                color: Color(0x33000000),
                child: Center(child: CircularProgressIndicator()),
              ),
            ),
        ],
      );
    }
    return switch (state.status) {
      AdminEnergyStatus.initial => const SizedBox.shrink(),
      AdminEnergyStatus.loading => const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        ),
      ),
      AdminEnergyStatus.notFound => Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? context.l10n.adminNoEnergyAggregate,
          style: Theme.of(context).textTheme.bodyMedium,
        ),
      ),
      _ => Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? context.l10n.adminLookupFailed,
          style: TextStyle(color: Theme.of(context).colorScheme.error),
        ),
      ),
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
                    label: context.l10n.adminOverMax,
                    color: Theme.of(context).colorScheme.tertiary,
                  ),
                ],
                if (snapshot.isFull && !overMax) ...[
                  const SizedBox(width: 8),
                  _Badge(label: context.l10n.adminFull, color: Colors.green),
                ],
              ],
            ),
            const SizedBox(height: 12),
            _kv(context, context.l10n.adminPlayerId, snapshot.playerId),
            _kv(
              context,
              context.l10n.adminRechargeInterval,
              '${snapshot.rechargeIntervalSeconds} s',
            ),
            _kv(
              context,
              context.l10n.adminLastRefilled,
              snapshot.lastRefilledOn.toUtc().toIso8601String(),
            ),
            if (snapshot.secondsUntilNextRefill != null)
              _kv(
                context,
                context.l10n.adminNextRefillIn,
                '${snapshot.secondsUntilNextRefill} s',
              ),
            if (snapshot.fullyRefilledAt != null)
              _kv(
                context,
                context.l10n.adminFullyRefilledAt,
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
                  label: Text(context.l10n.adminSetAmount),
                ),
                FilledButton.tonalIcon(
                  onPressed: () => _promptGrant(context),
                  icon: const Icon(Icons.add),
                  label: Text(context.l10n.adminGrantBonus),
                ),
                OutlinedButton.icon(
                  onPressed: () => _confirmReset(context),
                  icon: const Icon(Icons.refresh),
                  label: Text(context.l10n.adminResetToFull),
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
      title: context.l10n.adminSetEnergyAmountTitle,
      label: context.l10n.adminNewCurrentAmount,
      helper: context.l10n.adminSetEnergyHelper,
      validate: (v) => v < 0 ? context.l10n.commonNonNegative : null,
    );
    if (value == null) return;
    await cubit.setAmount(value);
  }

  Future<void> _promptGrant(BuildContext context) async {
    final cubit = context.read<AdminEnergyCubit>();
    final value = await _intDialog(
      context,
      title: context.l10n.adminGrantBonusEnergyTitle,
      label: context.l10n.adminBonusAmount,
      helper: context.l10n.adminGrantEnergyHelper,
      validate: (v) => v <= 0 ? context.l10n.commonGreaterThanZero : null,
    );
    if (value == null) return;
    await cubit.grant(value);
  }

  Future<void> _confirmReset(BuildContext context) async {
    final cubit = context.read<AdminEnergyCubit>();
    final confirmed = await showDialog<bool>(
      context: context,
      useRootNavigator: false,
      builder: (_) => AlertDialog(
        title: Text(context.l10n.adminResetEnergyTitle),
        content: Text(context.l10n.adminResetEnergyMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(context.l10n.commonCancel),
          ),
          FilledButton.tonal(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(context.l10n.commonReset),
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
    useRootNavigator: false,
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
            if (trimmed.isEmpty) return context.l10n.commonRequired;
            final parsed = int.tryParse(trimmed);
            if (parsed == null) return context.l10n.commonMustBeNumber;
            return validate(parsed);
          },
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(context.l10n.commonCancel),
        ),
        FilledButton(
          onPressed: () {
            if (formKey.currentState!.validate()) {
              Navigator.of(context).pop(int.parse(controller.text.trim()));
            }
          },
          child: Text(context.l10n.commonApply),
        ),
      ],
    ),
  );
}
