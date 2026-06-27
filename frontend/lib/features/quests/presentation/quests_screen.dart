import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/quests/application/quests_cubit.dart';
import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/features/quests/data/quest_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
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
          return AppScreen(
            child: AppErrorState(
              title: context.l10n.sessionStorageFailedTitle,
              message: context.l10n.sessionStorageFailedMessage,
            ),
          );
        }

        final tokenStore = snapshot.data;
        if (tokenStore == null) {
          return AppScreen(
            child: AppLoadingState(message: context.l10n.preparingSession),
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
      size: AppScreenSize.wide,
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
              AppBackBar(title: context.l10n.navQuests),
              const SizedBox(height: 8),
              Text(
                context.l10n.questsSubtitle,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 18),
              if (state.isLoading && state.quests.isEmpty)
                AppLoadingState(message: context.l10n.questsLoading)
              else if (state.status == QuestsStatus.failure)
                AppErrorState(
                  title: context.l10n.questsLoadError,
                  message: state.message ?? context.l10n.commonTryAgain,
                  onRetry: () => context.read<QuestsCubit>().loadQuests(),
                )
              else if (state.status == QuestsStatus.success &&
                  state.quests.isEmpty)
                AppEmptyState(
                  title: context.l10n.noQuestsTitle,
                  message: context.l10n.noQuestsMessage,
                  actionLabel: context.l10n.commonRefresh,
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

    return LayoutBuilder(
      builder: (context, constraints) {
        final useTwoColumns = constraints.maxWidth >= 720;
        final gap = useTwoColumns ? 12.0 : 10.0;
        final cardWidth = useTwoColumns
            ? (constraints.maxWidth - gap) / 2
            : constraints.maxWidth;

        return Wrap(
          spacing: gap,
          runSpacing: gap,
          children: [
            for (final quest in sorted)
              SizedBox(
                width: cardWidth,
                child: _QuestTile(
                  quest: quest,
                  isClaiming: claimingId == quest.id,
                  claimDisabled: claimingId != null && claimingId != quest.id,
                ),
              ),
          ],
        );
      },
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
    final tone = _toneForState(quest.state, colorScheme);
    final progressFraction = quest.threshold == 0
        ? 0.0
        : (quest.progress / quest.threshold).clamp(0.0, 1.0);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: tone.withValues(alpha: 0.22)),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: AppPalette.primary.withValues(alpha: 0.08),
            blurRadius: 16,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _QuestStateIcon(state: quest.state, color: tone),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        quest.name,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.w800,
                          height: 1.12,
                        ),
                      ),
                      if (quest.description.isNotEmpty) ...[
                        const SizedBox(height: 5),
                        Text(
                          quest.description,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: textTheme.bodySmall?.copyWith(
                            color: colorScheme.onSurfaceVariant,
                            height: 1.25,
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            ClipRRect(
              borderRadius: BorderRadius.circular(999),
              child: LinearProgressIndicator(
                value: progressFraction,
                minHeight: 8,
                backgroundColor: colorScheme.outline.withValues(alpha: 0.14),
                color: tone,
              ),
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Text(
                  '${quest.progress}/${quest.threshold}',
                  style: textTheme.labelMedium?.copyWith(
                    color: colorScheme.onSurfaceVariant,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Align(
                    alignment: Alignment.centerRight,
                    child: _RewardChips(quest: quest),
                  ),
                ),
              ],
            ),
            if (quest.isReadyToClaim) ...[
              const SizedBox(height: 14),
              FilledButton.icon(
                onPressed: (isClaiming || claimDisabled)
                    ? null
                    : () async {
                        final audio = context.read<AudioService>();
                        final cubit = context.read<QuestsCubit>();
                        final claimed = await cubit.claim(quest.id);
                        await audio.playEffect(
                          claimed ? SoundEffect.questClaim : SoundEffect.error,
                        );
                      },
                icon: isClaiming
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.card_giftcard_rounded, size: 18),
                label: Text(
                  isClaiming
                      ? context.l10n.questClaiming
                      : context.l10n.questClaimReward,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Color _toneForState(QuestState state, ColorScheme colorScheme) {
    return switch (state) {
      QuestState.readyToClaim => AppPalette.primary,
      QuestState.active => AppPalette.focus,
      QuestState.claimed => AppPalette.success,
      QuestState.unknown => colorScheme.onSurfaceVariant,
    };
  }
}

class _QuestStateIcon extends StatelessWidget {
  const _QuestStateIcon({required this.state, required this.color});

  final QuestState state;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final icon = switch (state) {
      QuestState.readyToClaim => Icons.card_giftcard_rounded,
      QuestState.active => Icons.flag_rounded,
      QuestState.claimed => Icons.check_circle_rounded,
      QuestState.unknown => Icons.help_rounded,
    };

    return Container(
      width: 46,
      height: 46,
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        border: Border.all(color: color.withValues(alpha: 0.18)),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Icon(icon, color: color, size: 22),
    );
  }
}

class _RewardChips extends StatelessWidget {
  const _RewardChips({required this.quest});

  final PlayerQuest quest;

  @override
  Widget build(BuildContext context) {
    final chips = <_RewardChipData>[
      if (quest.energyReward > 0)
        _RewardChipData('+${quest.energyReward}⚡', AppPalette.focus),
      if (quest.hintReward > 0)
        _RewardChipData('+${quest.hintReward}💡', AppPalette.success),
      if (quest.undoReward > 0)
        _RewardChipData('+${quest.undoReward}↶', AppPalette.primary),
      if (quest.resetReward > 0)
        _RewardChipData('+${quest.resetReward}↻', AppPalette.danger),
      if (quest.diamondReward > 0)
        _RewardChipData('+${quest.diamondReward}💎', AppPalette.primary),
    ];

    if (chips.isEmpty) {
      return const SizedBox.shrink();
    }

    return Wrap(
      alignment: WrapAlignment.end,
      spacing: 6,
      runSpacing: 6,
      children: [
        for (final chip in chips)
          _RewardChip(label: chip.label, color: chip.color),
      ],
    );
  }
}

class _RewardChipData {
  const _RewardChipData(this.label, this.color);

  final String label;
  final Color color;
}

class _RewardChip extends StatelessWidget {
  const _RewardChip({required this.label, required this.color});

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        border: Border.all(color: color.withValues(alpha: 0.16)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 5),
        child: Text(
          label,
          style: Theme.of(context).textTheme.labelMedium?.copyWith(
            color: color,
            fontWeight: FontWeight.w800,
            height: 1,
          ),
        ),
      ),
    );
  }
}
