import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/undo/data/player_undo.dart';
import 'package:lexilink_app/features/undo/data/undo_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum UndoStatus { initial, loading, success, failure }

class UndoCubit extends Cubit<UndoState> {
  UndoCubit({required UndoRepository undoRepository})
    : _undoRepository = undoRepository,
      super(const UndoState.initial());

  final UndoRepository _undoRepository;

  Future<void> loadUndo() async {
    emit(const UndoState.loading());

    try {
      final undo = await _undoRepository.getMe();
      emit(UndoState.success(undo: undo));
    } on ApiException catch (error) {
      emit(UndoState.failure(message: error.message));
    } on Exception {
      emit(
        const UndoState.failure(
          message: 'We could not load undo balance. Try again.',
        ),
      );
    }
  }
}

class UndoState extends Equatable {
  const UndoState({
    required this.status,
    this.undo,
    this.message,
  });

  const UndoState.initial() : this(status: UndoStatus.initial);

  const UndoState.loading() : this(status: UndoStatus.loading);

  const UndoState.success({required PlayerUndo undo})
    : this(status: UndoStatus.success, undo: undo);

  const UndoState.failure({required String message})
    : this(status: UndoStatus.failure, message: message);

  final UndoStatus status;
  final PlayerUndo? undo;
  final String? message;

  bool get isLoading => status == UndoStatus.loading;

  @override
  List<Object?> get props => [status, undo, message];
}
