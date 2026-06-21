import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/profile/application/profile_summary_cubit.dart';
import 'package:lexilink_app/features/profile/data/player_stats.dart';
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
      size: AppScreenSize.wide,
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
              FilledButton.icon(
                icon: const Icon(Icons.leaderboard_outlined),
                label: Text(context.l10n.viewLeaderboard),
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
    final handle = stats.handle ?? context.l10n.guestPlayer;
    final localeLabel = stats.locale ?? context.l10n.commonUnknown;
    final providerLabel = stats.isGuest
        ? context.l10n.guestSession
        : context.l10n.providersLinked(stats.authProvidersLinked);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _ProfileHero(
          stats: stats,
          handle: handle,
          providerLabel: providerLabel,
          localeLabel: localeLabel,
        ),
        const SizedBox(height: 16),
        _ProfileMetricGrid(stats: stats),
        const SizedBox(height: 16),
        _ProfileAccountPanel(
          providerLabel: providerLabel,
          localeLabel: localeLabel,
        ),
        const SizedBox(height: 4),
      ],
    );
  }
}

class _ProfileHero extends StatelessWidget {
  const _ProfileHero({
    required this.stats,
    required this.handle,
    required this.providerLabel,
    required this.localeLabel,
  });

  final PlayerStats stats;
  final String handle;
  final String providerLabel;
  final String localeLabel;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.primaryContainer,
        border: Border.all(
          color: colorScheme.primary.withValues(alpha: 0.18),
        ),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Row(
          children: [
            _ProfileAvatar(
              initial: handle.isEmpty ? '?' : handle[0],
              avatarUrl: stats.avatarUrl,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    handle,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: textTheme.titleLarge?.copyWith(
                      color: colorScheme.onPrimaryContainer,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    '$providerLabel · $localeLabel',
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: textTheme.bodyMedium?.copyWith(
                      color: colorScheme.onPrimaryContainer.withValues(
                        alpha: 0.74,
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      _ProfileChip(
                        icon: Icons.sports_esports_outlined,
                        label:
                            '${context.l10n.statGamesCompleted}: '
                            '${stats.gamesCompleted}',
                      ),
                      _ProfileChip(
                        icon: Icons.emoji_events_outlined,
                        label:
                            stats.bestScore?.toString() ??
                            context.l10n.commonUnknown,
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ProfileAvatar extends StatelessWidget {
  const _ProfileAvatar({required this.initial, required this.avatarUrl});

  final String initial;
  final String? avatarUrl;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final imageUrl = avatarUrl;

    return Container(
      width: 76,
      height: 76,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: theme.colorScheme.secondary,
        image: imageUrl == null || imageUrl.isEmpty
            ? null
            : DecorationImage(
                image: NetworkImage(imageUrl),
                fit: BoxFit.cover,
                onError: (_, _) {},
              ),
        border: Border.all(
          color: theme.colorScheme.surface.withValues(alpha: 0.9),
          width: 3,
        ),
      ),
      alignment: Alignment.center,
      child: imageUrl == null || imageUrl.isEmpty
          ? Text(
              initial.toUpperCase(),
              style: theme.textTheme.headlineMedium?.copyWith(
                color: theme.colorScheme.onSecondary,
              ),
            )
          : null,
    );
  }
}

class _ProfileChip extends StatelessWidget {
  const _ProfileChip({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface.withValues(alpha: 0.82),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 16, color: colorScheme.primary),
            const SizedBox(width: 6),
            Text(
              label,
              style: Theme.of(context).textTheme.labelSmall?.copyWith(
                color: colorScheme.onSurface,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ProfileMetricGrid extends StatelessWidget {
  const _ProfileMetricGrid({required this.stats});

  final PlayerStats stats;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        const columns = 2;
        const spacing = 10.0;
        final itemWidth =
            (constraints.maxWidth - (spacing * (columns - 1))) / columns;

        return Wrap(
          spacing: spacing,
          runSpacing: spacing,
          children: [
            _ProfileMetricTile(
              width: itemWidth,
              icon: Icons.military_tech_outlined,
              label: context.l10n.statBestScore,
              value: stats.bestScore == null
                  ? '-'
                  : formatNumber(stats.bestScore!),
            ),
            _ProfileMetricTile(
              width: itemWidth,
              icon: Icons.auto_awesome_outlined,
              label: context.l10n.statTotalScore,
              value: formatNumber(stats.totalScore),
            ),
          ],
        );
      },
    );
  }

  static String formatNumber(int value) {
    if (value >= 1000000) {
      return '${(value / 1000000).toStringAsFixed(1)}M';
    }

    if (value >= 10000) {
      return '${(value / 1000).toStringAsFixed(1)}K';
    }

    return value.toString();
  }
}

class _ProfileMetricTile extends StatelessWidget {
  const _ProfileMetricTile({
    required this.width,
    required this.icon,
    required this.label,
    required this.value,
  });

  final double width;
  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return SizedBox(
      width: width,
      height: 118,
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: colorScheme.surface,
          border: Border.all(
            color: colorScheme.outline.withValues(alpha: 0.24),
          ),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Icon(icon, size: 22, color: colorScheme.primary),
              const Spacer(),
              FittedBox(
                fit: BoxFit.scaleDown,
                alignment: Alignment.centerLeft,
                child: Text(value, style: textTheme.titleLarge),
              ),
              const SizedBox(height: 4),
              Text(
                label,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: textTheme.labelSmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ProfileAccountPanel extends StatelessWidget {
  const _ProfileAccountPanel({
    required this.providerLabel,
    required this.localeLabel,
  });

  final String providerLabel;
  final String localeLabel;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(
          color: colorScheme.outline.withValues(alpha: 0.24),
        ),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Column(
          children: [
            _ProfileInfoRow(
              icon: Icons.verified_user_outlined,
              label: providerLabel,
            ),
            const SizedBox(height: 12),
            _ProfileInfoRow(
              icon: Icons.language_outlined,
              label: '${context.l10n.languageLabel}: $localeLabel',
            ),
          ],
        ),
      ),
    );
  }
}

class _ProfileInfoRow extends StatelessWidget {
  const _ProfileInfoRow({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      children: [
        Icon(icon, size: 20, color: colorScheme.primary),
        const SizedBox(width: 10),
        Expanded(
          child: Text(
            label,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              color: colorScheme.onSurfaceVariant,
            ),
          ),
        ),
      ],
    );
  }
}
