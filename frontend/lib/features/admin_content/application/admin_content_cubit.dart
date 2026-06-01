import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_content/data/admin_content_models.dart';
import 'package:lexilink_app/features/admin_content/data/admin_content_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminContentStatus { initial, loading, loaded, saving, failure }

class AdminContentCubit extends Cubit<AdminContentState> {
  AdminContentCubit({required AdminContentRepository repository})
    : _repository = repository,
      super(const AdminContentState.initial());

  final AdminContentRepository _repository;

  Future<void> load({String? locale}) async {
    emit(
      state.copyWith(
        status: AdminContentStatus.loading,
        localeFilter: locale,
        clearLocaleFilter: locale == null,
        clearError: true,
      ),
    );
    try {
      final categories = await _repository.fetchCategories(locale: locale);
      emit(
        state.copyWith(
          status: AdminContentStatus.loaded,
          categories: categories,
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminContentStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> changeLocaleFilter(String? locale) => load(locale: locale);

  Future<AdminContentCategoryDetail?> fetchCategory(String id) async {
    try {
      return await _repository.fetchCategory(id);
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminContentStatus.failure,
          errorMessage: e.message,
        ),
      );
      return null;
    }
  }

  Future<void> saveCategory({
    required String name,
    required String description,
    required String language,
    String? id,
  }) async {
    emit(state.copyWith(status: AdminContentStatus.saving, clearError: true));
    try {
      if (id == null) {
        await _repository.createCategory(
          name: name,
          description: description,
          language: language,
        );
      } else {
        await _repository.updateCategory(
          id: id,
          name: name,
          description: description,
          language: language,
        );
      }
      await load(locale: state.localeFilter);
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminContentStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }
}

class AdminContentState extends Equatable {
  const AdminContentState({
    required this.status,
    required this.categories,
    this.localeFilter,
    this.errorMessage,
  });

  const AdminContentState.initial()
    : this(
        status: AdminContentStatus.initial,
        categories: const [],
      );

  final AdminContentStatus status;
  final List<AdminContentCategory> categories;
  final String? localeFilter;
  final String? errorMessage;

  AdminContentState copyWith({
    AdminContentStatus? status,
    List<AdminContentCategory>? categories,
    String? localeFilter,
    String? errorMessage,
    bool clearLocaleFilter = false,
    bool clearError = false,
  }) {
    return AdminContentState(
      status: status ?? this.status,
      categories: categories ?? this.categories,
      localeFilter: clearLocaleFilter
          ? null
          : (localeFilter ?? this.localeFilter),
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  @override
  List<Object?> get props => [
    status,
    categories,
    localeFilter,
    errorMessage,
  ];
}
