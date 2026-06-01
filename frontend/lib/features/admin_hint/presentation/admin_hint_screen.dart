import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_hint/application/admin_hint_cubit.dart';
import 'package:lexilink_app/features/admin_hint/data/admin_hint_repository.dart';
import 'package:lexilink_app/features/admin_hint/data/player_hint_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class AdminHintScreen extends StatefulWidget {
  const AdminHintScreen({super.key, this.cubitFactory});

  final AdminHintCubit Function()? cubitFactory;

  @override
  State<AdminHintScreen> createState() => _AdminHintScreenState();
}

class _AdminHintScreenState extends State<AdminHintScreen> {
  AdminHintCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminHintCubit cubit;
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

  AdminHintCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminHintCubit(
      repository: AdminHintRepository(apiClient: apiClient),
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
    return BlocProvider.value(value: _cubit!, child: const _AdminHintView());
  }
}

class _AdminHintView extends StatefulWidget {
  const _AdminHintView();

  @override
  State<_AdminHintView> createState() => _AdminHintViewState();
}

class _AdminHintViewState extends State<_AdminHintView> {
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
    return BlocConsumer<AdminHintCubit, AdminHintState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminHintStatus.failure,
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
                  context.l10n.adminHintConsoleTitle,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  context.l10n.adminHintConsoleHelp,
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

  Widget _buildLookupRow(BuildContext context, AdminHintState state) {
    final busy =
        state.status == AdminHintStatus.loading ||
        state.status == AdminHintStatus.saving;
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

  Widget _buildBody(BuildContext context, AdminHintState state) {
    if (state.snapshot != null) {
      return Stack(
        children: [
          _HintCard(snapshot: state.snapshot!),
          if (state.status == AdminHintStatus.saving)
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
      AdminHintStatus.initial => const SizedBox.shrink(),
      AdminHintStatus.loading => const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        ),
      ),
      AdminHintStatus.notFound => Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? context.l10n.adminNoHintInventory,
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
    context.read<AdminHintCubit>().load(id);
  }
}

class _HintCard extends StatelessWidget {
  const _HintCard({required this.snapshot});

  final PlayerHintSnapshot snapshot;

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
                  Icons.lightbulb_outline,
                  size: 28,
                  color: Theme.of(context).colorScheme.tertiary,
                ),
                const SizedBox(width: 8),
                Text(
                  '${snapshot.balance}',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
              ],
            ),
            const SizedBox(height: 12),
            _kv(context, context.l10n.adminPlayerId, snapshot.playerId),
            const SizedBox(height: 20),
            Wrap(
              spacing: 12,
              runSpacing: 12,
              children: [
                FilledButton.icon(
                  onPressed: () => _promptSet(context),
                  icon: const Icon(Icons.edit),
                  label: Text(context.l10n.adminSetBalance),
                ),
                FilledButton.tonalIcon(
                  onPressed: () => _promptGrant(context),
                  icon: const Icon(Icons.add),
                  label: Text(context.l10n.adminGrantHints),
                ),
                OutlinedButton.icon(
                  onPressed: () => _confirmReset(context),
                  icon: const Icon(Icons.refresh),
                  label: Text(context.l10n.adminResetToZero),
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
    final cubit = context.read<AdminHintCubit>();
    final value = await _intDialog(
      context,
      title: context.l10n.adminSetHintBalanceTitle,
      label: context.l10n.adminNewBalance,
      helper: context.l10n.adminSetHintHelper,
      validate: (v) => v < 0 ? context.l10n.commonNonNegative : null,
    );
    if (value == null) return;
    await cubit.setBalance(value);
  }

  Future<void> _promptGrant(BuildContext context) async {
    final cubit = context.read<AdminHintCubit>();
    final value = await _intDialog(
      context,
      title: context.l10n.adminGrantHints,
      label: context.l10n.adminHintAmount,
      helper: context.l10n.adminGrantHintHelper,
      validate: (v) => v <= 0 ? context.l10n.commonGreaterThanZero : null,
    );
    if (value == null) return;
    await cubit.grant(value);
  }

  Future<void> _confirmReset(BuildContext context) async {
    final cubit = context.read<AdminHintCubit>();
    final confirmed = await showDialog<bool>(
      context: context,
      useRootNavigator: false,
      builder: (_) => AlertDialog(
        title: Text(context.l10n.adminResetHintTitle),
        content: Text(context.l10n.adminResetHintMessage),
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
