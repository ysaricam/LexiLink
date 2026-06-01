import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_reset/data/admin_reset_repository.dart';
import 'package:lexilink_app/features/admin_reset/data/player_reset_snapshot.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminResetStatus {
  initial,
  loading,
  loaded,
  saving,
  notFound,
  failure,
}

class AdminResetCubit extends Cubit<AdminResetState> {
  AdminResetCubit({required AdminResetRepository repository})
    : _repository = repository,
      super(const AdminResetState.initial());

  final AdminResetRepository _repository;

  Future<void> load(String playerId) async {
    emit(
      state.copyWith(
        status: AdminResetStatus.loading,
        currentPlayerId: playerId,
        clearSnapshot: true,
        clearError: true,
      ),
    );
    try {
      final snapshot = await _repository.fetchSnapshot(playerId);
      emit(
        state.copyWith(
          status: AdminResetStatus.loaded,
          snapshot: snapshot,
        ),
      );
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
        emit(
          state.copyWith(
            status: AdminResetStatus.notFound,
            errorMessage: 'No reset inventory for player $playerId.',
          ),
        );
        return;
      }
      emit(
        state.copyWith(
          status: AdminResetStatus.failure,
          errorMessage: e.message,
        ),
      );
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
    emit(state.copyWith(status: AdminResetStatus.saving, clearError: true));
    try {
      await action();
      final snapshot = await _repository.fetchSnapshot(id);
      emit(
        state.copyWith(
          status: AdminResetStatus.loaded,
          snapshot: snapshot,
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminResetStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }
}

class AdminResetState extends Equatable {
  const AdminResetState({
    required this.status,
    this.snapshot,
    this.currentPlayerId,
    this.errorMessage,
  });

  const AdminResetState.initial() : this(status: AdminResetStatus.initial);

  AdminResetState copyWith({
    AdminResetStatus? status,
    PlayerResetSnapshot? snapshot,
    String? currentPlayerId,
    String? errorMessage,
    bool clearSnapshot = false,
    bool clearError = false,
  }) {
    return AdminResetState(
      status: status ?? this.status,
      snapshot: clearSnapshot ? null : (snapshot ?? this.snapshot),
      currentPlayerId: currentPlayerId ?? this.currentPlayerId,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminResetStatus status;
  final PlayerResetSnapshot? snapshot;
  final String? currentPlayerId;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, snapshot, currentPlayerId, errorMessage];
}
