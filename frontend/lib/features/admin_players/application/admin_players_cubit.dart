import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_players/data/admin_players_repository.dart';
import 'package:lexilink_app/features/admin_players/data/player_admin_detail.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminPlayersStatus {
  initial,
  loading,
  loaded,
  saving,
  notFound,
  failure,
}

class AdminPlayersCubit extends Cubit<AdminPlayersState> {
  AdminPlayersCubit({required AdminPlayersRepository repository})
    : _repository = repository,
      super(const AdminPlayersState.initial());

  final AdminPlayersRepository _repository;

  Future<void> lookup(String handle) async {
    emit(
      state.copyWith(
        status: AdminPlayersStatus.loading,
        currentHandle: handle,
        clearDetail: true,
        clearError: true,
      ),
    );
    try {
      final detail = await _repository.fetchDetailByHandle(handle);
      emit(
        state.copyWith(
          status: AdminPlayersStatus.loaded,
          detail: detail,
        ),
      );
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
        emit(
          state.copyWith(
            status: AdminPlayersStatus.notFound,
            errorMessage: 'No player with handle $handle.',
          ),
        );
        return;
      }
      emit(
        state.copyWith(
          status: AdminPlayersStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> ban({required String reason}) async {
    final detail = state.detail;
    if (detail == null) return;
    emit(state.copyWith(status: AdminPlayersStatus.saving, clearError: true));
    try {
      await _repository.ban(playerId: detail.id, reason: reason);
      await _reload(detail.id);
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminPlayersStatus.failure,
          errorMessage: e.message,
          detail: detail,
        ),
      );
    }
  }

  Future<void> unban() async {
    final detail = state.detail;
    if (detail == null) return;
    emit(state.copyWith(status: AdminPlayersStatus.saving, clearError: true));
    try {
      await _repository.unban(detail.id);
      await _reload(detail.id);
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminPlayersStatus.failure,
          errorMessage: e.message,
          detail: detail,
        ),
      );
    }
  }

  Future<void> _reload(String id) async {
    final detail = await _repository.fetchDetailById(id);
    emit(
      state.copyWith(
        status: AdminPlayersStatus.loaded,
        detail: detail,
      ),
    );
  }
}

class AdminPlayersState extends Equatable {
  const AdminPlayersState({
    required this.status,
    this.detail,
    this.currentHandle,
    this.errorMessage,
  });

  const AdminPlayersState.initial() : this(status: AdminPlayersStatus.initial);

  AdminPlayersState copyWith({
    AdminPlayersStatus? status,
    PlayerAdminDetail? detail,
    String? currentHandle,
    String? errorMessage,
    bool clearDetail = false,
    bool clearError = false,
  }) {
    return AdminPlayersState(
      status: status ?? this.status,
      detail: clearDetail ? null : (detail ?? this.detail),
      currentHandle: currentHandle ?? this.currentHandle,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminPlayersStatus status;
  final PlayerAdminDetail? detail;
  final String? currentHandle;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, detail, currentHandle, errorMessage];
}
