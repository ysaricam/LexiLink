import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/diamond/application/diamond_cubit.dart';
import 'package:lexilink_app/features/rewarded_ads/application/rewarded_ad_cubit.dart';
import 'package:lexilink_app/features/rewarded_ads/data/rewarded_ad_repository.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';

class EarnDiamondsScreen extends StatefulWidget {
  const EarnDiamondsScreen({super.key, this.cubitFactory});

  /// Overridable for tests; production builds the real cubit from the API
  /// client, token store, and the app-wide [AdsService].
  final RewardedAdCubit Function()? cubitFactory;

  @override
  State<EarnDiamondsScreen> createState() => _EarnDiamondsScreenState();
}

class _EarnDiamondsScreenState extends State<EarnDiamondsScreen> {
  RewardedAdCubit? _cubit;
  http.Client? _client;

  @override
  void initState() {
    super.initState();
    // Read the app-wide AdsService synchronously before any async gap.
    final adsService = context.read<AdsService>();
    unawaited(_init(adsService));
  }

  Future<void> _init(AdsService adsService) async {
    final RewardedAdCubit cubit;
    if (widget.cubitFactory != null) {
      cubit = widget.cubitFactory!();
    } else {
      final tokenStore = await SharedPreferencesTokenStore.create();
      final playerId = await tokenStore.readPlayerId();
      _client = http.Client();
      cubit = RewardedAdCubit(
        repository: RewardedAdRepository(
          apiClient: ApiClient(
            config: ApiConfig.local(),
            httpClient: _client!,
            tokenStore: tokenStore,
          ),
        ),
        adsService: adsService,
        userId: playerId,
        isSupported: adsService.isSupported,
      );
    }

    if (!mounted) {
      await cubit.close();
      return;
    }
    setState(() => _cubit = cubit..load());
  }

  @override
  void dispose() {
    _cubit?.close();
    _client?.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final cubit = _cubit;
    if (cubit == null) {
      return Scaffold(
        body: Center(
          child: AppLoadingState(message: context.l10n.openingRewards),
        ),
      );
    }
    return BlocProvider.value(value: cubit, child: const _EarnDiamondsView());
  }
}

class _EarnDiamondsView extends StatelessWidget {
  const _EarnDiamondsView();

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<RewardedAdCubit, RewardedAdState>(
      listenWhen: (previous, current) =>
          previous.rewardJustWatched != current.rewardJustWatched &&
              current.rewardJustWatched ||
          (previous.message != current.message && current.message != null),
      listener: (context, state) {
        if (state.rewardJustWatched) {
          _readIfPresent<AudioService>(context)?.playEffect(
            SoundEffect.purchase,
          );
          _readIfPresent<DiamondCubit>(context)?.loadDiamond();
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(context.l10n.rewardWatchedSnack)),
          );
        } else if (state.message != null) {
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(SnackBar(content: Text(state.message!)));
        }
      },
      builder: (context, state) {
        return Scaffold(
          body: SafeArea(
            child: Column(
              children: [
                AppBackBar(title: context.l10n.navEarnDiamonds),
                Expanded(child: _body(context, state)),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _body(BuildContext context, RewardedAdState state) {
    switch (state.status) {
      case RewardedAdStatusState.initial:
      case RewardedAdStatusState.loading:
        return Center(
          child: AppLoadingState(message: context.l10n.loadingRewards),
        );
      case RewardedAdStatusState.unavailable:
        return Center(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: AppEmptyState(
              title: context.l10n.rewardedUnavailableTitle,
              message:
                  state.message ?? context.l10n.rewardedUnavailableMessage,
            ),
          ),
        );
      case RewardedAdStatusState.failure:
        return Center(
          child: AppErrorState(
            title: context.l10n.couldNotLoadRewards,
            message: state.message ?? context.l10n.commonTryAgain,
            onRetry: () => context.read<RewardedAdCubit>().load(),
          ),
        );
      case RewardedAdStatusState.ready:
      case RewardedAdStatusState.watching:
        return _RewardPanel(state: state);
    }
  }
}

class _RewardPanel extends StatelessWidget {
  const _RewardPanel({required this.state});

  final RewardedAdState state;

  @override
  Widget build(BuildContext context) {
    final data = state.data!;
    final capped = data.isCapped;
    final watching = state.isWatching;

    final label = watching
        ? context.l10n.rewardLoadingAd
        : capped
        ? context.l10n.rewardDailyLimitReached
        : context.l10n.rewardWatchEarn(data.diamondPerAd);

    return RefreshIndicator(
      onRefresh: () => context.read<RewardedAdCubit>().load(),
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 16, 20, 28),
        children: [
          Card(
            clipBehavior: Clip.antiAlias,
            child: DecoratedBox(
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  colors: [Color(0xffe8f6f3), Color(0xfffff2cf)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
              ),
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(
                          Icons.diamond_outlined,
                          size: 40,
                          color: AppPalette.focus,
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            context.l10n.rewardCardTitle(data.diamondPerAd),
                            style: Theme.of(context).textTheme.titleMedium
                                ?.copyWith(fontWeight: FontWeight.w800),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Text(
                      context.l10n.rewardToday(
                        data.grantsToday,
                        data.dailyLimit,
                        data.remainingToday,
                      ),
                      style: Theme.of(context).textTheme.bodyMedium,
                    ),
                    const SizedBox(height: 20),
                    AppPrimaryButton(
                      label: label,
                      onPressed: (capped || watching)
                          ? null
                          : () => context.read<RewardedAdCubit>().watch(),
                    ),
                  ],
                ),
              ),
            ),
          ),
          const SizedBox(height: 16),
          Text(
            context.l10n.rewardFooter,
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}

T? _readIfPresent<T extends Object>(BuildContext context) {
  try {
    return context.read<T>();
  } on Object catch (_) {
    return null;
  }
}
