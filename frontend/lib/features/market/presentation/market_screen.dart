import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/diamond/application/diamond_cubit.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/hint/application/hint_cubit.dart';
import 'package:lexilink_app/features/market/application/market_cubit.dart';
import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/features/market/data/market_repository.dart';
import 'package:lexilink_app/features/reset/application/reset_cubit.dart';
import 'package:lexilink_app/features/undo/application/undo_cubit.dart';
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

class MarketScreen extends StatefulWidget {
  const MarketScreen({super.key, this.cubitFactory});

  final MarketCubit Function()? cubitFactory;

  @override
  State<MarketScreen> createState() => _MarketScreenState();
}

class _MarketScreenState extends State<MarketScreen> {
  MarketCubit? _cubit;
  http.Client? _client;

  @override
  void initState() {
    super.initState();
    unawaited(_init());
  }

  Future<void> _init() async {
    final MarketCubit cubit;
    if (widget.cubitFactory != null) {
      cubit = widget.cubitFactory!();
    } else {
      final tokenStore = await SharedPreferencesTokenStore.create();
      _client = http.Client();
      cubit = MarketCubit(
        repository: MarketRepository(
          apiClient: ApiClient(
            config: ApiConfig.local(),
            httpClient: _client!,
            tokenStore: tokenStore,
          ),
        ),
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
          child: AppLoadingState(message: context.l10n.openingMarket),
        ),
      );
    }
    return BlocProvider.value(value: cubit, child: const _MarketView());
  }
}

class _MarketView extends StatefulWidget {
  const _MarketView();

  @override
  State<_MarketView> createState() => _MarketViewState();
}

class _MarketViewState extends State<_MarketView> {
  int _tab = 0;

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<MarketCubit, MarketState>(
      listenWhen: (p, c) => p.message != c.message && c.message != null,
      listener: (context, state) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(state.message!)));
        final effect = state.status == MarketStatus.failure
            ? SoundEffect.error
            : SoundEffect.purchase;
        _readIfPresent<AudioService>(context)?.playEffect(effect);
        _readIfPresent<DiamondCubit>(context)?.loadDiamond();
        _readIfPresent<EnergyCubit>(context)?.loadEnergy();
        _readIfPresent<HintCubit>(context)?.loadHint();
        _readIfPresent<UndoCubit>(context)?.loadUndo();
        _readIfPresent<ResetCubit>(context)?.loadReset();
      },
      builder: (context, state) {
        return Scaffold(
          body: SafeArea(
            child: Column(
              children: [
                AppBackBar(title: context.l10n.marketTitle),
                Expanded(child: _body(context, state)),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _body(BuildContext context, MarketState state) {
    if (state.status == MarketStatus.loading ||
        state.status == MarketStatus.initial) {
      return Center(
        child: AppLoadingState(message: context.l10n.fetchingOffers),
      );
    }
    if (state.status == MarketStatus.failure && state.categories.isEmpty) {
      return Center(
        child: AppErrorState(
          title: context.l10n.marketUnavailable,
          message: state.message ?? context.l10n.commonTryAgain,
          onRetry: () => context.read<MarketCubit>().load(),
        ),
      );
    }
    if (state.categories.isEmpty) {
      return Center(
        child: AppEmptyState(
          title: context.l10n.noOffersTitle,
          message: context.l10n.commonCheckBackLater,
        ),
      );
    }

    final category =
        state.categories[_tab.clamp(0, state.categories.length - 1)];
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          height: 56,
          child: ListView.separated(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            scrollDirection: Axis.horizontal,
            itemCount: state.categories.length,
            separatorBuilder: (_, _) => const SizedBox(width: 8),
            itemBuilder: (context, i) {
              final selected = i == _tab;
              return ChoiceChip(
                selected: selected,
                label: Text(state.categories[i].name),
                onSelected: (_) => setState(() => _tab = i),
              );
            },
          ),
        ),
        Expanded(
          child: GridView.builder(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
            gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
              maxCrossAxisExtent: 280,
              mainAxisExtent: 246,
              crossAxisSpacing: 14,
              mainAxisSpacing: 14,
            ),
            itemCount: category.items.length,
            itemBuilder: (context, i) => _MarketItemCard(
              item: category.items[i],
              busy: state.status == MarketStatus.buying,
            ),
          ),
        ),
      ],
    );
  }
}

class _MarketItemCard extends StatelessWidget {
  const _MarketItemCard({required this.item, required this.busy});

  final MarketItem item;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    final disabled =
        busy ||
        item.isSoldOut ||
        item.limitReached ||
        item.itemType == MarketItemType.diamond;
    return Card(
      clipBehavior: Clip.antiAlias,
      child: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [Color(0xfffff7df), Color(0xffe1f1ed)],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
        ),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Text(
                    item.itemType.symbol,
                    style: const TextStyle(fontSize: 34),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      '${item.quantity} ${item.itemType.wire}',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 10),
              if (item.hasPromotion)
                _Pill(
                  text: context.l10n.promoPrice(item.effectivePrice),
                  color: AppPalette.focus,
                )
              else
                _Pill(
                  text: context.l10n.price(item.effectivePrice),
                  color: AppPalette.primary,
                ),
              const SizedBox(height: 10),
              Text(
                context.l10n.stockLabel(
                  item.remainingStock?.toString() ??
                      context.l10n.commonUnlimited,
                ),
              ),
              Text(
                context.l10n.yourRemaining(
                  item.perPlayerRemaining?.toString() ??
                      context.l10n.commonUnlimited,
                ),
              ),
              const Spacer(),
              AppPrimaryButton(
                label: disabled
                    ? context.l10n.commonUnavailable
                    : context.l10n.commonBuy,
                onPressed: disabled ? null : () => _confirm(context),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _confirm(BuildContext context) async {
    final cubit = context.read<MarketCubit>();
    final ok = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: Text(
          context.l10n.buyConfirmTitle(item.quantity, item.itemType.wire),
        ),
        content: Text(context.l10n.buyConfirmMessage(item.effectivePrice)),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(context.l10n.commonCancel),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(context.l10n.commonBuy),
          ),
        ],
      ),
    );
    if (ok ?? false) {
      await cubit.buy(item);
    }
  }
}

T? _readIfPresent<T extends Object>(BuildContext context) {
  try {
    return context.read<T>();
  } on Object catch (_) {
    return null;
  }
}

class _Pill extends StatelessWidget {
  const _Pill({required this.text, required this.color});

  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(99),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
        child: Text(
          text,
          style: TextStyle(color: color, fontWeight: FontWeight.w800),
        ),
      ),
    );
  }
}
