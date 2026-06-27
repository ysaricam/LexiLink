import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/diamond/data/diamond_repository.dart';
import 'package:lexilink_app/features/diamond/data/player_diamond.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum DiamondStatus { initial, loading, success, failure }

class DiamondCubit extends Cubit<DiamondState> {
  DiamondCubit({
    required DiamondRepository diamondRepository,
    DiamondState initialState = const DiamondState.initial(),
  }) : _diamondRepository = diamondRepository,
       super(initialState);

  final DiamondRepository _diamondRepository;

  Future<void> loadDiamond() async {
    emit(const DiamondState.loading());

    try {
      final diamond = await _diamondRepository.getMe();
      emit(DiamondState.success(diamond: diamond));
    } on ApiException catch (error) {
      emit(DiamondState.failure(message: error.message));
    } on Exception {
      emit(
        const DiamondState.failure(
          message: 'We could not load diamond balance. Try again.',
        ),
      );
    }
  }
}

class DiamondState extends Equatable {
  const DiamondState({
    required this.status,
    this.diamond,
    this.message,
  });

  const DiamondState.initial() : this(status: DiamondStatus.initial);

  const DiamondState.loading() : this(status: DiamondStatus.loading);

  const DiamondState.success({required PlayerDiamond diamond})
    : this(status: DiamondStatus.success, diamond: diamond);

  const DiamondState.failure({required String message})
    : this(status: DiamondStatus.failure, message: message);

  final DiamondStatus status;
  final PlayerDiamond? diamond;
  final String? message;

  bool get isLoading => status == DiamondStatus.loading;

  @override
  List<Object?> get props => [status, diamond, message];
}
