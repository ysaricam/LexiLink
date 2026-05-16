import 'package:equatable/equatable.dart';

class Category extends Equatable {
  const Category({
    required this.id,
    required this.name,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final name = json['name'];

    if (id is! String || id.isEmpty || name is! String || name.isEmpty) {
      throw StateError('Category response is missing required fields.');
    }

    return Category(id: id, name: name);
  }

  final String id;
  final String name;

  @override
  List<Object> get props => [id, name];
}
