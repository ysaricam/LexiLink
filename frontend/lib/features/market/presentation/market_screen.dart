import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/diamond/application/diamond_cubit.dart';
import 'package:lexilink_app/features/diamond/data/diamond_repository.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/hint/application/hint_cubit.dart';
import 'package:lexilink_app/features/market/application/market_cubit.dart';
import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/features/market/data/market_repository.dart';
import 'package:lexilink_app/features/payments/presentation/payment_screen.dart';
import 'package:lexilink_app/features/reset/application/reset_cubit.dart';
import 'package:lexilink_app/features/undo/application/undo_cubit.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';

class MarketScreen extends StatefulWidget {
  const MarketScreen({
    super.key,
    this.cubitFactory,
    this.initialItemType,
    this.modal = false,
  });

  final MarketCubit Function()? cubitFactory;
  final MarketItemType? initialItemType;
  final bool modal;

  @override
  State<MarketScreen> createState() => _MarketScreenState();
}

class _MarketScreenState extends State<MarketScreen> {
  MarketCubit? _cubit;
  DiamondCubit? _diamondCubit;
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
      final apiClient = ApiClient(
        config: ApiConfig.local(),
        httpClient: _client!,
        tokenStore: tokenStore,
      );
      cubit = MarketCubit(
        repository: MarketRepository(apiClient: apiClient),
      );
      _diamondCubit = DiamondCubit(
        diamondRepository: DiamondRepository(apiClient: apiClient),
      );
    }
    if (!mounted) {
      await cubit.close();
      await _diamondCubit?.close();
      return;
    }
    setState(() {
      _cubit = cubit..load();
      _diamondCubit?.loadDiamond();
    });
  }

  @override
  void dispose() {
    _cubit?.close();
    _diamondCubit?.close();
    _client?.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final cubit = _cubit;
    if (cubit == null) {
      return Scaffold(
        backgroundColor: Theme.of(context).scaffoldBackgroundColor,
        body: Center(
          child: AppLoadingState(message: context.l10n.openingMarket),
        ),
      );
    }
    final view = _MarketView(
      initialItemType: widget.initialItemType,
      modal: widget.modal,
    );
    final diamondCubit = _diamondCubit;
    if (diamondCubit == null) {
      return BlocProvider.value(value: cubit, child: view);
    }

    return MultiBlocProvider(
      providers: [
        BlocProvider.value(value: cubit),
        BlocProvider.value(value: diamondCubit),
      ],
      child: _MarketView(
        initialItemType: widget.initialItemType,
        modal: widget.modal,
      ),
    );
  }
}

class _MarketView extends StatefulWidget {
  const _MarketView({required this.initialItemType, required this.modal});

  final MarketItemType? initialItemType;
  final bool modal;

  @override
  State<_MarketView> createState() => _MarketViewState();
}

class _MarketViewState extends State<_MarketView> {
  int _tab = 0;
  bool _tabChangedByUser = false;

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
          backgroundColor: Theme.of(context).scaffoldBackgroundColor,
          body: SafeArea(
            child: Column(
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
                  child: AppBackBar(
                    title: context.l10n.marketTitle,
                    onBack: widget.modal
                        ? () => Navigator.of(context).pop()
                        : null,
                    trailing: const _MarketDiamondBalance(),
                  ),
                ),
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

    final categoryIndex = _resolvedCategoryIndex(state.categories);
    final category = state.categories[categoryIndex];
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          height: 46,
          child: ListView.separated(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            scrollDirection: Axis.horizontal,
            itemCount: state.categories.length,
            separatorBuilder: (_, _) => const SizedBox(width: 8),
            itemBuilder: (context, i) {
              final selected = i == _tab;
              return FilterChip(
                selected: selected,
                label: Text(state.categories[i].name),
                avatar: Text(
                  _categorySymbol(state.categories[i]),
                  style: const TextStyle(fontSize: 16),
                ),
                showCheckmark: false,
                side: BorderSide(
                  color: selected
                      ? AppPalette.primary
                      : AppPalette.primary.withValues(alpha: 0.2),
                ),
                onSelected: (_) => setState(() {
                  _tab = i;
                  _tabChangedByUser = true;
                }),
              );
            },
          ),
        ),
        Expanded(
          child: GridView.builder(
            padding: const EdgeInsets.fromLTRB(16, 10, 16, 24),
            gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
              maxCrossAxisExtent: 220,
              mainAxisExtent: 250,
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

  int _resolvedCategoryIndex(List<MarketCategory> categories) {
    if (!_tabChangedByUser && widget.initialItemType != null) {
      final index = categories.indexWhere(
        (category) => category.items.any(
          (item) => item.itemType == widget.initialItemType,
        ),
      );
      if (index >= 0) {
        _tab = index;
      }
    }

    return _tab.clamp(0, categories.length - 1);
  }

  static String _categorySymbol(MarketCategory category) {
    final icon = category.icon;
    if (icon != null && icon.isNotEmpty) {
      return icon;
    }

    final firstItem = category.items.isEmpty ? null : category.items.first;
    return firstItem?.itemType.symbol ?? '◆';
  }
}

class _MarketDiamondBalance extends StatelessWidget {
  const _MarketDiamondBalance();

  @override
  Widget build(BuildContext context) {
    final diamondCubit = _readIfPresent<DiamondCubit>(context);
    if (diamondCubit == null) {
      return const _MarketDiamondBalancePill(balance: null);
    }

    return BlocBuilder<DiamondCubit, DiamondState>(
      bloc: diamondCubit,
      builder: (context, state) {
        return _MarketDiamondBalancePill(balance: state.diamond?.balance);
      },
    );
  }
}

class _MarketDiamondBalancePill extends StatelessWidget {
  const _MarketDiamondBalancePill({required this.balance});

  final int? balance;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: AppPalette.focus.withValues(alpha: 0.32)),
        boxShadow: [
          BoxShadow(
            color: AppPalette.focus.withValues(alpha: 0.1),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.diamond_outlined,
              size: 18,
              color: AppPalette.focus,
            ),
            const SizedBox(width: 6),
            Text(
              balance?.toString() ?? '-',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                color: colorScheme.onSurface,
                fontWeight: FontWeight.w800,
              ),
            ),
          ],
        ),
      ),
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
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: AppPalette.primary.withValues(alpha: 0.14)),
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
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 58,
                  height: 58,
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    color: _itemTone(item.itemType).withValues(alpha: 0.14),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    item.itemType.symbol,
                    style: const TextStyle(fontSize: 34),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '${item.quantity} ${item.itemType.wire}',
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                      const SizedBox(height: 6),
                      _Pill(
                        text: item.hasPromotion
                            ? context.l10n.promoPrice(item.effectivePrice)
                            : context.l10n.price(item.effectivePrice),
                        color: item.hasPromotion
                            ? AppPalette.focus
                            : AppPalette.primary,
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            _ItemMetaRow(
              icon: Icons.inventory_2_outlined,
              text: context.l10n.stockLabel(
                item.remainingStock?.toString() ?? context.l10n.commonUnlimited,
              ),
            ),
            const SizedBox(height: 8),
            _ItemMetaRow(
              icon: Icons.person_outline,
              text: context.l10n.yourRemaining(
                item.perPlayerRemaining?.toString() ??
                    context.l10n.commonUnlimited,
              ),
            ),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                icon: Icon(
                  disabled ? Icons.block : Icons.shopping_bag_outlined,
                ),
                label: Text(
                  disabled
                      ? context.l10n.commonUnavailable
                      : context.l10n.commonBuy,
                ),
                onPressed: disabled ? null : () => _confirm(context),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Color _itemTone(MarketItemType type) {
    return switch (type) {
      MarketItemType.energy => AppPalette.focus,
      MarketItemType.hint => AppPalette.success,
      MarketItemType.undo => AppPalette.primary,
      MarketItemType.reset => AppPalette.danger,
      MarketItemType.diamond => AppPalette.focus,
    };
  }

  Future<void> _confirm(BuildContext context) async {
    final cubit = context.read<MarketCubit>();
    final diamondBalance = _readIfPresent<DiamondCubit>(
      context,
    )?.state.diamond?.balance;
    if (diamondBalance != null && diamondBalance < item.effectivePrice) {
      await showPaymentSheet(context);
      if (context.mounted) {
        await _readIfPresent<DiamondCubit>(context)?.loadDiamond();
      }
      return;
    }

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

class _ItemMetaRow extends StatelessWidget {
  const _ItemMetaRow({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      children: [
        Icon(icon, size: 16, color: colorScheme.onSurfaceVariant),
        const SizedBox(width: 7),
        Expanded(
          child: Text(
            text,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: Theme.of(context).textTheme.bodySmall?.copyWith(
              color: colorScheme.onSurfaceVariant,
            ),
          ),
        ),
      ],
    );
  }
}

Future<void> showMarketSheet(
  BuildContext context, {
  MarketItemType? initialItemType,
}) {
  final diamondCubit = _readIfPresent<DiamondCubit>(context);
  final energyCubit = _readIfPresent<EnergyCubit>(context);
  final hintCubit = _readIfPresent<HintCubit>(context);
  final undoCubit = _readIfPresent<UndoCubit>(context);
  final resetCubit = _readIfPresent<ResetCubit>(context);

  if (diamondCubit != null && diamondCubit.state.diamond == null) {
    unawaited(diamondCubit.loadDiamond());
  }

  return showDialog<void>(
    context: context,
    builder: (_) {
      final dialog = Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 18, vertical: 28),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        clipBehavior: Clip.antiAlias,
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 520, maxHeight: 640),
          child: MarketScreen(initialItemType: initialItemType, modal: true),
        ),
      );
      final providers = [
        if (diamondCubit != null)
          BlocProvider<DiamondCubit>.value(value: diamondCubit),
        if (energyCubit != null)
          BlocProvider<EnergyCubit>.value(value: energyCubit),
        if (hintCubit != null) BlocProvider<HintCubit>.value(value: hintCubit),
        if (undoCubit != null) BlocProvider<UndoCubit>.value(value: undoCubit),
        if (resetCubit != null)
          BlocProvider<ResetCubit>.value(value: resetCubit),
      ];

      if (providers.isEmpty) {
        return dialog;
      }

      return MultiBlocProvider(providers: providers, child: dialog);
    },
  );
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
