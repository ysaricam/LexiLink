import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_energy/data/admin_energy_repository.dart';
import 'package:lexilink_app/features/admin_energy/data/player_energy_snapshot.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum AdminEnergyStatus {
  initial,
  loading,
  loaded,
  saving,
  notFound,
  failure,
}

class AdminEnergyCubit extends Cubit<AdminEnergyState> {
  AdminEnergyCubit({required AdminEnergyRepository repository})
    : _repository = repository,
      super(const AdminEnergyState.initial());

  final AdminEnergyRepository _repository;

  Future<void> load(String playerId) async {
    emit(state.copyWith(
      status: AdminEnergyStatus.loading,
      currentPlayerId: playerId,
      clearSnapshot: true,
      clearError: true,
    ));
    try {
      final snapshot = await _repository.fetchSnapshot(playerId);
      emit(state.copyWith(
        status: AdminEnergyStatus.loaded,
        snapshot: snapshot,
      ));
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
        emit(state.copyWith(
          status: AdminEnergyStatus.notFound,
          errorMessage: 'No energy aggregate for player $playerId.',
        ));
        return;
      }
      emit(state.copyWith(
        status: AdminEnergyStatus.failure,
        errorMessage: e.message,
      ));
    }
  }

  Future<void> setAmount(int amount) async {
    final id = state.snapshot?.playerId;
    if (id == null) return;
    await _mutate(() => _repository.setAmount(playerId: id, amount: amount));
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
    emit(state.copyWith(status: AdminEnergyStatus.saving, clearError: true));
    try {
      await action();
      final snapshot = await _repository.fetchSnapshot(id);
      emit(state.copyWith(
        status: AdminEnergyStatus.loaded,
        snapshot: snapshot,
      ));
    } on ApiException catch (e) {
      emit(state.copyWith(
        status: AdminEnergyStatus.failure,
        errorMessage: e.message,
      ));
    }
  }
}

class AdminEnergyState extends Equatable {
  const AdminEnergyState({
    required this.status,
    this.snapshot,
    this.currentPlayerId,
    this.errorMessage,
  });

  const AdminEnergyState.initial() : this(status: AdminEnergyStatus.initial);

  AdminEnergyState copyWith({
    AdminEnergyStatus? status,
    PlayerEnergySnapshot? snapshot,
    String? currentPlayerId,
    String? errorMessage,
    bool clearSnapshot = false,
    bool clearError = false,
  }) {
    return AdminEnergyState(
      status: status ?? this.status,
      snapshot: clearSnapshot ? null : (snapshot ?? this.snapshot),
      currentPlayerId: currentPlayerId ?? this.currentPlayerId,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }

  final AdminEnergyStatus status;
  final PlayerEnergySnapshot? snapshot;
  final String? currentPlayerId;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, snapshot, currentPlayerId, errorMessage];
}
