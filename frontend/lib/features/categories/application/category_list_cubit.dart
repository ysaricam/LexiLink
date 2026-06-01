import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum CategoryListStatus {
  initial,
  loading,
  success,
  failure,
}

class CategoryListCubit extends Cubit<CategoryListState> {
  CategoryListCubit({
    required CategoryRepository categoryRepository,
  }) : _categoryRepository = categoryRepository,
       super(const CategoryListState.initial());

  final CategoryRepository _categoryRepository;

  Future<void> loadCategories({required String locale}) async {
    emit(const CategoryListState.loading());

    try {
      final categories = await _categoryRepository.getCategories(
        locale: locale,
      );
      emit(CategoryListState.success(categories: categories));
    } on ApiException catch (error) {
      emit(CategoryListState.failure(message: error.message));
    } on Exception {
      emit(
        const CategoryListState.failure(
          message: 'We could not load categories. Try again.',
        ),
      );
    }
  }

  void selectCategory(String categoryId) {
    if (state.status != CategoryListStatus.success) {
      return;
    }

    final categoryExists = state.categories.any(
      (category) => category.id == categoryId,
    );
    if (!categoryExists) {
      return;
    }

    emit(state.copyWith(selectedCategoryId: categoryId));
  }
}

class CategoryListState extends Equatable {
  const CategoryListState({
    required this.status,
    this.categories = const [],
    this.message,
    this.selectedCategoryId,
  });

  const CategoryListState.initial() : this(status: CategoryListStatus.initial);

  const CategoryListState.loading() : this(status: CategoryListStatus.loading);

  const CategoryListState.success({
    required List<Category> categories,
  }) : this(
         status: CategoryListStatus.success,
         categories: categories,
       );

  const CategoryListState.failure({
    required String message,
  }) : this(
         status: CategoryListStatus.failure,
         message: message,
       );

  final CategoryListStatus status;
  final List<Category> categories;
  final String? message;
  final String? selectedCategoryId;

  bool get isLoading => status == CategoryListStatus.loading;

  CategoryListState copyWith({
    String? selectedCategoryId,
  }) {
    return CategoryListState(
      status: status,
      categories: categories,
      message: message,
      selectedCategoryId: selectedCategoryId ?? this.selectedCategoryId,
    );
  }

  @override
  List<Object?> get props => [status, categories, message, selectedCategoryId];
}
