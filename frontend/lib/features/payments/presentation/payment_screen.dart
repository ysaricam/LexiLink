import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/diamond/application/diamond_cubit.dart';
import 'package:lexilink_app/features/payments/application/payment_cubit.dart';
import 'package:lexilink_app/features/payments/data/payment_models.dart';
import 'package:lexilink_app/features/payments/data/payment_repository.dart';
import 'package:lexilink_app/features/payments/data/payment_store_service.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';

class PaymentScreen extends StatefulWidget {
  const PaymentScreen({super.key, this.cubitFactory, this.modal = false});

  final PaymentCubit Function()? cubitFactory;
  final bool modal;

  @override
  State<PaymentScreen> createState() => _PaymentScreenState();
}

class _PaymentScreenState extends State<PaymentScreen> {
  PaymentCubit? _cubit;
  http.Client? _client;

  @override
  void initState() {
    super.initState();
    unawaited(_init());
  }

  Future<void> _init() async {
    final PaymentCubit cubit;
    if (widget.cubitFactory != null) {
      cubit = widget.cubitFactory!();
    } else {
      final tokenStore = await SharedPreferencesTokenStore.create();
      _client = http.Client();
      cubit = PaymentCubit(
        repository: PaymentRepository(
          apiClient: ApiClient(
            config: ApiConfig.local(),
            httpClient: _client!,
            tokenStore: tokenStore,
          ),
        ),
        storeService: InAppPurchaseStoreService(),
        platform: _currentPaymentPlatform(),
        isSupportedPlatform: _isPurchasePlatformSupported(),
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
        backgroundColor: Theme.of(context).scaffoldBackgroundColor,
        body: Center(
          child: AppLoadingState(message: context.l10n.openingDiamonds),
        ),
      );
    }
    return BlocProvider.value(
      value: cubit,
      child: _PaymentView(modal: widget.modal),
    );
  }
}

class _PaymentView extends StatelessWidget {
  const _PaymentView({required this.modal});

  final bool modal;

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<PaymentCubit, PaymentState>(
      listenWhen: (previous, current) =>
          previous.message != current.message ||
          previous.grantedDiamondAmount != current.grantedDiamondAmount,
      listener: (context, state) {
        if (state.status == PaymentStatus.success) {
          final amount = state.grantedDiamondAmount;
          final message = amount == null
              ? state.message
              : context.l10n.diamondsAddedSnack(amount);
          if (message != null) {
            ScaffoldMessenger.of(
              context,
            ).showSnackBar(SnackBar(content: Text(message)));
          }
          _readIfPresent<AudioService>(context)?.playEffect(
            SoundEffect.purchase,
          );
          _readIfPresent<DiamondCubit>(context)?.loadDiamond();
        } else if (state.status == PaymentStatus.failure) {
          if (state.message != null) {
            ScaffoldMessenger.of(
              context,
            ).showSnackBar(SnackBar(content: Text(state.message!)));
          }
          _readIfPresent<AudioService>(context)?.playEffect(SoundEffect.error);
        } else if (state.message != null) {
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(SnackBar(content: Text(state.message!)));
        }
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
                    title: context.l10n.diamondsTitle,
                    onBack: modal ? () => Navigator.of(context).pop() : null,
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

  Widget _body(BuildContext context, PaymentState state) {
    if (state.status == PaymentStatus.initial ||
        state.status == PaymentStatus.loading) {
      return Center(
        child: AppLoadingState(message: context.l10n.fetchingBundles),
      );
    }

    if (state.status == PaymentStatus.unavailable) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: AppEmptyState(
            title: context.l10n.purchasesUnavailable,
            message: state.message ?? context.l10n.purchasesUnavailableMessage,
            actionLabel: context.l10n.commonRetry,
            onAction: () => context.read<PaymentCubit>().load(),
          ),
        ),
      );
    }

    if (state.status == PaymentStatus.failure && state.bundles.isEmpty) {
      return Center(
        child: AppErrorState(
          title: context.l10n.couldNotLoadDiamonds,
          message: state.message ?? context.l10n.commonTryAgain,
          onRetry: () => context.read<PaymentCubit>().load(),
        ),
      );
    }

    if (state.bundles.isEmpty) {
      return Center(
        child: AppEmptyState(
          title: context.l10n.noBundlesTitle,
          message: context.l10n.commonCheckBackLater,
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: () => context.read<PaymentCubit>().load(),
      child: GridView.builder(
        padding: const EdgeInsets.fromLTRB(16, 10, 16, 24),
        gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
          maxCrossAxisExtent: 220,
          mainAxisExtent: 228,
          crossAxisSpacing: 14,
          mainAxisSpacing: 14,
        ),
        itemCount: state.bundles.length,
        itemBuilder: (context, index) => _DiamondBundleCard(
          bundle: state.bundles[index],
          busy: state.isBusy,
          selected:
              state.selectedProductId ==
              state.bundles[index].product.storeProductId,
        ),
      ),
    );
  }
}

Future<void> showPaymentSheet(BuildContext context) {
  final diamondCubit = _readIfPresent<DiamondCubit>(context);

  return showDialog<void>(
    context: context,
    builder: (_) {
      final dialog = Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 18, vertical: 28),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        clipBehavior: Clip.antiAlias,
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 520, maxHeight: 640),
          child: const PaymentScreen(modal: true),
        ),
      );

      if (diamondCubit == null) {
        return dialog;
      }

      return BlocProvider<DiamondCubit>.value(
        value: diamondCubit,
        child: dialog,
      );
    },
  );
}

class _DiamondBundleCard extends StatelessWidget {
  const _DiamondBundleCard({
    required this.bundle,
    required this.busy,
    required this.selected,
  });

  final DiamondBundle bundle;
  final bool busy;
  final bool selected;

  @override
  Widget build(BuildContext context) {
    final disabled = busy || !bundle.isAvailable;
    final actionLabel = selected && busy
        ? context.l10n.commonProcessing
        : bundle.isAvailable
        ? context.l10n.commonBuy
        : context.l10n.commonUnavailable;

    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: AppPalette.focus.withValues(alpha: 0.2)),
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
              children: [
                Container(
                  width: 58,
                  height: 58,
                  decoration: BoxDecoration(
                    color: AppPalette.focus.withValues(alpha: 0.14),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(
                    Icons.diamond_outlined,
                    size: 34,
                    color: AppPalette.focus,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    context.l10n.diamondBundleAmount(
                      bundle.product.diamondAmount,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            _PricePill(text: bundle.displayPrice),
            const SizedBox(height: 12),
            Text(
              bundle.storeProduct?.title ?? bundle.product.storeProductId,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: textTheme.bodyMedium?.copyWith(
                color: colorScheme.onSurfaceVariant,
              ),
            ),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                icon: Icon(
                  disabled ? Icons.block : Icons.shopping_bag_outlined,
                ),
                label: Text(actionLabel),
                onPressed: disabled
                    ? null
                    : () => context.read<PaymentCubit>().buy(bundle),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _PricePill extends StatelessWidget {
  const _PricePill({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppPalette.primary.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(99),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
        child: Text(
          text,
          style: const TextStyle(
            color: AppPalette.primary,
            fontWeight: FontWeight.w800,
          ),
        ),
      ),
    );
  }
}

PaymentPlatform? _currentPaymentPlatform() {
  if (kIsWeb) return null;
  if (defaultTargetPlatform == TargetPlatform.iOS ||
      defaultTargetPlatform == TargetPlatform.macOS) {
    return PaymentPlatform.apple;
  }
  if (defaultTargetPlatform == TargetPlatform.android) {
    return PaymentPlatform.google;
  }
  return null;
}

bool _isPurchasePlatformSupported() {
  if (kIsWeb) return false;
  return defaultTargetPlatform == TargetPlatform.iOS ||
      defaultTargetPlatform == TargetPlatform.android;
}

T? _readIfPresent<T extends Object>(BuildContext context) {
  try {
    return context.read<T>();
  } on Object catch (_) {
    return null;
  }
}
