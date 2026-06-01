import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_diamond/application/admin_diamond_cubit.dart';
import 'package:lexilink_app/features/admin_diamond/data/admin_diamond_repository.dart';
import 'package:lexilink_app/features/admin_diamond/data/player_diamond_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class AdminDiamondScreen extends StatefulWidget {
  const AdminDiamondScreen({super.key, this.cubitFactory});

  final AdminDiamondCubit Function()? cubitFactory;

  @override
  State<AdminDiamondScreen> createState() => _AdminDiamondScreenState();
}

class _AdminDiamondScreenState extends State<AdminDiamondScreen> {
  AdminDiamondCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminDiamondCubit cubit;
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

  AdminDiamondCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminDiamondCubit(
      repository: AdminDiamondRepository(apiClient: apiClient),
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
    return BlocProvider.value(
      value: _cubit!,
      child: const _AdminDiamondView(),
    );
  }
}

class _AdminDiamondView extends StatefulWidget {
  const _AdminDiamondView();

  @override
  State<_AdminDiamondView> createState() => _AdminDiamondViewState();
}

class _AdminDiamondViewState extends State<_AdminDiamondView> {
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
    return BlocConsumer<AdminDiamondCubit, AdminDiamondState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminDiamondStatus.failure,
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
                  context.l10n.adminDiamondConsoleTitle,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  context.l10n.adminDiamondConsoleHelp,
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

  Widget _buildLookupRow(BuildContext context, AdminDiamondState state) {
    final busy =
        state.status == AdminDiamondStatus.loading ||
        state.status == AdminDiamondStatus.saving;
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

  Widget _buildBody(BuildContext context, AdminDiamondState state) {
    if (state.snapshot != null) {
      return Stack(
        children: [
          _DiamondCard(snapshot: state.snapshot!),
          if (state.status == AdminDiamondStatus.saving)
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
      AdminDiamondStatus.initial => const SizedBox.shrink(),
      AdminDiamondStatus.loading => const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        ),
      ),
      AdminDiamondStatus.notFound => Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? context.l10n.adminNoDiamondInventory,
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
    context.read<AdminDiamondCubit>().load(id);
  }
}

class _DiamondCard extends StatelessWidget {
  const _DiamondCard({required this.snapshot});

  final PlayerDiamondSnapshot snapshot;

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
                  Icons.diamond_outlined,
                  size: 28,
                  color: Theme.of(context).colorScheme.primary,
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
                  label: Text(context.l10n.adminGrantDiamonds),
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
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 160,
          child: Text(key, style: Theme.of(context).textTheme.bodySmall),
        ),
        Expanded(child: SelectableText(value)),
      ],
    );
  }

  Future<void> _promptSet(BuildContext context) async {
    final value = await _numberDialog(
      context,
      title: context.l10n.adminSetDiamondBalanceTitle,
      label: context.l10n.adminBalance,
      initial: snapshot.balance,
    );
    if (value == null || !context.mounted) return;
    await context.read<AdminDiamondCubit>().setBalance(value);
  }

  Future<void> _promptGrant(BuildContext context) async {
    final value = await _numberDialog(
      context,
      title: context.l10n.adminGrantDiamonds,
      label: context.l10n.adminAmount,
      min: 1,
    );
    if (value == null || !context.mounted) return;
    await context.read<AdminDiamondCubit>().grant(value);
  }

  Future<void> _confirmReset(BuildContext context) async {
    final confirmed = await showDialog<bool>(
      context: context,
      useRootNavigator: false,
      builder: (_) => AlertDialog(
        title: Text(context.l10n.adminResetDiamondTitle),
        content: Text(context.l10n.adminResetDiamondMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(context.l10n.commonCancel),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(context.l10n.commonReset),
          ),
        ],
      ),
    );
    if (confirmed != true || !context.mounted) return;
    await context.read<AdminDiamondCubit>().reset();
  }

  Future<int?> _numberDialog(
    BuildContext context, {
    required String title,
    required String label,
    int initial = 0,
    int min = 0,
  }) async {
    return showDialog<int>(
      context: context,
      useRootNavigator: false,
      builder: (_) => _DiamondNumberDialog(
        title: title,
        label: label,
        initial: initial,
        min: min,
      ),
    );
  }
}

class _DiamondNumberDialog extends StatefulWidget {
  const _DiamondNumberDialog({
    required this.title,
    required this.label,
    required this.initial,
    required this.min,
  });

  final String title;
  final String label;
  final int initial;
  final int min;

  @override
  State<_DiamondNumberDialog> createState() => _DiamondNumberDialogState();
}

class _DiamondNumberDialogState extends State<_DiamondNumberDialog> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: '${widget.initial}');
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.title),
      content: TextField(
        controller: _controller,
        keyboardType: TextInputType.number,
        decoration: InputDecoration(
          labelText: widget.label,
          border: const OutlineInputBorder(),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(context.l10n.commonCancel),
        ),
        FilledButton(
          onPressed: () {
            final parsed = int.tryParse(_controller.text.trim());
            if (parsed == null || parsed < widget.min) return;
            Navigator.of(context).pop(parsed);
          },
          child: Text(context.l10n.commonApply),
        ),
      ],
    );
  }
}
