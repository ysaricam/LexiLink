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
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';

class PaymentScreen extends StatefulWidget {
  const PaymentScreen({super.key, this.cubitFactory});

  final PaymentCubit Function()? cubitFactory;

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
      return const Scaffold(
        body: Center(child: AppLoadingState(message: 'Opening diamonds...')),
      );
    }
    return BlocProvider.value(value: cubit, child: const _PaymentView());
  }
}

class _PaymentView extends StatelessWidget {
  const _PaymentView();

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<PaymentCubit, PaymentState>(
      listenWhen: (previous, current) =>
          previous.message != current.message && current.message != null,
      listener: (context, state) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(state.message!)));
        if (state.status == PaymentStatus.success) {
          _readIfPresent<AudioService>(context)?.playEffect(
            SoundEffect.purchase,
          );
          _readIfPresent<DiamondCubit>(context)?.loadDiamond();
        } else if (state.status == PaymentStatus.failure) {
          _readIfPresent<AudioService>(context)?.playEffect(SoundEffect.error);
        }
      },
      builder: (context, state) {
        return Scaffold(
          body: SafeArea(
            child: Column(
              children: [
                const AppBackBar(title: 'Diamonds'),
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
      return const Center(
        child: AppLoadingState(message: 'Fetching diamond bundles...'),
      );
    }

    if (state.status == PaymentStatus.unavailable) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: AppEmptyState(
            title: 'Purchases unavailable',
            message:
                state.message ?? 'Diamonds purchase is not available here.',
            actionLabel: 'Retry',
            onAction: () => context.read<PaymentCubit>().load(),
          ),
        ),
      );
    }

    if (state.status == PaymentStatus.failure && state.bundles.isEmpty) {
      return Center(
        child: AppErrorState(
          title: 'Could not load diamonds',
          message: state.message ?? 'Try again.',
          onRetry: () => context.read<PaymentCubit>().load(),
        ),
      );
    }

    if (state.bundles.isEmpty) {
      return const Center(
        child: AppEmptyState(
          title: 'No bundles yet',
          message: 'Check back later.',
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: () => context.read<PaymentCubit>().load(),
      child: GridView.builder(
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
        gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
          maxCrossAxisExtent: 280,
          mainAxisExtent: 220,
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
        ? 'Processing...'
        : bundle.isAvailable
        ? 'Buy'
        : 'Unavailable';

    return Card(
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
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  const Icon(
                    Icons.diamond_outlined,
                    size: 34,
                    color: AppPalette.focus,
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      '${bundle.product.diamondAmount} diamonds',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
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
                style: Theme.of(context).textTheme.bodyMedium,
              ),
              const Spacer(),
              AppPrimaryButton(
                label: actionLabel,
                onPressed: disabled
                    ? null
                    : () => context.read<PaymentCubit>().buy(bundle),
              ),
            ],
          ),
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
