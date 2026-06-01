import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_quests/application/admin_quests_cubit.dart';
import 'package:lexilink_app/features/admin_quests/data/admin_quests_repository.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_definition.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';
import 'package:lexilink_app/features/admin_quests/presentation/quest_definition_form.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
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
          prev.errorMessage != curr.errorMessage && curr.errorMessage != null,
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
                label: Text(context.l10n.adminNewQuest),
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
          child: Text(state.errorMessage ?? context.l10n.adminQuestLoadError),
        ),
      );
    }
    if (state.definitions.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Text(
            context.l10n.adminNoQuestDefinitions,
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
            context.l10n.adminQuestsTitle,
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
      energyReward: result.energyReward,
      hintReward: result.hintReward,
      undoReward: result.undoReward,
      resetReward: result.resetReward,
      diamondReward: result.diamondReward,
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
      '${_triggerLabel(context, definition.trigger)} · ${definition.threshold}',
      if (prereqName != null) context.l10n.adminQuestPrerequisite(prereqName),
      if (definition.description.isNotEmpty) definition.description,
    ];
    return ListTile(
      title: Row(
        children: [
          Expanded(child: Text(definition.name)),
          if (definition.energyReward > 0) ...[
            _RewardBadge(
              label: '+${definition.energyReward}⚡',
              color: Theme.of(context).colorScheme.primary,
            ),
            const SizedBox(width: 4),
          ],
          if (definition.hintReward > 0) ...[
            _RewardBadge(
              label: '+${definition.hintReward}💡',
              color: Theme.of(context).colorScheme.tertiary,
            ),
            const SizedBox(width: 4),
          ],
          if (definition.undoReward > 0) ...[
            _RewardBadge(
              label: '+${definition.undoReward}↶',
              color: Theme.of(context).colorScheme.secondary,
            ),
            const SizedBox(width: 4),
          ],
          if (definition.resetReward > 0) ...[
            _RewardBadge(
              label: '+${definition.resetReward}↻',
              color: Theme.of(context).colorScheme.error,
            ),
            const SizedBox(width: 4),
          ],
          if (definition.diamondReward > 0) ...[
            _RewardBadge(
              label: '+${definition.diamondReward}💎',
              color: Theme.of(context).colorScheme.primary,
            ),
            const SizedBox(width: 4),
          ],
          const SizedBox(width: 4),
          if (!definition.isActive) const _DeactivatedBadge(),
        ],
      ),
      subtitle: Text(subtitleParts.join(' · ')),
      isThreeLine: definition.description.isNotEmpty,
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          IconButton(
            tooltip: context.l10n.adminQuestEditTooltip,
            icon: const Icon(Icons.edit_outlined),
            onPressed: () => _edit(context),
          ),
          if (definition.isActive)
            IconButton(
              tooltip: context.l10n.adminQuestDeactivateTooltip,
              icon: const Icon(Icons.block_flipped),
              onPressed: () => _deactivate(context),
            )
          else
            IconButton(
              tooltip: context.l10n.adminQuestReactivateTooltip,
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
      energyReward: result.energyReward,
      hintReward: result.hintReward,
      undoReward: result.undoReward,
      resetReward: result.resetReward,
      diamondReward: result.diamondReward,
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
        title: Text(context.l10n.adminQuestDeactivateTitle),
        content: Text(
          context.l10n.adminQuestDeactivateMessage(definition.name),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(context.l10n.commonCancel),
          ),
          FilledButton.tonal(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(context.l10n.adminQuestDeactivateTooltip),
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

String _triggerLabel(BuildContext context, QuestTrigger trigger) =>
    switch (trigger) {
      QuestTrigger.gameCompletedTotal => context.l10n.adminQuestTriggerTotal,
      QuestTrigger.gameCompletedDaily => context.l10n.adminQuestTriggerDaily,
      QuestTrigger.authProviderLinked =>
        context.l10n.adminQuestTriggerAuthProvider,
    };

class _RewardBadge extends StatelessWidget {
  const _RewardBadge({required this.label, required this.color});
  final String label;
  final Color color;
  @override
  Widget build(BuildContext context) {
    return _Badge(label: label, color: color);
  }
}

class _DeactivatedBadge extends StatelessWidget {
  const _DeactivatedBadge();
  @override
  Widget build(BuildContext context) {
    return _Badge(
      label: context.l10n.adminInactive,
      color: Colors.grey.shade600,
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
      child: Text(
        label,
        style: TextStyle(color: color, fontSize: 12),
      ),
    );
  }
}
