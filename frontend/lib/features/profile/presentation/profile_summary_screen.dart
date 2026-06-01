import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/profile/application/profile_summary_cubit.dart';
import 'package:lexilink_app/features/profile/data/player_stats.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class ProfileSummaryScreen extends StatefulWidget {
  const ProfileSummaryScreen({super.key});

  @override
  State<ProfileSummaryScreen> createState() => _ProfileSummaryScreenState();
}

class _ProfileSummaryScreenState extends State<ProfileSummaryScreen> {
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

        return _ProfileSummaryProviders(tokenStore: tokenStore);
      },
    );
  }
}

class _ProfileSummaryProviders extends StatefulWidget {
  const _ProfileSummaryProviders({required this.tokenStore});

  final TokenStore tokenStore;

  @override
  State<_ProfileSummaryProviders> createState() =>
      _ProfileSummaryProvidersState();
}

class _ProfileSummaryProvidersState extends State<_ProfileSummaryProviders> {
  late final http.Client _httpClient;
  late final ProfileSummaryCubit _profileSummaryCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );
    _profileSummaryCubit = ProfileSummaryCubit(
      playerStatsRepository: PlayerStatsRepository(apiClient: apiClient),
      tokenStore: widget.tokenStore,
    );
    unawaited(_profileSummaryCubit.loadSummary());
  }

  @override
  void dispose() {
    _profileSummaryCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: _profileSummaryCubit,
      child: const _ProfileSummaryView(),
    );
  }
}

class _ProfileSummaryView extends StatelessWidget {
  const _ProfileSummaryView();

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      child: BlocBuilder<ProfileSummaryCubit, ProfileSummaryState>(
        builder: (context, state) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AppBackBar(title: context.l10n.navProfile),
              const SizedBox(height: 20),
              if (state.isLoading)
                AppLoadingState(message: context.l10n.loadingProfile)
              else if (state.status == ProfileSummaryStatus.failure)
                AppErrorState(
                  title: context.l10n.couldNotLoadProfile,
                  message: state.message ?? context.l10n.commonTryAgain,
                  onRetry: () =>
                      context.read<ProfileSummaryCubit>().loadSummary(),
                )
              else if (state.status == ProfileSummaryStatus.success &&
                  state.stats != null)
                _ProfileSummaryCard(stats: state.stats!)
              else
                AppEmptyState(
                  title: context.l10n.noProfileTitle,
                  message: context.l10n.noProfileMessage,
                ),
              const SizedBox(height: 20),
              AppPrimaryButton(
                label: context.l10n.viewLeaderboard,
                onPressed: () => context.go('/leaderboard'),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _ProfileSummaryCard extends StatelessWidget {
  const _ProfileSummaryCard({required this.stats});

  final PlayerStats stats;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    final handle = stats.handle ?? context.l10n.guestPlayer;
    final localeLabel = stats.locale ?? context.l10n.commonUnknown;
    final providerLabel = stats.isGuest
        ? context.l10n.guestSession
        : context.l10n.providersLinked(stats.authProvidersLinked);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Center(
          child: _ProfileAvatar(initial: handle.isEmpty ? '?' : handle[0]),
        ),
        const SizedBox(height: 12),
        Text(
          handle,
          style: textTheme.titleLarge,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 4),
        Text(
          '$providerLabel · $localeLabel',
          style: textTheme.bodySmall?.copyWith(
            color: colorScheme.onSurfaceVariant,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 20),
        DecoratedBox(
          decoration: BoxDecoration(
            color: colorScheme.surface,
            border: Border.all(
              color: colorScheme.outline.withValues(alpha: 0.32),
            ),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
            child: Column(
              children: [
                _ProfileStatRow(
                  label: context.l10n.statGamesCompleted,
                  value: stats.gamesCompleted.toString(),
                ),
                const _RowDivider(),
                _ProfileStatRow(
                  label: context.l10n.statBestScore,
                  value: stats.bestScore?.toString() ?? '—',
                ),
                const _RowDivider(),
                _ProfileStatRow(
                  label: context.l10n.statTotalScore,
                  value: stats.totalScore.toString(),
                ),
                const _RowDivider(),
                _ProfileStatRow(
                  label: context.l10n.statLastCompleted,
                  value: _formatDate(stats.lastGameCompletedOn),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  static String _formatDate(DateTime? value) {
    if (value == null) {
      return '—';
    }

    final local = value.toLocal();
    final y = local.year.toString().padLeft(4, '0');
    final m = local.month.toString().padLeft(2, '0');
    final d = local.day.toString().padLeft(2, '0');
    return '$y-$m-$d';
  }
}

class _ProfileAvatar extends StatelessWidget {
  const _ProfileAvatar({required this.initial});

  final String initial;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      width: 84,
      height: 84,
      decoration: const BoxDecoration(
        shape: BoxShape.circle,
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xffe39d4a), Color(0xffb04d2b)],
        ),
      ),
      alignment: Alignment.center,
      child: Text(
        initial.toUpperCase(),
        style: theme.textTheme.displaySmall?.copyWith(color: Colors.white),
      ),
    );
  }
}

class _RowDivider extends StatelessWidget {
  const _RowDivider();

  @override
  Widget build(BuildContext context) {
    return Divider(
      height: 18,
      thickness: 1,
      color: Theme.of(context).colorScheme.outline.withValues(alpha: 0.18),
    );
  }
}

class _ProfileStatRow extends StatelessWidget {
  const _ProfileStatRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: textTheme.bodyMedium?.copyWith(
            color: colorScheme.onSurfaceVariant,
          ),
        ),
        Text(value, style: textTheme.titleMedium),
      ],
    );
  }
}
