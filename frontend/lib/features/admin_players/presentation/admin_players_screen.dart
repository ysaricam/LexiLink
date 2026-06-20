import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_players/application/admin_players_cubit.dart';
import 'package:lexilink_app/features/admin_players/data/admin_players_repository.dart';
import 'package:lexilink_app/features/admin_players/data/player_admin_detail.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Admin player console. Lookup is by the public player handle
/// (`DisplayName#1234`); mutations still use the stable player id returned
/// by the detail endpoint.
class AdminPlayersScreen extends StatefulWidget {
  const AdminPlayersScreen({super.key, this.cubitFactory});

  /// Test seam. Production resolves the persisted admin token store
  /// and wires an [ApiClient] + [AdminPlayersRepository].
  final AdminPlayersCubit Function()? cubitFactory;

  @override
  State<AdminPlayersScreen> createState() => _AdminPlayersScreenState();
}

class _AdminPlayersScreenState extends State<AdminPlayersScreen> {
  AdminPlayersCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminPlayersCubit cubit;
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

  AdminPlayersCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminPlayersCubit(
      repository: AdminPlayersRepository(apiClient: apiClient),
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
    return BlocProvider.value(value: _cubit!, child: const _AdminPlayersView());
  }
}

class _AdminPlayersView extends StatefulWidget {
  const _AdminPlayersView();

  @override
  State<_AdminPlayersView> createState() => _AdminPlayersViewState();
}

class _AdminPlayersViewState extends State<_AdminPlayersView> {
  late final TextEditingController _handleController;

  @override
  void initState() {
    super.initState();
    _handleController = TextEditingController();
  }

  @override
  void dispose() {
    _handleController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<AdminPlayersCubit, AdminPlayersState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminPlayersStatus.failure,
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
                _buildHeader(context),
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

  Widget _buildHeader(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          context.l10n.adminPlayerConsoleTitle,
          style: Theme.of(context).textTheme.headlineSmall,
        ),
        const SizedBox(height: 4),
        Text(
          context.l10n.adminPlayerConsoleHelp,
          style: Theme.of(context).textTheme.bodySmall,
        ),
      ],
    );
  }

  Widget _buildLookupRow(BuildContext context, AdminPlayersState state) {
    final busy =
        state.status == AdminPlayersStatus.loading ||
        state.status == AdminPlayersStatus.saving;
    return LayoutBuilder(
      builder: (context, constraints) {
        final fieldWidth = constraints.maxWidth < 520
            ? constraints.maxWidth
            : 420.0;
        return Wrap(
          spacing: 12,
          runSpacing: 12,
          crossAxisAlignment: WrapCrossAlignment.center,
          children: [
            SizedBox(
              width: fieldWidth,
              child: TextField(
                controller: _handleController,
                enabled: !busy,
                decoration: InputDecoration(
                  labelText: context.l10n.adminPlayerHandle,
                  hintText: 'Yasin#0042',
                  border: const OutlineInputBorder(),
                  isDense: true,
                ),
                onSubmitted: busy ? null : (_) => _submit(context),
              ),
            ),
            FilledButton.icon(
              onPressed: busy ? null : () => _submit(context),
              icon: const Icon(Icons.search),
              label: Text(context.l10n.adminLookUp),
            ),
          ],
        );
      },
    );
  }

  Widget _buildBody(BuildContext context, AdminPlayersState state) {
    return switch (state.status) {
      AdminPlayersStatus.initial => const SizedBox.shrink(),
      AdminPlayersStatus.loading || AdminPlayersStatus.saving => const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        ),
      ),
      AdminPlayersStatus.notFound => Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? context.l10n.adminNoPlayerFound,
          style: Theme.of(context).textTheme.bodyMedium,
        ),
      ),
      AdminPlayersStatus.failure when state.detail == null => Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? context.l10n.adminLookupFailed,
          style: TextStyle(color: Theme.of(context).colorScheme.error),
        ),
      ),
      AdminPlayersStatus.failure ||
      AdminPlayersStatus.loaded => _PlayerDetailCard(detail: state.detail!),
    };
  }

  void _submit(BuildContext context) {
    final handle = _handleController.text.trim();
    if (handle.isEmpty) return;
    context.read<AdminPlayersCubit>().lookup(handle);
  }
}

class _PlayerDetailCard extends StatelessWidget {
  const _PlayerDetailCard({required this.detail});

  final PlayerAdminDetail detail;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CircleAvatar(
                  radius: 24,
                  backgroundImage: detail.avatarUrl == null
                      ? null
                      : NetworkImage(detail.avatarUrl!),
                  child: detail.avatarUrl == null
                      ? const Icon(Icons.person)
                      : null,
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              detail.displayName,
                              style: Theme.of(context).textTheme.titleLarge,
                            ),
                          ),
                          if (detail.isBanned) const _BannedBadge(),
                          if (detail.isGuest) const _GuestBadge(),
                        ],
                      ),
                      const SizedBox(height: 4),
                      SelectableText(
                        detail.handle,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const Divider(height: 32),
            _kv(context, context.l10n.adminId, detail.id),
            _kv(context, context.l10n.adminLocale, detail.locale),
            _kv(
              context,
              context.l10n.adminAuthProvidersLinked,
              detail.authProvidersLinked.toString(),
            ),
            _kv(
              context,
              context.l10n.adminCreated,
              detail.createdAt.toUtc().toIso8601String(),
            ),
            if (detail.isBanned) ...[
              const Divider(height: 32),
              _kv(
                context,
                context.l10n.adminBannedAt,
                detail.bannedAt?.toUtc().toIso8601String() ??
                    context.l10n.commonNoDash,
              ),
              _kv(
                context,
                context.l10n.adminReason,
                detail.bannedReason ?? context.l10n.commonNoDash,
              ),
            ],
            const SizedBox(height: 16),
            Align(
              alignment: Alignment.centerRight,
              child: detail.isBanned
                  ? FilledButton.tonalIcon(
                      onPressed: () => _confirmUnban(context),
                      icon: const Icon(Icons.lock_open),
                      label: Text(context.l10n.adminUnban),
                    )
                  : FilledButton.icon(
                      style: FilledButton.styleFrom(
                        backgroundColor: Theme.of(context).colorScheme.error,
                      ),
                      onPressed: () => _confirmBan(context),
                      icon: const Icon(Icons.block),
                      label: Text(context.l10n.adminBan),
                    ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _kv(BuildContext context, String key, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 160,
            child: Text(
              key,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
          Expanded(child: SelectableText(value)),
        ],
      ),
    );
  }

  Future<void> _confirmBan(BuildContext context) async {
    final cubit = context.read<AdminPlayersCubit>();
    final reason = await showDialog<String>(
      context: context,
      useRootNavigator: false,
      builder: (_) => const _BanReasonDialog(),
    );
    if (reason == null || reason.trim().isEmpty) return;
    await cubit.ban(reason: reason.trim());
  }

  Future<void> _confirmUnban(BuildContext context) async {
    final cubit = context.read<AdminPlayersCubit>();
    final confirmed = await showDialog<bool>(
      context: context,
      useRootNavigator: false,
      builder: (_) => AlertDialog(
        title: Text(context.l10n.adminUnbanPlayerTitle),
        content: Text(
          context.l10n.adminUnbanPlayerMessage(detail.handle),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(context.l10n.commonCancel),
          ),
          FilledButton.tonal(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(context.l10n.adminUnban),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    await cubit.unban();
  }
}

class _BanReasonDialog extends StatefulWidget {
  const _BanReasonDialog();

  @override
  State<_BanReasonDialog> createState() => _BanReasonDialogState();
}

class _BanReasonDialogState extends State<_BanReasonDialog> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(context.l10n.adminBanPlayerTitle),
      content: TextField(
        controller: _controller,
        autofocus: true,
        decoration: InputDecoration(
          labelText: context.l10n.adminReason,
          border: const OutlineInputBorder(),
        ),
        maxLines: 3,
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(context.l10n.commonCancel),
        ),
        FilledButton(
          onPressed: () => Navigator.of(context).pop(_controller.text),
          child: Text(context.l10n.adminBan),
        ),
      ],
    );
  }
}

class _BannedBadge extends StatelessWidget {
  const _BannedBadge();
  @override
  Widget build(BuildContext context) {
    return _Badge(
      label: context.l10n.adminBanned,
      color: Theme.of(context).colorScheme.error,
    );
  }
}

class _GuestBadge extends StatelessWidget {
  const _GuestBadge();
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(left: 8),
      child: _Badge(label: context.l10n.adminGuest, color: Colors.teal),
    );
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
