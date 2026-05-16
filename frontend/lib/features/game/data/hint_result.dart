import 'package:equatable/equatable.dart';

class HintResult extends Equatable {
  const HintResult({
    required this.type,
    required this.recommendedLinkId,
  });

  factory HintResult.fromJson(Map<String, dynamic> json) {
    final type = json['type'];
    final recommendedLinkId = json['recommendedLinkId'];

    if (type is! String ||
        type.isEmpty ||
        recommendedLinkId is! String ||
        recommendedLinkId.isEmpty) {
      throw StateError('Hint response is missing required fields.');
    }

    return HintResult(
      type: type,
      recommendedLinkId: recommendedLinkId,
    );
  }

  final String type;
  final String recommendedLinkId;

  @override
  List<Object> get props => [type, recommendedLinkId];
}
