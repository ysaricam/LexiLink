import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_undo/data/admin_undo_repository.dart';
import 'package:lexilink_app/features/admin_undo/data/player_undo_snapshot.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminUndoStatus {
  initial,
  loading,
  loaded,
  saving,
  notFound,
  failure,
}

class AdminUndoCubit extends Cubit<AdminUndoState> {
  AdminUndoCubit({required AdminUndoRepository repository})
    : _repository = repository,
      super(const AdminUndoState.initial());

  final AdminUndoRepository _repository;

  Future<void> load(String playerId) async {
    emit(state.copyWith(
      status: AdminUndoStatus.loading,
      currentPlayerId: playerId,
      clearSnapshot: true,
      clearError: true,
    ));
    try {
      final snapshot = await _repository.fetchSnapshot(playerId);
      emit(state.copyWith(
        status: AdminUndoStatus.loaded,
        snapshot: snapshot,
      ));
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
        emit(state.copyWith(
          status: AdminUndoStatus.notFound,
          errorMessage: 'No undo inventory for player $playerId.',
        ));
        return;
      }
      emit(state.copyWith(
        status: AdminUndoStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> setBalance(int balance) async {
    final id = state.snapshot?.playerId;
    if (id == null) return;
    await _mutate(() => _repository.setBalance(playerId: id, balance: balance));
  }

  Future<void> grant(int amount) async {
    final id = state.snapshot?.playerId;
    if (id == null) return;
    await _mutate(() => _repository.grant(playerId: id, amount: amount));
  }

  Future<void> reset() async {
    final id = state.snapshot?.playerId;
    if (id == null) return;
    await _mutate(() => _repository.reset(id));
  }

  Future<void> _mutate(Future<void> Function() action) async {
    final id = state.snapshot!.playerId;
    emit(state.copyWith(status: AdminUndoStatus.saving, clearError: true));
    try {
      await action();
      final snapshot = await _repository.fetchSnapshot(id);
      emit(state.copyWith(
        status: AdminUndoStatus.loaded,
        snapshot: snapshot,
      ));
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminUndoStatus.failure,
        errorMessage: e.message,
      ));
    }
  }
}

class AdminUndoState extends Equatable {
  const AdminUndoState({
    required this.status,
    this.snapshot,
    this.currentPlayerId,
    this.errorMessage,
  });

  const AdminUndoState.initial() : this(status: AdminUndoStatus.initial);

  AdminUndoState copyWith({
    AdminUndoStatus? status,
    PlayerUndoSnapshot? snapshot,
    String? currentPlayerId,
    String? errorMessage,
    bool clearSnapshot = false,
    bool clearError = false,
  }) {
    return AdminUndoState(
      status: status ?? this.status,
      snapshot: clearSnapshot ? null : (snapshot ?? this.snapshot),
      currentPlayerId: currentPlayerId ?? this.currentPlayerId,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminUndoStatus status;
  final PlayerUndoSnapshot? snapshot;
  final String? currentPlayerId;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, snapshot, currentPlayerId, errorMessage];
}
