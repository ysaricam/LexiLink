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
      child: const _LeaderboardView(),
    );
  }
}

class _LeaderboardView extends StatelessWidget {
  const _LeaderboardView();

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      child: BlocBuilder<LeaderboardCubit, LeaderboardState>(
        builder: (context, state) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const AppBackBar(title: 'Leaderboard'),
              const SizedBox(height: 8),
              Text(
                _periodSubtitle(state.query.period),
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 16),
              _PeriodSelector(
                selected: state.query.period,
                disabled: state.isLoading,
                onChanged: (period) =>
                    context.read<LeaderboardCubit>().changePeriod(period),
              ),
              const SizedBox(height: 20),
              if (state.isLoading)
                const AppLoadingState(message: 'Loading leaderboard...')
              else if (state.status == LeaderboardStatus.failure)
                AppErrorState(
                  title: 'Could not load leaderboard',
                  message: state.message ?? 'Try again.',
                  onRetry: () => context
                      .read<LeaderboardCubit>()
                      .loadLeaderboard(query: state.query),
                )
              else if (state.status == LeaderboardStatus.success &&
                  state.entries.isEmpty)
                AppEmptyState(
                  title: 'No scores yet',
                  message: _emptyMessage(state.query.period),
                  actionLabel: 'Refresh',
                  onAction: () => context
                      .read<LeaderboardCubit>()
                      .loadLeaderboard(query: state.query),
                )
              else
                _LeaderboardList(entries: state.entries),
            ],
          );
        },
      ),
    );
  }
}

String _periodSubtitle(LeaderboardPeriod period) {
  switch (period) {
    case LeaderboardPeriod.allTime:
      return 'All-time best score across players.';
    case LeaderboardPeriod.daily:
      return 'Best score today (UTC).';
    case LeaderboardPeriod.weekly:
      return 'Best score this week (UTC, Monday start).';
  }
}

String _emptyMessage(LeaderboardPeriod period) {
  switch (period) {
    case LeaderboardPeriod.allTime:
      return 'Complete a game to appear on the leaderboard.';
    case LeaderboardPeriod.daily:
      return 'No scores recorded today yet.';
    case LeaderboardPeriod.weekly:
      return 'No scores recorded this week yet.';
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
    return Center(
      child: SegmentedButton<LeaderboardPeriod>(
        segments: const [
          ButtonSegment(
            value: LeaderboardPeriod.allTime,
            label: Text('All-time'),
          ),
          ButtonSegment(
            value: LeaderboardPeriod.daily,
            label: Text('Daily'),
          ),
          ButtonSegment(
            value: LeaderboardPeriod.weekly,
            label: Text('Weekly'),
          ),
        ],
        selected: {selected},
        onSelectionChanged: disabled
            ? null
            : (selection) => onChanged(selection.first),
      ),
    );
  }
}

class _LeaderboardList extends StatelessWidget {
  const _LeaderboardList({required this.entries});

  final List<LeaderboardEntry> entries;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        for (var index = 0; index < entries.length; index++) ...[
          _LeaderboardRow(rank: index + 1, entry: entries[index]),
          if (index < entries.length - 1) const SizedBox(height: 10),
        ],
      ],
    );
  }
}

class _LeaderboardRow extends StatelessWidget {
  const _LeaderboardRow({required this.rank, required this.entry});

  final int rank;
  final LeaderboardEntry entry;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;
    final handle = entry.handle ?? 'Guest player';

    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(color: colorScheme.outline.withValues(alpha: 0.42)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          children: [
            SizedBox(
              width: 32,
              child: Text(
                '$rank',
                style: textTheme.titleMedium,
                textAlign: TextAlign.center,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(handle, style: textTheme.titleMedium),
                  const SizedBox(height: 2),
                  Text(
                    '${entry.gamesCompleted} games · total ${entry.totalScore}',
                    style: textTheme.bodySmall?.copyWith(
                      color: colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            Text(
              entry.bestScore?.toString() ?? '—',
              style: textTheme.titleMedium,
            ),
          ],
        ),
      ),
    );
  }
}
