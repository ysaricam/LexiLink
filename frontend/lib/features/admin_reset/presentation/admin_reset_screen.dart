import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_reset/application/admin_reset_cubit.dart';
import 'package:lexilink_app/features/admin_reset/data/admin_reset_repository.dart';
import 'package:lexilink_app/features/admin_reset/data/player_reset_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class AdminResetScreen extends StatefulWidget {
  const AdminResetScreen({super.key, this.cubitFactory});

  final AdminResetCubit Function()? cubitFactory;

  @override
  State<AdminResetScreen> createState() => _AdminResetScreenState();
}

class _AdminResetScreenState extends State<AdminResetScreen> {
  AdminResetCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminResetCubit cubit;
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

  AdminResetCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminResetCubit(
      repository: AdminResetRepository(apiClient: apiClient),
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
    return BlocProvider.value(value: _cubit!, child: const _AdminResetView());
  }
}

class _AdminResetView extends StatefulWidget {
  const _AdminResetView();

  @override
  State<_AdminResetView> createState() => _AdminResetViewState();
}

class _AdminResetViewState extends State<_AdminResetView> {
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
    return BlocConsumer<AdminResetCubit, AdminResetState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminResetStatus.failure,
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
                  'Reset console',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  'Lookup by player GUID, then snap / grant / reset. '
                  'Reset inventory has no max cap.',
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

  Widget _buildLookupRow(BuildContext context, AdminResetState state) {
    final busy = state.status == AdminResetStatus.loading ||
        state.status == AdminResetStatus.saving;
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

  Widget _buildBody(BuildContext context, AdminResetState state) {
    if (state.snapshot != null) {
      return Stack(
        children: [
          _ResetCard(snapshot: state.snapshot!),
          if (state.status == AdminResetStatus.saving)
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
      AdminResetStatus.initial => const SizedBox.shrink(),
      AdminResetStatus.loading => const Center(
          child: Padding(
            padding: EdgeInsets.all(24),
            child: CircularProgressIndicator(),
          ),
        ),
      AdminResetStatus.notFound => Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            state.errorMessage ?? 'No reset inventory.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
        ),
      _ => Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            state.errorMessage ?? 'Lookup failed.',
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
    };
  }

  void _submit(BuildContext context) {
    final id = _idController.text.trim();
    if (id.isEmpty) return;
    context.read<AdminResetCubit>().load(id);
  }
}

class _ResetCard extends StatelessWidget {
  const _ResetCard({required this.snapshot});

  final PlayerResetSnapshot snapshot;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(
                  Icons.restart_alt,
                  size: 28,
                  color: Theme.of(context).colorScheme.error,
                ),
                const SizedBox(width: 8),
                Text(
                  '${snapshot.balance}',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
              ],
            ),
            const SizedBox(height: 12),
            _kv(context, 'Player id', snapshot.playerId),
            const SizedBox(height: 20),
            Wrap(
              spacing: 12,
              runSpacing: 12,
              children: [
                FilledButton.icon(
                  onPressed: () => _promptSet(context),
                  icon: const Icon(Icons.edit),
                  label: const Text('Set balance'),
                ),
                FilledButton.tonalIcon(
                  onPressed: () => _promptGrant(context),
                  icon: const Icon(Icons.add),
                  label: const Text('Grant resets'),
                ),
                OutlinedButton.icon(
                  onPressed: () => _confirmReset(context),
                  icon: const Icon(Icons.refresh),
                  label: const Text('Reset to zero'),
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
    final cubit = context.read<AdminResetCubit>();
    final value = await _intDialog(
      context,
      title: 'Set reset balance',
      label: 'New balance',
      helper: "Snaps the player's reset balance to this value (>= 0).",
      validate: (v) => v < 0 ? 'Must be >= 0' : null,
    );
    if (value == null) return;
    await cubit.setBalance(value);
  }

  Future<void> _promptGrant(BuildContext context) async {
    final cubit = context.read<AdminResetCubit>();
    final value = await _intDialog(
      context,
      title: 'Grant resets',
      label: 'Reset amount',
      helper: 'Adds to the existing balance — no max cap.',
      validate: (v) => v <= 0 ? 'Must be greater than 0' : null,
    );
    if (value == null) return;
    await cubit.grant(value);
  }

  Future<void> _confirmReset(BuildContext context) async {
    final cubit = context.read<AdminResetCubit>();
    final confirmed = await showDialog<bool>(
      context: context,
      useRootNavigator: false,
      builder: (_) => AlertDialog(
        title: const Text('Reset reset balance?'),
        content: const Text(
          'Sets the player\'s reset balance to zero.',
        ),
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
