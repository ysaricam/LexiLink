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
                'Quest tamamla, bonus enerji kazan.',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 20),
              if (state.isLoading && state.quests.isEmpty)
                const AppLoadingState(message: 'Quest\'ler yükleniyor...')
              else if (state.status == QuestsStatus.failure)
                AppErrorState(
                  title: 'Quest\'ler yüklenemedi',
                  message: state.message ?? 'Tekrar dene.',
                  onRetry: () => context.read<QuestsCubit>().loadQuests(),
                )
              else if (state.status == QuestsStatus.success &&
                  state.quests.isEmpty)
                AppEmptyState(
                  title: 'Henüz quest yok',
                  message:
                      'Bir oyun tamamla, quest\'ler burada görünmeye başlasın.',
                  actionLabel: 'Yenile',
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
        case QuestState.unknown:
          return 3;
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
    final progressFraction = quest.threshold == 0
        ? 0.0
        : (quest.progress / quest.threshold).clamp(0.0, 1.0);

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
                    quest.name,
                    style: textTheme.titleMedium,
                  ),
                ),
                _StateBadge(state: quest.state),
              ],
            ),
            if (quest.description.isNotEmpty) ...[
              const SizedBox(height: 4),
              Text(
                quest.description,
                style: textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                ),
              ),
            ],
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
                  '${quest.progress}/${quest.threshold}',
                  style: textTheme.bodySmall?.copyWith(
                    color: colorScheme.onSurfaceVariant,
                  ),
                ),
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    if (quest.energyReward > 0)
                      Text(
                        '+${quest.energyReward}⚡',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.primary,
                        ),
                      ),
                    if (quest.energyReward > 0 && quest.hintReward > 0)
                      const SizedBox(width: 8),
                    if (quest.hintReward > 0)
                      Text(
                        '+${quest.hintReward}💡',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.tertiary,
                        ),
                      ),
                  ],
                ),
              ],
            ),
            if (quest.isReadyToClaim) ...[
              const SizedBox(height: 12),
              AppPrimaryButton(
                label: isClaiming ? 'Alınıyor...' : 'Ödülü al',
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
        'Hazır',
        colorScheme.onPrimary,
        colorScheme.primary,
      ),
      QuestState.active => (
        'Aktif',
        colorScheme.onSurface,
        colorScheme.surfaceContainerHighest,
      ),
      QuestState.claimed => (
        'Alındı',
        colorScheme.onSecondaryContainer,
        colorScheme.secondaryContainer,
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
