import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/hint/data/hint_repository.dart';
import 'package:lexilink_app/features/hint/data/player_hint.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum HintStatus { initial, loading, success, failure }

class HintCubit extends Cubit<HintState> {
  HintCubit({required HintRepository hintRepository})
    : _hintRepository = hintRepository,
      super(const HintState.initial());

  final HintRepository _hintRepository;

  Future<void> loadHint() async {
    emit(const HintState.loading());

    try {
      final hint = await _hintRepository.getMe();
      emit(HintState.success(hint: hint));
    } on ApiException catch (error) {
      emit(HintState.failure(message: error.message));
    } on Exception {
      emit(
        const HintState.failure(
          message: 'We could not load hint balance. Try again.',
        ),
      );
    }
  }
}

class HintState extends Equatable {
  const HintState({
    required this.status,
    this.hint,
    this.message,
  });

  const HintState.initial() : this(status: HintStatus.initial);

  const HintState.loading() : this(status: HintStatus.loading);

  const HintState.success({required PlayerHint hint})
    : this(status: HintStatus.success, hint: hint);

  const HintState.failure({required String message})
    : this(status: HintStatus.failure, message: message);

  final HintStatus status;
  final PlayerHint? hint;
  final String? message;

  bool get isLoading => status == HintStatus.loading;

  @override
  List<Object?> get props => [status, hint, message];
}
