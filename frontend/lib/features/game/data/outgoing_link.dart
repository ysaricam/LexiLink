import 'package:equatable/equatable.dart';

class OutgoingLink extends Equatable {
  const OutgoingLink({
    required this.id,
    required this.value,
    required this.isActive,
  });

  factory OutgoingLink.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final value = json['value'];
    final isActive = json['isActive'];

    if (id is! String || id.isEmpty || value is! String || isActive is! bool) {
      throw StateError('Outgoing link response is missing required fields.');
    }

    return OutgoingLink(
      id: id,
      value: value,
      isActive: isActive,
    );
  }

  final String id;
  final String value;
  final bool isActive;

  @override
  List<Object> get props => [id, value, isActive];
}
