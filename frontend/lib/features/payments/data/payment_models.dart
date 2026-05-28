import 'package:equatable/equatable.dart';

enum PaymentPlatform {
  apple('Apple'),
  google('Google');

  const PaymentPlatform(this.wire);

  final String wire;
}

class PaymentProduct extends Equatable {
  const PaymentProduct({
    required this.id,
    required this.storeProductId,
    required this.diamondAmount,
    required this.isActive,
  });

  factory PaymentProduct.fromJson(Map<String, dynamic> json) {
    return PaymentProduct(
      id: json['id'] as String,
      storeProductId: json['storeProductId'] as String,
      diamondAmount: json['diamondAmount'] as int,
      isActive: json['isActive'] as bool,
    );
  }

  final String id;
  final String storeProductId;
  final int diamondAmount;
  final bool isActive;

  @override
  List<Object?> get props => [id, storeProductId, diamondAmount, isActive];
}

class StorePaymentProduct extends Equatable {
  const StorePaymentProduct({
    required this.id,
    required this.title,
    required this.description,
    required this.price,
  });

  final String id;
  final String title;
  final String description;
  final String price;

  @override
  List<Object?> get props => [id, title, description, price];
}

class DiamondBundle extends Equatable {
  const DiamondBundle({
    required this.product,
    required this.storeProduct,
  });

  final PaymentProduct product;
  final StorePaymentProduct? storeProduct;

  bool get isAvailable => product.isActive && storeProduct != null;
  String get displayPrice => storeProduct?.price ?? 'Unavailable';

  @override
  List<Object?> get props => [product, storeProduct];
}

class StorePurchaseProof extends Equatable {
  const StorePurchaseProof({
    required this.productId,
    this.purchaseId,
    this.verificationData,
  });

  final String productId;
  final String? purchaseId;
  final String? verificationData;

  @override
  List<Object?> get props => [productId, purchaseId, verificationData];
}

class PaymentVerifyResult extends Equatable {
  const PaymentVerifyResult({
    required this.paymentId,
    required this.productId,
    required this.diamondAmount,
    required this.status,
    required this.canFinishTransaction,
    required this.isReplay,
    this.postProcessingFailureReason,
  });

  factory PaymentVerifyResult.fromJson(Map<String, dynamic> json) {
    return PaymentVerifyResult(
      paymentId: json['paymentId'] as String,
      productId: json['productId'] as String,
      diamondAmount: json['diamondAmount'] as int,
      status: json['status'] as String,
      canFinishTransaction: json['canFinishTransaction'] as bool? ?? false,
      isReplay: json['isReplay'] as bool? ?? false,
      postProcessingFailureReason:
          json['postProcessingFailureReason'] as String?,
    );
  }

  final String paymentId;
  final String productId;
  final int diamondAmount;
  final String status;
  final bool canFinishTransaction;
  final bool isReplay;
  final String? postProcessingFailureReason;

  bool get isGranted => status.toLowerCase() == 'granted';

  @override
  List<Object?> get props => [
    paymentId,
    productId,
    diamondAmount,
    status,
    canFinishTransaction,
    isReplay,
    postProcessingFailureReason,
  ];
}
