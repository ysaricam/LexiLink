import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_audit/application/admin_audit_cubit.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_action.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_audit_repository.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class AdminAuditScreen extends StatefulWidget {
  const AdminAuditScreen({super.key, this.cubitFactory});

  final AdminAuditCubit Function()? cubitFactory;

  @override
  State<AdminAuditScreen> createState() => _AdminAuditScreenState();
}

class _AdminAuditScreenState extends State<AdminAuditScreen> {
  AdminAuditCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminAuditCubit cubit;
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
      _cubit = cubit..load();
      _initializing = false;
    });
  }

  AdminAuditCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminAuditCubit(
      repository: AdminAuditRepository(apiClient: apiClient),
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
    return BlocProvider.value(value: _cubit!, child: const _AdminAuditView());
  }
}

class _AdminAuditView extends StatefulWidget {
  const _AdminAuditView();

  @override
  State<_AdminAuditView> createState() => _AdminAuditViewState();
}

class _AdminAuditViewState extends State<_AdminAuditView> {
  late final TextEditingController _adminIdController;
  late final TextEditingController _targetTypeController;
  late final TextEditingController _targetIdController;

  @override
  void initState() {
    super.initState();
    _adminIdController = TextEditingController();
    _targetTypeController = TextEditingController();
    _targetIdController = TextEditingController();
  }

  @override
  void dispose() {
    _adminIdController.dispose();
    _targetTypeController.dispose();
    _targetIdController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<AdminAuditCubit, AdminAuditState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null,
      listener: (context, state) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(state.errorMessage!)),
        );
      },
      builder: (context, state) {
        return SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 32),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Audit log',
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              const SizedBox(height: 4),
              Text(
                'Newest first. Filters are optional; page size 50.',
                style: Theme.of(context).textTheme.bodySmall,
              ),
              const SizedBox(height: 16),
              _buildFilterRow(context, state),
              const SizedBox(height: 24),
              _buildBody(context, state),
            ],
          ),
        );
      },
    );
  }

  Widget _buildFilterRow(BuildContext context, AdminAuditState state) {
    final busy = state.status == AdminAuditStatus.loading;
    return Wrap(
      spacing: 12,
      runSpacing: 12,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: [
        SizedBox(
          width: 280,
          child: TextField(
            controller: _adminIdController,
            enabled: !busy,
            decoration: const InputDecoration(
              labelText: 'Admin user id (GUID)',
              border: OutlineInputBorder(),
              isDense: true,
            ),
          ),
        ),
        SizedBox(
          width: 220,
          child: TextField(
            controller: _targetTypeController,
            enabled: !busy,
            decoration: const InputDecoration(
              labelText: 'Target type (e.g. Games.Category)',
              border: OutlineInputBorder(),
              isDense: true,
            ),
          ),
        ),
        SizedBox(
          width: 280,
          child: TextField(
            controller: _targetIdController,
            enabled: !busy,
            decoration: const InputDecoration(
              labelText: 'Target id',
              border: OutlineInputBorder(),
              isDense: true,
            ),
          ),
        ),
        FilledButton.icon(
          onPressed: busy ? null : () => _applyFilters(context),
          icon: const Icon(Icons.filter_alt),
          label: const Text('Apply filters'),
        ),
        TextButton.icon(
          onPressed: busy ? null : () => _clearFilters(context),
          icon: const Icon(Icons.clear),
          label: const Text('Clear'),
        ),
      ],
    );
  }

  Widget _buildBody(BuildContext context, AdminAuditState state) {
    if (state.status == AdminAuditStatus.loading && state.actions.isEmpty) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        ),
      );
    }
    if (state.status == AdminAuditStatus.failure && state.actions.isEmpty) {
      return Padding(
        padding: const EdgeInsets.all(16),
        child: Text(
          state.errorMessage ?? 'Failed to load audit log.',
          style: TextStyle(color: Theme.of(context).colorScheme.error),
        ),
      );
    }
    if (state.actions.isEmpty) {
      return const Padding(
        padding: EdgeInsets.all(24),
        child: Text('No audit entries match the current filters.'),
      );
    }
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Card(
          clipBehavior: Clip.antiAlias,
          child: Column(
            children: [
              for (var i = 0; i < state.actions.length; i++) ...[
                _AuditRow(action: state.actions[i]),
                if (i < state.actions.length - 1) const Divider(height: 0),
              ],
            ],
          ),
        ),
        const SizedBox(height: 16),
        Row(
          children: [
            Text(
              'Offset ${state.offset}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
            const Spacer(),
            OutlinedButton.icon(
              onPressed: state.offset == 0
                  ? null
                  : () => context.read<AdminAuditCubit>().prevPage(),
              icon: const Icon(Icons.chevron_left),
              label: const Text('Prev'),
            ),
            const SizedBox(width: 8),
            OutlinedButton.icon(
              onPressed: state.hasMore
                  ? () => context.read<AdminAuditCubit>().nextPage()
                  : null,
              icon: const Icon(Icons.chevron_right),
              label: const Text('Next'),
            ),
          ],
        ),
      ],
    );
  }

  void _applyFilters(BuildContext context) {
    final filter = AdminAuditFilter(
      adminUserId: _adminIdController.text.trim().isEmpty
          ? null
          : _adminIdController.text.trim(),
      targetType: _targetTypeController.text.trim().isEmpty
          ? null
          : _targetTypeController.text.trim(),
      targetId: _targetIdController.text.trim().isEmpty
          ? null
          : _targetIdController.text.trim(),
    );
    context.read<AdminAuditCubit>().applyFilter(filter);
  }

  void _clearFilters(BuildContext context) {
    _adminIdController.clear();
    _targetTypeController.clear();
    _targetIdController.clear();
    context.read<AdminAuditCubit>().applyFilter(const AdminAuditFilter());
  }
}

class _AuditRow extends StatelessWidget {
  const _AuditRow({required this.action});

  final AdminAction action;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      title: Row(
        children: [
          Expanded(
            child: Text(
              action.actionType,
              style: const TextStyle(fontWeight: FontWeight.w600),
            ),
          ),
          Text(
            action.occurredOn.toUtc().toIso8601String(),
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ],
      ),
      subtitle: Padding(
        padding: const EdgeInsets.only(top: 4),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              action.targetId == null
                  ? action.targetType
                  : '${action.targetType} · ${action.targetId}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
            SelectableText(
              'admin: ${action.adminUserId}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      ),
      trailing: IconButton(
        tooltip: 'View payload',
        icon: const Icon(Icons.code),
        onPressed: () => _showPayload(context),
      ),
    );
  }

  Future<void> _showPayload(BuildContext context) async {
    final pretty = _prettyJson(action.payloadJson);
    await showDialog<void>(
      context: context,
      builder: (_) => AlertDialog(
        title: Text(action.actionType),
        content: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 600, maxHeight: 500),
          child: SingleChildScrollView(
            child: SelectableText(
              pretty,
              style: const TextStyle(fontFamily: 'monospace', fontSize: 12),
            ),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Close'),
          ),
        ],
      ),
    );
  }

  /// Defensive pretty-print: server promises JSON but we don't want a
  /// malformed payload to crash the dialog. Falls back to the raw
  /// string when decoding fails.
  static String _prettyJson(String raw) {
    try {
      final decoded = jsonDecode(raw);
      return const JsonEncoder.withIndent('  ').convert(decoded);
    } on FormatException {
      return raw;
    }
  }
}
