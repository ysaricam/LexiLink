import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_diamond/data/admin_diamond_repository.dart';
import 'package:lexilink_app/features/admin_diamond/data/player_diamond_snapshot.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminDiamondStatus {
  initial,
  loading,
  loaded,
  saving,
  notFound,
  failure,
}

class AdminDiamondCubit extends Cubit<AdminDiamondState> {
  AdminDiamondCubit({required AdminDiamondRepository repository})
    : _repository = repository,
      super(const AdminDiamondState.initial());

  final AdminDiamondRepository _repository;

  Future<void> load(String playerId) async {
    emit(
      state.copyWith(
        status: AdminDiamondStatus.loading,
        currentPlayerId: playerId,
        clearSnapshot: true,
        clearError: true,
      ),
    );
    try {
      final snapshot = await _repository.fetchSnapshot(playerId);
      emit(
        state.copyWith(
          status: AdminDiamondStatus.loaded,
          snapshot: snapshot,
        ),
      );
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
        emit(
          state.copyWith(
            status: AdminDiamondStatus.notFound,
            errorMessage: 'No diamond inventory for player $playerId.',
          ),
        );
        return;
      }
      emit(
        state.copyWith(
          status: AdminDiamondStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }

  Future<void> setBalance(int balance) async {
    final id = state.snapshot?.playerId;
    if (id == null) return;
    await _mutate(
      () => _repository.setBalance(playerId: id, balance: balance),
    );
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
    emit(state.copyWith(status: AdminDiamondStatus.saving, clearError: true));
    try {
      await action();
      final snapshot = await _repository.fetchSnapshot(id);
      emit(
        state.copyWith(
          status: AdminDiamondStatus.loaded,
          snapshot: snapshot,
        ),
      );
    } on ApiException catch (e) {
      emit(
        state.copyWith(
          status: AdminDiamondStatus.failure,
          errorMessage: e.message,
        ),
      );
    }
  }
}

class AdminDiamondState extends Equatable {
  const AdminDiamondState({
    required this.status,
    this.snapshot,
    this.currentPlayerId,
    this.errorMessage,
  });

  const AdminDiamondState.initial() : this(status: AdminDiamondStatus.initial);

  AdminDiamondState copyWith({
    AdminDiamondStatus? status,
    PlayerDiamondSnapshot? snapshot,
    String? currentPlayerId,
    String? errorMessage,
    bool clearSnapshot = false,
    bool clearError = false,
  }) {
    return AdminDiamondState(
      status: status ?? this.status,
      snapshot: clearSnapshot ? null : (snapshot ?? this.snapshot),
      currentPlayerId: currentPlayerId ?? this.currentPlayerId,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminDiamondStatus status;
  final PlayerDiamondSnapshot? snapshot;
  final String? currentPlayerId;
  final String? errorMessage;

  @override
  List<Object?> get props => [
    status,
    snapshot,
    currentPlayerId,
    errorMessage,
  ];
}
