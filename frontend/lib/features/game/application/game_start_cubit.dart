import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum GameStartStatus {
  idle,
  submitting,
  success,
  failure,
}

class GameStartCubit extends Cubit<GameStartState> {
  GameStartCubit({
    required GameRepository gameRepository,
    required TokenStore tokenStore,
  }) : _gameRepository = gameRepository,
       _tokenStore = tokenStore,
       super(const GameStartState.idle());

  final GameRepository _gameRepository;
  final TokenStore _tokenStore;

  Future<void> startGame({
    required String categoryId,
  }) async {
    emit(const GameStartState.submitting());

    try {
      final playerId = await _tokenStore.readPlayerId();
      if (playerId == null || playerId.isEmpty) {
        emit(
          const GameStartState.failure(message: 'Guest session is missing.'),
        );
        return;
      }

      final gameId = await _gameRepository.createGame(
        playerId: playerId,
        categoryId: categoryId,
      );
      await _gameRepository.startGame(gameId);
      emit(GameStartState.success(gameId: gameId));
    } on ApiException catch (error) {
      emit(GameStartState.failure(message: error.message));
    } on Exception {
      emit(
        const GameStartState.failure(
          message: 'We could not start a game. Try again.',
        ),
      );
    }
  }
}

class GameStartState extends Equatable {
  const GameStartState({
    required this.status,
    this.gameId,
    this.message,
  });

  const GameStartState.idle() : this(status: GameStartStatus.idle);

  const GameStartState.submitting() : this(status: GameStartStatus.submitting);

  const GameStartState.success({required String gameId})
    : this(status: GameStartStatus.success, gameId: gameId);

  const GameStartState.failure({required String message})
    : this(status: GameStartStatus.failure, message: message);

  final GameStartStatus status;
  final String? gameId;
  final String? message;

  bool get isSubmitting => status == GameStartStatus.submitting;

  @override
  List<Object?> get props => [status, gameId, message];
}
