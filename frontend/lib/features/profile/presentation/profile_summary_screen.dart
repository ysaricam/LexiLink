import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/auth/data/guest_device_id_store.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/auth/data/social_sign_in_service.dart';
import 'package:lexilink_app/features/profile/application/account_link_cubit.dart';
import 'package:lexilink_app/features/profile/application/profile_summary_cubit.dart';
import 'package:lexilink_app/features/profile/data/account_link_repository.dart';
import 'package:lexilink_app/features/profile/data/player_stats.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
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
  late final Future<_ProfileBootstrap> _bootstrapFuture;

  @override
  void initState() {
    super.initState();
    _bootstrapFuture = _createBootstrap();
  }

  Future<_ProfileBootstrap> _createBootstrap() async {
    final tokenStore = await SharedPreferencesTokenStore.create();
    final guestDeviceId = await GuestDeviceIdStore().readOrCreate();

    return _ProfileBootstrap(
      tokenStore: tokenStore,
      guestDeviceId: guestDeviceId,
    );
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<_ProfileBootstrap>(
      future: _bootstrapFuture,
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return AppScreen(
            child: AppErrorState(
              title: context.l10n.sessionStorageFailedTitle,
              message: context.l10n.sessionStorageFailedMessage,
            ),
          );
        }

        final bootstrap = snapshot.data;
        if (bootstrap == null) {
          return AppScreen(
            child: AppLoadingState(message: context.l10n.preparingSession),
          );
        }

        return _ProfileSummaryProviders(bootstrap: bootstrap);
      },
    );
  }
}

class _ProfileBootstrap {
  const _ProfileBootstrap({
    required this.tokenStore,
    required this.guestDeviceId,
  });

  final TokenStore tokenStore;
  final String guestDeviceId;
}

class _ProfileSummaryProviders extends StatefulWidget {
  const _ProfileSummaryProviders({required this.bootstrap});

  final _ProfileBootstrap bootstrap;

  @override
  State<_ProfileSummaryProviders> createState() =>
      _ProfileSummaryProvidersState();
}

class _ProfileSummaryProvidersState extends State<_ProfileSummaryProviders> {
  late final http.Client _httpClient;
  late final ProfileSummaryCubit _profileSummaryCubit;
  late final AccountLinkCubit _accountLinkCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.bootstrap.tokenStore,
    );
    _profileSummaryCubit = ProfileSummaryCubit(
      playerStatsRepository: PlayerStatsRepository(apiClient: apiClient),
      tokenStore: widget.bootstrap.tokenStore,
    );
    _accountLinkCubit = AccountLinkCubit(
      accountLinkRepository: AccountLinkRepository(apiClient: apiClient),
      guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
      socialSignInService: const SocialSignInService(),
      tokenStore: widget.bootstrap.tokenStore,
    );
    unawaited(_profileSummaryCubit.loadSummary());
  }

  @override
  void dispose() {
    _accountLinkCubit.close();
    _profileSummaryCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider.value(value: _profileSummaryCubit),
        BlocProvider.value(value: _accountLinkCubit),
      ],
      child: _ProfileSummaryView(guestDeviceId: widget.bootstrap.guestDeviceId),
    );
  }
}

class _ProfileSummaryView extends StatelessWidget {
  const _ProfileSummaryView({required this.guestDeviceId});

  final String guestDeviceId;

  @override
  Widget build(BuildContext context) {
    return BlocListener<AccountLinkCubit, AccountLinkState>(
      listener: (context, state) {
        if (state.status == AccountLinkStatus.success) {
          final message = switch (state.success) {
            AccountLinkSuccess.switchedToExistingApplePlayer =>
              context.l10n.appleAccountActivated,
            AccountLinkSuccess.returnedToGuest =>
              context.l10n.guestSessionActivated,
            _ => context.l10n.accountLinked,
          };
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(message)),
          );
          context.read<ProfileSummaryCubit>().loadSummary();
        } else if (state.status == AccountLinkStatus.failure) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(state.message ?? context.l10n.actionFailed)),
          );
        }
      },
      child: AppScreen(
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
                  _ProfileSummaryCard(
                    stats: state.stats!,
                    sessionMode: state.sessionMode,
                    guestDeviceId: guestDeviceId,
                  )
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
      ),
    );
  }
}

class _ProfileSummaryCard extends StatelessWidget {
  const _ProfileSummaryCard({
    required this.stats,
    required this.sessionMode,
    required this.guestDeviceId,
  });

  final PlayerStats stats;
  final AuthSessionMode? sessionMode;
  final String guestDeviceId;

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
          sessionMode: sessionMode,
        ),
        const SizedBox(height: 16),
        _ProfileMetricGrid(stats: stats),
        const SizedBox(height: 16),
        _ProfileAccountPanel(
          providerLabel: providerLabel,
          localeLabel: localeLabel,
          authProvidersLinked: stats.authProvidersLinked,
          sessionMode: sessionMode,
          guestDeviceId: guestDeviceId,
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
    required this.sessionMode,
  });

  final PlayerStats stats;
  final String handle;
  final String providerLabel;
  final String localeLabel;
  final AuthSessionMode? sessionMode;

  @override
  Widget build(BuildContext context) {
    return _EditableProfileHero(
      stats: stats,
      handle: handle,
      providerLabel: providerLabel,
      localeLabel: localeLabel,
      canEditHandle: sessionMode == AuthSessionMode.apple,
    );
  }
}

class _EditableProfileHero extends StatefulWidget {
  const _EditableProfileHero({
    required this.stats,
    required this.handle,
    required this.providerLabel,
    required this.localeLabel,
    required this.canEditHandle,
  });

  final PlayerStats stats;
  final String handle;
  final String providerLabel;
  final String localeLabel;
  final bool canEditHandle;

  @override
  State<_EditableProfileHero> createState() => _EditableProfileHeroState();
}

class _EditableProfileHeroState extends State<_EditableProfileHero> {
  late final TextEditingController _displayNameController;
  late final TextEditingController _discriminatorController;
  bool _isEditing = false;
  bool _isSaving = false;
  String? _displayNameError;
  String? _discriminatorError;

  @override
  void initState() {
    super.initState();
    _displayNameController = TextEditingController(
      text: widget.stats.displayName ?? '',
    );
    _discriminatorController = TextEditingController(
      text: widget.stats.discriminator?.toString() ?? '',
    );
  }

  @override
  void didUpdateWidget(covariant _EditableProfileHero oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (!_isEditing &&
        (oldWidget.stats.displayName != widget.stats.displayName ||
            oldWidget.stats.discriminator != widget.stats.discriminator)) {
      _displayNameController.text = widget.stats.displayName ?? '';
      _discriminatorController.text =
          widget.stats.discriminator?.toString() ?? '';
    }
  }

  @override
  void dispose() {
    _displayNameController.dispose();
    _discriminatorController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;
    final handle = widget.handle;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(
          color: colorScheme.primary.withValues(alpha: 0.18),
        ),
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: colorScheme.primary.withValues(alpha: 0.1),
            blurRadius: 18,
            offset: const Offset(0, 10),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Row(
          children: [
            _ProfileAvatar(
              initial: handle.isEmpty ? '?' : handle[0],
              avatarUrl: widget.stats.avatarUrl,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  AnimatedSwitcher(
                    duration: const Duration(milliseconds: 180),
                    child: _isEditing
                        ? _InlineHandleEditor(
                            key: const ValueKey('handle-editor'),
                            displayNameController: _displayNameController,
                            discriminatorController: _discriminatorController,
                            displayNameError: _displayNameError,
                            discriminatorError: _discriminatorError,
                            isSaving: _isSaving,
                            onSave: _saveHandle,
                            onCancel: _cancelEdit,
                          )
                        : _HandleTitleRow(
                            key: const ValueKey('handle-title'),
                            handle: handle,
                            canEdit: widget.canEditHandle,
                            onEdit: _startEdit,
                          ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    '${widget.providerLabel} · ${widget.localeLabel}',
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: textTheme.bodyMedium?.copyWith(
                      color: colorScheme.onSurfaceVariant,
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
                            '${widget.stats.gamesCompleted}',
                      ),
                      _ProfileChip(
                        icon: Icons.emoji_events_outlined,
                        label:
                            widget.stats.bestScore?.toString() ??
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

  void _startEdit() {
    setState(() {
      _isEditing = true;
      _displayNameError = null;
      _discriminatorError = null;
      _displayNameController.text = widget.stats.displayName ?? '';
      _discriminatorController.text =
          widget.stats.discriminator?.toString() ?? '';
    });
  }

  void _cancelEdit() {
    if (_isSaving) {
      return;
    }

    setState(() {
      _isEditing = false;
      _displayNameError = null;
      _discriminatorError = null;
      _displayNameController.text = widget.stats.displayName ?? '';
      _discriminatorController.text =
          widget.stats.discriminator?.toString() ?? '';
    });
  }

  Future<void> _saveHandle() async {
    final displayName = _displayNameController.text.trim();
    final discriminatorText = _discriminatorController.text.trim();
    final discriminator = int.tryParse(discriminatorText);

    setState(() {
      _displayNameError =
          displayName.length < 2 ||
              displayName.length > 32 ||
              displayName.contains('#')
          ? context.l10n.usernameInvalidName
          : null;
      _discriminatorError =
          discriminator == null || discriminator < 1 || discriminator > 9999
          ? context.l10n.usernameInvalidCode
          : null;
    });

    if (_displayNameError != null || _discriminatorError != null) {
      return;
    }

    setState(() => _isSaving = true);
    final messenger = ScaffoldMessenger.of(context);
    final error = await context.read<ProfileSummaryCubit>().updateHandle(
      displayName: displayName,
      discriminator: discriminator!,
    );

    if (!mounted) {
      return;
    }

    setState(() {
      _isSaving = false;
      _isEditing = error != null;
    });

    messenger.showSnackBar(
      SnackBar(content: Text(error ?? context.l10n.usernameUpdated)),
    );
  }
}

class _HandleTitleRow extends StatelessWidget {
  const _HandleTitleRow({
    required this.handle,
    required this.canEdit,
    required this.onEdit,
    super.key,
  });

  final String handle;
  final bool canEdit;
  final VoidCallback onEdit;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      children: [
        Flexible(
          child: Text(
            handle,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: textTheme.titleLarge?.copyWith(
              color: colorScheme.onSurface,
              fontWeight: FontWeight.w800,
            ),
          ),
        ),
        if (canEdit) ...[
          const SizedBox(width: 6),
          Tooltip(
            message: context.l10n.editUsername,
            child: IconButton(
              visualDensity: VisualDensity.compact,
              iconSize: 19,
              constraints: const BoxConstraints.tightFor(
                width: 34,
                height: 34,
              ),
              style: IconButton.styleFrom(
                backgroundColor: colorScheme.primary.withValues(alpha: 0.08),
                foregroundColor: colorScheme.primary,
              ),
              onPressed: onEdit,
              icon: const Icon(Icons.edit_outlined),
            ),
          ),
        ],
      ],
    );
  }
}

class _InlineHandleEditor extends StatelessWidget {
  const _InlineHandleEditor({
    required this.displayNameController,
    required this.discriminatorController,
    required this.displayNameError,
    required this.discriminatorError,
    required this.isSaving,
    required this.onSave,
    required this.onCancel,
    super.key,
  });

  final TextEditingController displayNameController;
  final TextEditingController discriminatorController;
  final String? displayNameError;
  final String? discriminatorError;
  final bool isSaving;
  final VoidCallback onSave;
  final VoidCallback onCancel;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return LayoutBuilder(
      builder: (context, constraints) {
        final actionButtons = Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            _InlineHandleIconButton(
              icon: isSaving ? Icons.hourglass_top_outlined : Icons.check,
              onPressed: isSaving ? null : onSave,
              foregroundColor: colorScheme.onPrimary,
              backgroundColor: colorScheme.primary,
              tooltip: context.l10n.commonSave,
            ),
            const SizedBox(width: 4),
            _InlineHandleIconButton(
              icon: Icons.close,
              onPressed: isSaving ? null : onCancel,
              foregroundColor: colorScheme.onSurfaceVariant,
              backgroundColor: colorScheme.surfaceContainerHighest,
              tooltip: context.l10n.commonCancel,
            ),
          ],
        );
        final inputs = Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: TextField(
                controller: displayNameController,
                enabled: !isSaving,
                maxLength: 32,
                textInputAction: TextInputAction.next,
                decoration: InputDecoration(
                  isDense: true,
                  counterText: '',
                  labelText: context.l10n.usernameLabel,
                  errorText: displayNameError,
                  filled: true,
                  fillColor: colorScheme.surfaceContainerHighest.withValues(
                    alpha: 0.45,
                  ),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
            ),
            const SizedBox(width: 8),
            SizedBox(
              width: 80,
              child: TextField(
                controller: discriminatorController,
                enabled: !isSaving,
                maxLength: 4,
                keyboardType: TextInputType.number,
                decoration: InputDecoration(
                  isDense: true,
                  counterText: '',
                  prefixText: '#',
                  labelText: context.l10n.usernameCodeLabel,
                  errorText: discriminatorError,
                  filled: true,
                  fillColor: colorScheme.surfaceContainerHighest.withValues(
                    alpha: 0.45,
                  ),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
            ),
          ],
        );

        if (constraints.maxWidth < 300) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              inputs,
              const SizedBox(height: 8),
              Align(alignment: Alignment.centerRight, child: actionButtons),
            ],
          );
        }

        return Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(child: inputs),
            const SizedBox(width: 6),
            actionButtons,
          ],
        );
      },
    );
  }
}

class _InlineHandleIconButton extends StatelessWidget {
  const _InlineHandleIconButton({
    required this.icon,
    required this.onPressed,
    required this.foregroundColor,
    required this.backgroundColor,
    required this.tooltip,
  });

  final IconData icon;
  final VoidCallback? onPressed;
  final Color foregroundColor;
  final Color backgroundColor;
  final String tooltip;

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: tooltip,
      child: IconButton(
        iconSize: 18,
        constraints: const BoxConstraints.tightFor(width: 34, height: 34),
        style: IconButton.styleFrom(
          foregroundColor: foregroundColor,
          backgroundColor: backgroundColor,
          disabledBackgroundColor: backgroundColor.withValues(alpha: 0.45),
        ),
        onPressed: onPressed,
        icon: Icon(icon),
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
        border: Border.all(color: colorScheme.primary.withValues(alpha: 0.12)),
        borderRadius: BorderRadius.circular(999),
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
            color: colorScheme.primary.withValues(alpha: 0.14),
          ),
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
    required this.authProvidersLinked,
    required this.sessionMode,
    required this.guestDeviceId,
  });

  final String providerLabel;
  final String localeLabel;
  final int authProvidersLinked;
  final AuthSessionMode? sessionMode;
  final String guestDeviceId;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(
          color: colorScheme.primary.withValues(alpha: 0.14),
        ),
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
            const SizedBox(height: 14),
            _AccountLinkActions(
              authProvidersLinked: authProvidersLinked,
              sessionMode: sessionMode,
              guestDeviceId: guestDeviceId,
            ),
          ],
        ),
      ),
    );
  }
}

class _AccountLinkActions extends StatelessWidget {
  const _AccountLinkActions({
    required this.authProvidersLinked,
    required this.sessionMode,
    required this.guestDeviceId,
  });

  final int authProvidersLinked;
  final AuthSessionMode? sessionMode;
  final String guestDeviceId;

  @override
  Widget build(BuildContext context) {
    final linkState = context.watch<AccountLinkCubit>().state;
    final isBusy = linkState.isBusy;
    final availability = accountLinkActionAvailability(
      authProvidersLinked: authProvidersLinked,
      sessionMode: sessionMode,
    );

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Align(
          alignment: Alignment.centerLeft,
          child: Text(
            context.l10n.linkAccount,
            style: Theme.of(context).textTheme.labelLarge,
          ),
        ),
        const SizedBox(height: 10),
        Wrap(
          spacing: 10,
          runSpacing: 10,
          children: [
            OutlinedButton.icon(
              icon: const Icon(Icons.apple),
              label: Text(
                linkState.status == AccountLinkStatus.linkingApple
                    ? context.l10n.linkingAccount
                    : context.l10n.linkApple,
              ),
              onPressed: isBusy || !availability.canLinkApple
                  ? null
                  : () => context.read<AccountLinkCubit>().linkApple(),
            ),
            if (availability.canReturnToGuest)
              OutlinedButton.icon(
                icon: const Icon(Icons.logout_outlined),
                label: Text(
                  linkState.status == AccountLinkStatus.returningToGuest
                      ? context.l10n.returningToGuest
                      : context.l10n.returnToGuest,
                ),
                onPressed: isBusy
                    ? null
                    : () => context.read<AccountLinkCubit>().returnToGuest(
                        deviceId: guestDeviceId,
                        displayName: context.l10n.guestPlayer,
                        locale: context.read<LocaleCubit>().state.backendLocale,
                      ),
              ),
          ],
        ),
      ],
    );
  }
}

AccountLinkActionAvailability accountLinkActionAvailability({
  required int authProvidersLinked,
  required AuthSessionMode? sessionMode,
}) {
  return switch (sessionMode) {
    AuthSessionMode.guest => const AccountLinkActionAvailability(
      canLinkApple: true,
      canReturnToGuest: false,
    ),
    AuthSessionMode.apple => const AccountLinkActionAvailability(
      canLinkApple: false,
      canReturnToGuest: true,
    ),
    null => AccountLinkActionAvailability(
      canLinkApple: authProvidersLinked == 0,
      canReturnToGuest: authProvidersLinked > 0,
    ),
  };
}

class AccountLinkActionAvailability {
  const AccountLinkActionAvailability({
    required this.canLinkApple,
    required this.canReturnToGuest,
  });

  final bool canLinkApple;
  final bool canReturnToGuest;
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
