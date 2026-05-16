import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/quests/application/quests_cubit.dart';
import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/features/quests/data/quest_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class QuestsScreen extends StatefulWidget {
  const QuestsScreen({super.key});

  @override
  State<QuestsScreen> createState() => _QuestsScreenState();
}

class _QuestsScreenState extends State<QuestsScreen> {
  late final Future<SharedPreferencesTokenStore> _tokenStoreFuture;

  @override
  void initState() {
    super.initState();
    _tokenStoreFuture = SharedPreferencesTokenStore.create();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<TokenStore>(
      future: _tokenStoreFuture,
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return const AppScreen(
            child: AppErrorState(
              title: 'Session storage failed',
              message: 'Restart the app and try again.',
            ),
          );
        }

        final tokenStore = snapshot.data;
        if (tokenStore == null) {
          return const AppScreen(
            child: AppLoadingState(message: 'Preparing session...'),
          );
        }

        return _QuestsProviders(tokenStore: tokenStore);
      },
    );
  }
}

class _QuestsProviders extends StatefulWidget {
  const _QuestsProviders({required this.tokenStore});

  final TokenStore tokenStore;

  @override
  State<_QuestsProviders> createState() => _QuestsProvidersState();
}

class _QuestsProvidersState extends State<_QuestsProviders> {
  late final http.Client _httpClient;
  late final QuestsCubit _questsCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );
    _questsCubit = QuestsCubit(
      questRepository: QuestRepository(apiClient: apiClient),
    );
    unawaited(_questsCubit.loadQuests());
  }

  @override
  void dispose() {
    _questsCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: _questsCubit,
      child: const _QuestsView(),
    );
  }
}

class _QuestsView extends StatelessWidget {
  const _QuestsView();

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      child: BlocConsumer<QuestsCubit, QuestsState>(
        listenWhen: (previous, current) =>
            previous.claimMessage != current.claimMessage &&
            current.claimMessage != null,
        listener: (context, state) {
          final message = state.claimMessage;
          if (message == null) {
            return;
          }
          ScaffoldMessenger.of(context)
            ..clearSnackBars()
            ..showSnackBar(SnackBar(content: Text(message)));
          context.read<QuestsCubit>().clearClaimMessage();
        },
        builder: (context, state) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const AppBackBar(title: 'Quests'),
              const SizedBox(height: 8),
              Text(
                'Complete daily and account quests to earn bonus energy.',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 20),
              if (state.isLoading && state.quests.isEmpty)
                const AppLoadingState(message: 'Loading quests...')
              else if (state.status == QuestsStatus.failure)
                AppErrorState(
                  title: 'Could not load quests',
                  message: state.message ?? 'Try again.',
                  onRetry: () => context.read<QuestsCubit>().loadQuests(),
                )
              else if (state.status == QuestsStatus.success &&
                  state.quests.isEmpty)
                AppEmptyState(
                  title: 'No quests yet',
                  message:
                      'Complete a game to unlock daily and account quests.',
                  actionLabel: 'Refresh',
                  onAction: () => context.read<QuestsCubit>().loadQuests(),
                )
              else
                _QuestsList(quests: state.quests, claimingId: state.claimingId),
            ],
          );
        },
      ),
    );
  }
}

class _QuestsList extends StatelessWidget {
  const _QuestsList({
    required this.quests,
    required this.claimingId,
  });

  final List<PlayerQuest> quests;
  final String? claimingId;

  @override
  Widget build(BuildContext context) {
    final sorted = [...quests]..sort(_questOrdering);

    return Column(
      children: [
        for (var index = 0; index < sorted.length; index++) ...[
          _QuestTile(
            quest: sorted[index],
            isClaiming: claimingId == sorted[index].id,
            claimDisabled: claimingId != null && claimingId != sorted[index].id,
          ),
          if (index < sorted.length - 1) const SizedBox(height: 10),
        ],
      ],
    );
  }

  static int _questOrdering(PlayerQuest a, PlayerQuest b) {
    int rank(PlayerQuest q) {
      switch (q.state) {
        case QuestState.readyToClaim:
          return 0;
        case QuestState.active:
          return 1;
        case QuestState.claimed:
          return 2;
        case QuestState.expired:
          return 3;
        case QuestState.unknown:
          return 4;
      }
    }

    final byRank = rank(a) - rank(b);
    if (byRank != 0) {
      return byRank;
    }
    return b.issuedAt.compareTo(a.issuedAt);
  }
}

class _QuestTile extends StatelessWidget {
  const _QuestTile({
    required this.quest,
    required this.isClaiming,
    required this.claimDisabled,
  });

  final PlayerQuest quest;
  final bool isClaiming;
  final bool claimDisabled;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;
    final progressFraction = quest.goal == 0
        ? 0.0
        : (quest.progress / quest.goal).clamp(0.0, 1.0);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: colorScheme.outline.withValues(alpha: 0.28)),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    _humanQuestType(quest.questType),
                    style: textTheme.titleMedium,
                  ),
                ),
                _StateBadge(state: quest.state),
              ],
            ),
            const SizedBox(height: 10),
            ClipRRect(
              borderRadius: BorderRadius.circular(999),
              child: LinearProgressIndicator(
                value: progressFraction,
                minHeight: 6,
                backgroundColor:
                    colorScheme.outline.withValues(alpha: 0.18),
                color: colorScheme.primary,
              ),
            ),
            const SizedBox(height: 8),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  '${quest.progress}/${quest.goal}',
                  style: textTheme.bodySmall?.copyWith(
                    color: colorScheme.onSurfaceVariant,
                  ),
                ),
                Text(
                  '+${quest.rewardAmount}⚡',
                  style: textTheme.labelLarge?.copyWith(
                    color: colorScheme.primary,
                  ),
                ),
              ],
            ),
            if (quest.isReadyToClaim) ...[
              const SizedBox(height: 12),
              AppPrimaryButton(
                label: isClaiming ? 'Claiming...' : 'Claim reward',
                onPressed: (isClaiming || claimDisabled)
                    ? null
                    : () => context.read<QuestsCubit>().claim(quest.id),
              ),
            ],
          ],
        ),
      ),
    );
  }

  static String _humanQuestType(String raw) {
    switch (raw) {
      case 'FirstGameCompleted':
        return 'Complete your first game';
      case 'ThreeGamesCompleted':
        return 'Complete three games';
      case 'AccountLinked':
        return 'Link an account';
      case 'DailyThreeGames':
        return 'Complete three games today';
      default:
        return raw;
    }
  }
}

class _StateBadge extends StatelessWidget {
  const _StateBadge({required this.state});

  final QuestState state;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    final (label, foreground, background) = switch (state) {
      QuestState.readyToClaim => (
        'Ready',
        colorScheme.onPrimary,
        colorScheme.primary,
      ),
      QuestState.active => (
        'Active',
        colorScheme.onSurface,
        colorScheme.surfaceContainerHighest,
      ),
      QuestState.claimed => (
        'Claimed',
        colorScheme.onSecondaryContainer,
        colorScheme.secondaryContainer,
      ),
      QuestState.expired => (
        'Expired',
        colorScheme.onSurfaceVariant,
        colorScheme.surfaceContainerLow,
      ),
      QuestState.unknown => (
        '—',
        colorScheme.onSurfaceVariant,
        colorScheme.surfaceContainerLow,
      ),
    };

    return DecoratedBox(
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
        child: Text(
          label,
          style: textTheme.labelSmall?.copyWith(color: foreground),
        ),
      ),
    );
  }
}
