import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/profile/application/leaderboard_cubit.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_entry.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_query.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class LeaderboardScreen extends StatefulWidget {
  const LeaderboardScreen({super.key});

  @override
  State<LeaderboardScreen> createState() => _LeaderboardScreenState();
}

class _LeaderboardScreenState extends State<LeaderboardScreen> {
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

        return _LeaderboardProviders(tokenStore: tokenStore);
      },
    );
  }
}

class _LeaderboardProviders extends StatefulWidget {
  const _LeaderboardProviders({required this.tokenStore});

  final TokenStore tokenStore;

  @override
  State<_LeaderboardProviders> createState() => _LeaderboardProvidersState();
}

class _LeaderboardProvidersState extends State<_LeaderboardProviders> {
  late final http.Client _httpClient;
  late final LeaderboardCubit _leaderboardCubit;
  late final Future<String?> _playerIdFuture;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );
    _leaderboardCubit = LeaderboardCubit(
      playerStatsRepository: PlayerStatsRepository(apiClient: apiClient),
    );
    _playerIdFuture = widget.tokenStore.readPlayerId();
    unawaited(_leaderboardCubit.loadLeaderboard());
  }

  @override
  void dispose() {
    _leaderboardCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: _leaderboardCubit,
      child: FutureBuilder<String?>(
        future: _playerIdFuture,
        builder: (context, snapshot) {
          return _LeaderboardView(currentPlayerId: snapshot.data);
        },
      ),
    );
  }
}

class _LeaderboardView extends StatelessWidget {
  const _LeaderboardView({required this.currentPlayerId});

  final String? currentPlayerId;

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      child: BlocBuilder<LeaderboardCubit, LeaderboardState>(
        builder: (context, state) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AppBackBar(title: context.l10n.leaderboardTitle),
              const SizedBox(height: 12),
              _PeriodSelector(
                selected: state.query.period,
                disabled: state.isLoading,
                onChanged: (period) =>
                    context.read<LeaderboardCubit>().changePeriod(period),
              ),
              const SizedBox(height: 16),
              if (state.isLoading)
                AppLoadingState(message: context.l10n.loadingLeaderboard)
              else if (state.status == LeaderboardStatus.failure)
                AppErrorState(
                  title: context.l10n.couldNotLoadLeaderboard,
                  message: state.message ?? context.l10n.commonTryAgain,
                  onRetry: () => context
                      .read<LeaderboardCubit>()
                      .loadLeaderboard(query: state.query),
                )
              else if (state.status == LeaderboardStatus.success &&
                  state.entries.isEmpty)
                AppEmptyState(
                  title: context.l10n.noScoresTitle,
                  message: _emptyMessage(context, state.query.period),
                  actionLabel: context.l10n.commonRefresh,
                  onAction: () => context
                      .read<LeaderboardCubit>()
                      .loadLeaderboard(query: state.query),
                )
              else
                _LeaderboardContent(
                  entries: state.entries,
                  period: state.query.period,
                  currentPlayerId: currentPlayerId,
                ),
            ],
          );
        },
      ),
    );
  }
}

String _periodSubtitle(BuildContext context, LeaderboardPeriod period) {
  switch (period) {
    case LeaderboardPeriod.allTime:
      return context.l10n.leaderboardAllTimeDesc;
    case LeaderboardPeriod.daily:
      return context.l10n.leaderboardDailyDesc;
    case LeaderboardPeriod.weekly:
      return context.l10n.leaderboardWeeklyDesc;
  }
}

String _emptyMessage(BuildContext context, LeaderboardPeriod period) {
  switch (period) {
    case LeaderboardPeriod.allTime:
      return context.l10n.leaderboardAllTimeEmpty;
    case LeaderboardPeriod.daily:
      return context.l10n.leaderboardDailyEmpty;
    case LeaderboardPeriod.weekly:
      return context.l10n.leaderboardWeeklyEmpty;
  }
}

class _PeriodSelector extends StatelessWidget {
  const _PeriodSelector({
    required this.selected,
    required this.disabled,
    required this.onChanged,
  });

  final LeaderboardPeriod selected;
  final bool disabled;
  final ValueChanged<LeaderboardPeriod> onChanged;

  @override
  Widget build(BuildContext context) {
    return SegmentedButton<LeaderboardPeriod>(
      showSelectedIcon: false,
      segments: [
        ButtonSegment(
          value: LeaderboardPeriod.allTime,
          label: Text(context.l10n.leaderboardAllTime),
        ),
        ButtonSegment(
          value: LeaderboardPeriod.daily,
          label: Text(context.l10n.leaderboardDaily),
        ),
        ButtonSegment(
          value: LeaderboardPeriod.weekly,
          label: Text(context.l10n.leaderboardWeekly),
        ),
      ],
      selected: {selected},
      onSelectionChanged: disabled
          ? null
          : (selection) => onChanged(selection.first),
    );
  }
}

class _LeaderboardContent extends StatelessWidget {
  const _LeaderboardContent({
    required this.entries,
    required this.period,
    required this.currentPlayerId,
  });

  final List<LeaderboardEntry> entries;
  final LeaderboardPeriod period;
  final String? currentPlayerId;

  @override
  Widget build(BuildContext context) {
    final topEntries = entries.take(3).toList(growable: false);
    final remainingEntries = entries.skip(3).toList(growable: false);
    final currentPlayerRank = currentPlayerId == null
        ? null
        : entries.indexWhere((entry) => entry.playerId == currentPlayerId) + 1;
    final currentPlayerEntry =
        currentPlayerRank == null || currentPlayerRank <= 0
        ? null
        : entries[currentPlayerRank - 1];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _LeaderboardSummary(
          leader: entries.isEmpty ? null : entries.first,
          period: period,
          currentPlayerRank: currentPlayerRank == 0 ? null : currentPlayerRank,
        ),
        const SizedBox(height: 16),
        _LeaderboardPodium(entries: topEntries),
        if (currentPlayerEntry != null && currentPlayerRank! > 3) ...[
          const SizedBox(height: 16),
          _LeaderboardRow(
            rank: currentPlayerRank,
            entry: currentPlayerEntry,
            highlighted: true,
          ),
        ],
        if (remainingEntries.isNotEmpty) ...[
          const SizedBox(height: 16),
          for (var index = 0; index < remainingEntries.length; index++) ...[
            _LeaderboardRow(
              rank: index + 4,
              entry: remainingEntries[index],
              highlighted: remainingEntries[index].playerId == currentPlayerId,
            ),
            if (index < remainingEntries.length - 1) const SizedBox(height: 10),
          ],
        ],
      ],
    );
  }
}

class _LeaderboardSummary extends StatelessWidget {
  const _LeaderboardSummary({
    required this.leader,
    required this.period,
    required this.currentPlayerRank,
  });

  final LeaderboardEntry? leader;
  final LeaderboardPeriod period;
  final int? currentPlayerRank;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final leaderScore = leader == null
        ? '0'
        : _formatNumber(leader!.totalScore);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: colorScheme.primary.withValues(alpha: 0.16)),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: colorScheme.primary.withValues(alpha: 0.08),
            blurRadius: 16,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            DecoratedBox(
              decoration: BoxDecoration(
                color: colorScheme.primary,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Padding(
                padding: const EdgeInsets.all(10),
                child: Icon(
                  Icons.emoji_events_outlined,
                  color: colorScheme.onPrimary,
                ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    _periodSubtitle(context, period),
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: colorScheme.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '${context.l10n.statTotalScore}: $leaderScore',
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: colorScheme.onSurface,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                ],
              ),
            ),
            if (currentPlayerRank != null)
              _RankPill(label: '#$currentPlayerRank'),
          ],
        ),
      ),
    );
  }
}

class _LeaderboardPodium extends StatelessWidget {
  const _LeaderboardPodium({required this.entries});

  final List<LeaderboardEntry> entries;

  @override
  Widget build(BuildContext context) {
    if (entries.isEmpty) return const SizedBox.shrink();

    final places = <int>[
      if (entries.length > 1) 2,
      1,
      if (entries.length > 2) 3,
    ];

    return Row(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        for (var index = 0; index < places.length; index++) ...[
          Expanded(
            child: _PodiumPlace(
              rank: places[index],
              entry: entries[places[index] - 1],
              height: places[index] == 1 ? 154 : 128,
            ),
          ),
          if (index < places.length - 1) const SizedBox(width: 10),
        ],
      ],
    );
  }
}

class _PodiumPlace extends StatelessWidget {
  const _PodiumPlace({
    required this.rank,
    required this.entry,
    required this.height,
  });

  final int rank;
  final LeaderboardEntry entry;
  final double height;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final handle = entry.handle ?? context.l10n.guestPlayer;
    final isWinner = rank == 1;
    final podiumColor = isWinner ? colorScheme.secondary : colorScheme.primary;

    return SizedBox(
      height: height,
      child: DecoratedBox(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [
              podiumColor,
              Color.lerp(
                podiumColor,
                Colors.black,
                0.28,
              )!,
            ],
          ),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              _RankPill(label: '#$rank', inverted: true),
              _PlayerAvatar(entry: entry, radius: isWinner ? 24 : 20),
              Flexible(
                child: Text(
                  handle,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                  style: theme.textTheme.labelLarge?.copyWith(
                    color: Colors.white,
                  ),
                ),
              ),
              Text(
                _formatNumber(entry.totalScore),
                style: theme.textTheme.labelSmall?.copyWith(
                  color: Colors.white.withValues(alpha: 0.86),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _LeaderboardRow extends StatelessWidget {
  const _LeaderboardRow({
    required this.rank,
    required this.entry,
    this.highlighted = false,
  });

  final int rank;
  final LeaderboardEntry entry;
  final bool highlighted;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;
    final handle = entry.handle ?? context.l10n.guestPlayer;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: highlighted
            ? colorScheme.secondaryContainer.withValues(alpha: 0.7)
            : colorScheme.surface,
        border: Border.all(
          color: highlighted
              ? colorScheme.secondary.withValues(alpha: 0.5)
              : colorScheme.outline.withValues(alpha: 0.24),
        ),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: colorScheme.primary.withValues(alpha: 0.06),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        child: Row(
          children: [
            _RankPill(label: '#$rank'),
            const SizedBox(width: 12),
            _PlayerAvatar(entry: entry, radius: 18),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    handle,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: textTheme.titleMedium,
                  ),
                  const SizedBox(height: 2),
                  Text(
                    '${context.l10n.statGamesCompleted}: '
                    '${entry.gamesCompleted} · '
                    '${context.l10n.statBestScore} ${entry.bestScore ?? 0}',
                    style: textTheme.bodySmall?.copyWith(
                      color: colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  _formatNumber(entry.totalScore),
                  style: textTheme.titleMedium?.copyWith(
                    color: colorScheme.primary,
                  ),
                ),
                Text(
                  context.l10n.statTotalScore,
                  style: textTheme.labelSmall?.copyWith(
                    color: colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _RankPill extends StatelessWidget {
  const _RankPill({required this.label, this.inverted = false});

  final String label;
  final bool inverted;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: inverted
            ? Colors.white.withValues(alpha: 0.22)
            : colorScheme.primary.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(8),
      ),
      child: SizedBox(
        width: 44,
        height: 30,
        child: Center(
          child: Text(
            label,
            style: Theme.of(context).textTheme.labelLarge?.copyWith(
              color: inverted ? Colors.white : colorScheme.primary,
            ),
          ),
        ),
      ),
    );
  }
}

class _PlayerAvatar extends StatelessWidget {
  const _PlayerAvatar({required this.entry, required this.radius});

  final LeaderboardEntry entry;
  final double radius;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final avatarUrl = entry.avatarUrl;
    final handle =
        entry.handle ?? entry.displayName ?? context.l10n.guestPlayer;

    return CircleAvatar(
      radius: radius,
      backgroundColor: colorScheme.primaryContainer,
      foregroundImage: avatarUrl == null ? null : NetworkImage(avatarUrl),
      child: Text(
        _initials(handle),
        style: Theme.of(context).textTheme.labelSmall?.copyWith(
          color: colorScheme.onPrimaryContainer,
        ),
      ),
    );
  }
}

String _initials(String value) {
  final trimmed = value.trim();
  if (trimmed.isEmpty) return '?';
  return trimmed.characters.first.toUpperCase();
}

String _formatNumber(int value) {
  final text = value.toString();
  final buffer = StringBuffer();
  for (var i = 0; i < text.length; i++) {
    final remaining = text.length - i;
    buffer.write(text[i]);
    if (remaining > 1 && remaining % 3 == 1) {
      buffer.write(',');
    }
  }
  return buffer.toString();
}
