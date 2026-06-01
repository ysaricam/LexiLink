import 'package:equatable/equatable.dart';

class Category extends Equatable {
  const Category({
    required this.id,
    required this.name,
    required this.language,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final name = json['name'];
    final language = json['language'];

    if (id is! String ||
        id.isEmpty ||
        name is! String ||
        name.isEmpty ||
        language is! String ||
        language.isEmpty) {
      throw StateError('Category response is missing required fields.');
    }

    return Category(id: id, name: name, language: language);
  }

  final String id;
  final String name;
  final String language;

  @override
  List<Object> get props => [id, name, language];
}
