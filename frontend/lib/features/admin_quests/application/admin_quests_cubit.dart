import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_quests/data/admin_quests_repository.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_definition.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminQuestsStatus { initial, loading, loaded, saving, failure }

class AdminQuestsCubit extends Cubit<AdminQuestsState> {
  AdminQuestsCubit({required AdminQuestsRepository repository})
    : _repository = repository,
      super(const AdminQuestsState.initial());

  final AdminQuestsRepository _repository;

  Future<void> load() async {
    emit(state.copyWith(status: AdminQuestsStatus.loading, clearError: true));
    try {
      final defs = await _repository.fetchDefinitions();
      emit(state.copyWith(
        status: AdminQuestsStatus.loaded,
        definitions: defs,
      ));
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminQuestsStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> create({
    required AdminQuestType questType,
    required AdminQuestCadence cadence,
    required int goal,
    required int rewardAmount,
    AdminQuestType? prerequisiteQuestType,
  }) async {
    emit(state.copyWith(status: AdminQuestsStatus.saving, clearError: true));
    try {
      await _repository.createDefinition(
        questType: questType,
        cadence: cadence,
        goal: goal,
        rewardAmount: rewardAmount,
        prerequisiteQuestType: prerequisiteQuestType,
      );
      await _reload();
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminQuestsStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> update({
    required String id,
    required int goal,
    required int rewardAmount,
    AdminQuestType? prerequisiteQuestType,
  }) async {
    emit(state.copyWith(status: AdminQuestsStatus.saving, clearError: true));
    try {
      await _repository.updateDefinition(
        id: id,
        goal: goal,
        rewardAmount: rewardAmount,
        prerequisiteQuestType: prerequisiteQuestType,
      );
      await _reload();
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminQuestsStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> deactivate(String id) async {
    emit(state.copyWith(status: AdminQuestsStatus.saving, clearError: true));
    try {
      await _repository.deactivateDefinition(id);
      await _reload();
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminQuestsStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> reactivate(String id) async {
    emit(state.copyWith(status: AdminQuestsStatus.saving, clearError: true));
    try {
      await _repository.reactivateDefinition(id);
      await _reload();
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminQuestsStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> _reload() async {
    final defs = await _repository.fetchDefinitions();
    emit(state.copyWith(
      status: AdminQuestsStatus.loaded,
      definitions: defs,
    ));
  }
}

class AdminQuestsState extends Equatable {
  const AdminQuestsState({
    required this.status,
    required this.definitions,
    this.errorMessage,
  });

  const AdminQuestsState.initial()
    : this(status: AdminQuestsStatus.initial, definitions: const []);

  AdminQuestsState copyWith({
    AdminQuestsStatus? status,
    List<QuestDefinition>? definitions,
    String? errorMessage,
    bool clearError = false,
  }) {
    return AdminQuestsState(
      status: status ?? this.status,
      definitions: definitions ?? this.definitions,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminQuestsStatus status;
  final List<QuestDefinition> definitions;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, definitions, errorMessage];
}
