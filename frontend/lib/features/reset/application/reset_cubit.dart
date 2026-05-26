import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/reset/data/player_reset.dart';
import 'package:lexilink_app/features/reset/data/reset_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum ResetStatus { initial, loading, success, failure }

class ResetCubit extends Cubit<ResetState> {
  ResetCubit({required ResetRepository resetRepository})
    : _resetRepository = resetRepository,
      super(const ResetState.initial());

  final ResetRepository _resetRepository;

  Future<void> loadReset() async {
    emit(const ResetState.loading());

    try {
      final reset = await _resetRepository.getMe();
      emit(ResetState.success(reset: reset));
    } on ApiException catch (error) {
      emit(ResetState.failure(message: error.message));
    } on Exception {
      emit(
        const ResetState.failure(
          message: 'We could not load reset balance. Try again.',
        ),
      );
    }
  }
}

class ResetState extends Equatable {
  const ResetState({
    required this.status,
    this.reset,
    this.message,
  });

  const ResetState.initial() : this(status: ResetStatus.initial);

  const ResetState.loading() : this(status: ResetStatus.loading);

  const ResetState.success({required PlayerReset reset})
    : this(status: ResetStatus.success, reset: reset);

  const ResetState.failure({required String message})
    : this(status: ResetStatus.failure, message: message);

  final ResetStatus status;
  final PlayerReset? reset;
  final String? message;

  bool get isLoading => status == ResetStatus.loading;

  @override
  List<Object?> get props => [status, reset, message];
}
