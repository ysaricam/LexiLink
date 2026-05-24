import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_quests/application/admin_quests_cubit.dart';
import 'package:lexilink_app/features/admin_quests/data/admin_quests_repository.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_definition.dart';
import 'package:lexilink_app/features/admin_quests/presentation/quest_definition_form.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Admin quest catalog. Tap a row to edit description / threshold /
/// reward / prerequisite / baseline; FAB to create a new definition.
/// The list reloads after each mutation.
class AdminQuestsScreen extends StatefulWidget {
  const AdminQuestsScreen({
    super.key,
    this.cubitFactory,
  });

  /// Test seam. Production resolves the persisted admin token store and
  /// wires an [ApiClient] + [AdminQuestsRepository].
  final AdminQuestsCubit Function()? cubitFactory;

  @override
  State<AdminQuestsScreen> createState() => _AdminQuestsScreenState();
}

class _AdminQuestsScreenState extends State<AdminQuestsScreen> {
  AdminQuestsCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _initialize();
  }

  Future<void> _initialize() async {
    final AdminQuestsCubit cubit;
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

  AdminQuestsCubit _buildCubit(TokenStore adminTokenStore) {
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      tokenStore: adminTokenStore,
    );
    return AdminQuestsCubit(
      repository: AdminQuestsRepository(apiClient: apiClient),
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
      child: const _AdminQuestsView(),
    );
  }
}

class _AdminQuestsView extends StatelessWidget {
  const _AdminQuestsView();

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<AdminQuestsCubit, AdminQuestsState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null,
      listener: (context, state) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(state.errorMessage!)),
        );
      },
      builder: (context, state) {
        return Stack(
          children: [
            _buildBody(context, state),
            if (state.status == AdminQuestsStatus.saving)
              const Positioned.fill(
                child: ColoredBox(
                  color: Color(0x33000000),
                  child: Center(child: CircularProgressIndicator()),
                ),
              ),
            Positioned(
              right: 24,
              bottom: 24,
              child: FloatingActionButton.extended(
                onPressed: () => _showCreateDialog(context),
                icon: const Icon(Icons.add),
                label: const Text('Yeni quest'),
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildBody(BuildContext context, AdminQuestsState state) {
    if (state.status == AdminQuestsStatus.initial ||
        state.status == AdminQuestsStatus.loading &&
            state.definitions.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }
    if (state.status == AdminQuestsStatus.failure &&
        state.definitions.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Text(state.errorMessage ?? 'Quest tanımları yüklenemedi.'),
        ),
      );
    }
    if (state.definitions.isEmpty) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: Text(
            'Henüz quest tanımı yok. "Yeni quest" ile başlat.',
          ),
        ),
      );
    }
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(24, 24, 24, 96),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Quest tanımları',
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          const SizedBox(height: 12),
          Card(
            child: Column(
              children: [
                for (var i = 0; i < state.definitions.length; i++) ...[
                  _QuestRow(
                    definition: state.definitions[i],
                    allDefinitions: state.definitions,
                  ),
                  if (i < state.definitions.length - 1)
                    const Divider(height: 0),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _showCreateDialog(BuildContext context) async {
    final cubit = context.read<AdminQuestsCubit>();
    final activeDefinitions = cubit.state.definitions
        .where((d) => d.isActive)
        .toList(growable: false);
    final result = await showDialog<QuestDefinitionFormResult>(
      context: context,
      useRootNavigator: false,
      builder: (_) => QuestDefinitionFormDialog(
        availablePrerequisites: activeDefinitions,
      ),
    );
    if (result == null) return;
    await cubit.create(
      name: result.name,
      description: result.description,
      trigger: result.trigger,
      threshold: result.threshold,
      reward: result.reward,
      progressBaseline: result.progressBaseline,
      prerequisiteQuestDefinitionId: result.prerequisiteQuestDefinitionId,
    );
  }
}

class _QuestRow extends StatelessWidget {
  const _QuestRow({
    required this.definition,
    required this.allDefinitions,
  });

  final QuestDefinition definition;
  final List<QuestDefinition> allDefinitions;

  @override
  Widget build(BuildContext context) {
    final prereqName = definition.prerequisiteQuestDefinitionId == null
        ? null
        : allDefinitions
            .where((d) => d.id == definition.prerequisiteQuestDefinitionId)
            .map((d) => d.name)
            .firstOrNull;
    final subtitleParts = <String>[
      '${definition.trigger.displayLabel} · ${definition.threshold}',
      if (prereqName != null) 'Ön koşul: $prereqName',
      if (definition.description.isNotEmpty) definition.description,
    ];
    return ListTile(
      title: Row(
        children: [
          Expanded(child: Text(definition.name)),
          _RewardBadge(reward: definition.reward),
          const SizedBox(width: 8),
          if (!definition.isActive) const _DeactivatedBadge(),
        ],
      ),
      subtitle: Text(subtitleParts.join(' · ')),
      isThreeLine: definition.description.isNotEmpty,
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          IconButton(
            tooltip: 'Düzenle',
            icon: const Icon(Icons.edit_outlined),
            onPressed: () => _edit(context),
          ),
          if (definition.isActive)
            IconButton(
              tooltip: 'Deaktive et',
              icon: const Icon(Icons.block_flipped),
              onPressed: () => _deactivate(context),
            )
          else
            IconButton(
              tooltip: 'Tekrar aktive et',
              icon: const Icon(Icons.power_settings_new),
              onPressed: () => _reactivate(context),
            ),
        ],
      ),
    );
  }

  Future<void> _edit(BuildContext context) async {
    final cubit = context.read<AdminQuestsCubit>();
    final activeDefinitions = cubit.state.definitions
        .where((d) => d.isActive)
        .toList(growable: false);
    final result = await showDialog<QuestDefinitionFormResult>(
      context: context,
      useRootNavigator: false,
      builder: (_) => QuestDefinitionFormDialog(
        initial: definition,
        availablePrerequisites: activeDefinitions,
      ),
    );
    if (result == null) return;
    await cubit.update(
      id: definition.id,
      description: result.description,
      threshold: result.threshold,
      reward: result.reward,
      progressBaseline: result.progressBaseline,
      prerequisiteQuestDefinitionId: result.prerequisiteQuestDefinitionId,
    );
  }

  Future<void> _deactivate(BuildContext context) async {
    final cubit = context.read<AdminQuestsCubit>();
    final confirmed = await showDialog<bool>(
      context: context,
      useRootNavigator: false,
      builder: (_) => AlertDialog(
        title: const Text('Quest tanımı deaktive edilsin mi?'),
        content: Text(
          '"${definition.name}" artık player\'lara verilmeyecek. '
          'Mevcut player ilerlemesi etkilenmez.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('İptal'),
          ),
          FilledButton.tonal(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('Deaktive et'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    await cubit.deactivate(definition.id);
  }

  Future<void> _reactivate(BuildContext context) async {
    await context.read<AdminQuestsCubit>().reactivate(definition.id);
  }
}

class _RewardBadge extends StatelessWidget {
  const _RewardBadge({required this.reward});
  final int reward;
  @override
  Widget build(BuildContext context) {
    return _Badge(label: '+$reward⚡', color: Theme.of(context).colorScheme.primary);
  }
}

class _DeactivatedBadge extends StatelessWidget {
  const _DeactivatedBadge();
  @override
  Widget build(BuildContext context) {
    return _Badge(label: 'Inactive', color: Colors.grey.shade600);
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
      child: Text(
        label,
        style: TextStyle(color: color, fontSize: 12),
      ),
    );
  }
}
