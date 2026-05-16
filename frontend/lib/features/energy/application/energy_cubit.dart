import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/features/energy/data/player_energy.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum EnergyStatus {
  initial,
  loading,
  success,
  failure,
}

class EnergyCubit extends Cubit<EnergyState> {
  EnergyCubit({
    required EnergyRepository energyRepository,
  }) : _energyRepository = energyRepository,
       super(const EnergyState.initial());

  final EnergyRepository _energyRepository;

  Future<void> loadEnergy() async {
    emit(const EnergyState.loading());

    try {
      final energy = await _energyRepository.getMe();
      emit(EnergyState.success(energy: energy));
    } on ApiException catch (error) {
      emit(EnergyState.failure(message: error.message));
    } on Exception {
      emit(
        const EnergyState.failure(
          message: 'We could not load energy. Try again.',
        ),
      );
    }
  }
}

class EnergyState extends Equatable {
  const EnergyState({
    required this.status,
    this.energy,
    this.message,
  });

  const EnergyState.initial() : this(status: EnergyStatus.initial);

  const EnergyState.loading() : this(status: EnergyStatus.loading);

  const EnergyState.success({required PlayerEnergy energy})
    : this(status: EnergyStatus.success, energy: energy);

  const EnergyState.failure({required String message})
    : this(status: EnergyStatus.failure, message: message);

  final EnergyStatus status;
  final PlayerEnergy? energy;
  final String? message;

  bool get isLoading => status == EnergyStatus.loading;

  @override
  List<Object?> get props => [status, energy, message];
}
