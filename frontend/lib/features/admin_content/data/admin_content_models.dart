import 'package:equatable/equatable.dart';

class AdminContentCategory extends Equatable {
  const AdminContentCategory({
    required this.id,
    required this.name,
    required this.language,
  });

  factory AdminContentCategory.fromJson(Map<String, dynamic> json) {
    return AdminContentCategory(
      id: json['id'] as String,
      name: json['name'] as String,
      language: json['language'] as String,
    );
  }

  final String id;
  final String name;
  final String language;

  @override
  List<Object?> get props => [id, name, language];
}

class AdminContentCategoryDetail extends Equatable {
  const AdminContentCategoryDetail({
    required this.id,
    required this.name,
    required this.description,
    required this.language,
    required this.linkCount,
  });

  factory AdminContentCategoryDetail.fromJson(Map<String, dynamic> json) {
    return AdminContentCategoryDetail(
      id: json['id'] as String,
      name: json['name'] as String,
      description: json['description'] as String,
      language: json['language'] as String,
      linkCount: json['linkCount'] as int,
    );
  }

  final String id;
  final String name;
  final String description;
  final String language;
  final int linkCount;

  @override
  List<Object?> get props => [id, name, description, language, linkCount];
}
