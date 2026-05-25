import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_hint/data/admin_hint_repository.dart';
import 'package:lexilink_app/features/admin_hint/data/player_hint_snapshot.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminHintStatus {
  initial,
  loading,
  loaded,
  saving,
  notFound,
  failure,
}

class AdminHintCubit extends Cubit<AdminHintState> {
  AdminHintCubit({required AdminHintRepository repository})
    : _repository = repository,
      super(const AdminHintState.initial());

  final AdminHintRepository _repository;

  Future<void> load(String playerId) async {
    emit(state.copyWith(
      status: AdminHintStatus.loading,
      currentPlayerId: playerId,
      clearSnapshot: true,
      clearError: true,
    ));
    try {
      final snapshot = await _repository.fetchSnapshot(playerId);
      emit(state.copyWith(
        status: AdminHintStatus.loaded,
        snapshot: snapshot,
      ));
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
        emit(state.copyWith(
          status: AdminHintStatus.notFound,
          errorMessage: 'No hint inventory for player $playerId.',
        ));
        return;
      }
      emit(state.copyWith(
        status: AdminHintStatus.failure,
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
    emit(state.copyWith(status: AdminHintStatus.saving, clearError: true));
    try {
      await action();
      final snapshot = await _repository.fetchSnapshot(id);
      emit(state.copyWith(
        status: AdminHintStatus.loaded,
        snapshot: snapshot,
      ));
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminHintStatus.failure,
        errorMessage: e.message,
      ));
    }
  }
}

class AdminHintState extends Equatable {
  const AdminHintState({
    required this.status,
    this.snapshot,
    this.currentPlayerId,
    this.errorMessage,
  });

  const AdminHintState.initial() : this(status: AdminHintStatus.initial);

  AdminHintState copyWith({
    AdminHintStatus? status,
    PlayerHintSnapshot? snapshot,
    String? currentPlayerId,
    String? errorMessage,
    bool clearSnapshot = false,
    bool clearError = false,
  }) {
    return AdminHintState(
      status: status ?? this.status,
      snapshot: clearSnapshot ? null : (snapshot ?? this.snapshot),
      currentPlayerId: currentPlayerId ?? this.currentPlayerId,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminHintStatus status;
  final PlayerHintSnapshot? snapshot;
  final String? currentPlayerId;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, snapshot, currentPlayerId, errorMessage];
}
