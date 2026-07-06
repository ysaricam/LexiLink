import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/game/data/game_details.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/features/game/data/outgoing_link.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum GameDetailsStatus {
  loading,
  success,
  failure,
}

enum GameAction {
  none,
  step,
  hint,
  undo,
  reset,
  abandon,
}

class GameDetailsCubit extends Cubit<GameDetailsState> {
  GameDetailsCubit({
    required GameRepository gameRepository,
  }) : _gameRepository = gameRepository,
       super(const GameDetailsState.loading());

  final GameRepository _gameRepository;

  Future<void> loadGame(String gameId) async {
    emit(const GameDetailsState.loading());

    try {
      final game = await _getGameWithFallbackDescriptions(gameId);
      final outgoingLinks = await _gameRepository.getOptions(game.id);
      emit(GameDetailsState.success(game: game, outgoingLinks: outgoingLinks));
    } on ApiException catch (error) {
      emit(GameDetailsState.failure(message: error.message));
    } on Exception {
      emit(
        const GameDetailsState.failure(
          message: 'We could not load the game. Try again.',
        ),
      );
    }
  }

  Future<void> makeStep(String nextLinkId) async {
    final game = state.game;
    if (game == null || state.isBusy) {
      return;
    }

    emit(
      state.copyWith(
        activeAction: GameAction.step,
        clearMessage: true,
        clearHint: true,
      ),
    );

    try {
      await _gameRepository.makeStep(gameId: game.id, nextLinkId: nextLinkId);
      await _reloadGame(game.id);
    } on ApiException catch (error) {
      emit(
        state.copyWith(activeAction: GameAction.none, message: error.message),
      );
    } on Exception {
      emit(
        state.copyWith(
          activeAction: GameAction.none,
          message: 'We could not make that step. Try again.',
        ),
      );
    }
  }

  Future<void> useHint() async {
    final game = state.game;
    if (game == null || state.isBusy) {
      return;
    }

    emit(
      state.copyWith(
        activeAction: GameAction.hint,
        clearMessage: true,
        clearHint: true,
      ),
    );

    try {
      final hint = await _gameRepository.useHint(game.id);
      await _reloadGame(game.id, recommendedLinkId: hint.recommendedLinkId);
    } on ApiException catch (error) {
      emit(
        state.copyWith(activeAction: GameAction.none, message: error.message),
      );
    } on Exception {
      emit(
        state.copyWith(
          activeAction: GameAction.none,
          message: 'We could not use a hint. Try again.',
        ),
      );
    }
  }

  Future<void> undo() => _performSimpleAction(
    GameAction.undo,
    _gameRepository.undo,
    'We could not undo. Try again.',
  );

  Future<void> reset() => _performSimpleAction(
    GameAction.reset,
    _gameRepository.reset,
    'We could not reset. Try again.',
  );

  Future<void> abandon() => _performSimpleAction(
    GameAction.abandon,
    _gameRepository.abandon,
    'We could not abandon this game. Try again.',
  );

  Future<void> _performSimpleAction(
    GameAction action,
    Future<void> Function(String gameId) request,
    String fallbackMessage,
  ) async {
    final game = state.game;
    if (game == null || state.isBusy) {
      return;
    }

    emit(
      state.copyWith(
        activeAction: action,
        clearMessage: true,
        clearHint: true,
      ),
    );

    try {
      await request(game.id);
      await _reloadGame(game.id);
    } on ApiException catch (error) {
      emit(
        state.copyWith(activeAction: GameAction.none, message: error.message),
      );
    } on Exception {
      emit(
        state.copyWith(
          activeAction: GameAction.none,
          message: fallbackMessage,
        ),
      );
    }
  }

  Future<void> _reloadGame(
    String gameId, {
    String? recommendedLinkId,
  }) async {
    final updatedGame = await _getGameWithFallbackDescriptions(gameId);
    final outgoingLinks = await _gameRepository.getOptions(updatedGame.id);
    emit(
      GameDetailsState.success(
        game: updatedGame,
        outgoingLinks: outgoingLinks,
        recommendedLinkId: recommendedLinkId,
      ),
    );
  }

  Future<GameDetails> _getGameWithFallbackDescriptions(String gameId) async {
    final game = await _gameRepository.getGame(gameId);
    if (game.currentDescription != null && game.targetDescription != null) {
      return game;
    }

    final descriptions = await Future.wait<String?>([
      if (game.currentDescription == null)
        _getLinkDescriptionOrNull(game.currentLinkId)
      else
        Future.value(game.currentDescription),
      if (game.targetDescription == null)
        _getLinkDescriptionOrNull(game.targetLinkId)
      else
        Future.value(game.targetDescription),
    ]);

    return game.withWordDescriptions(
      currentDescription: descriptions[0],
      targetDescription: descriptions[1],
    );
  }

  Future<String?> _getLinkDescriptionOrNull(String linkId) async {
    try {
      return await _gameRepository.getLinkDescription(linkId);
    } on Exception {
      return null;
    }
  }
}

class GameDetailsState extends Equatable {
  const GameDetailsState({
    required this.status,
    this.game,
    this.outgoingLinks = const [],
    this.message,
    this.activeAction = GameAction.none,
    this.recommendedLinkId,
  });

  const GameDetailsState.loading() : this(status: GameDetailsStatus.loading);

  const GameDetailsState.success({
    required GameDetails game,
    required List<OutgoingLink> outgoingLinks,
    String? recommendedLinkId,
  }) : this(
         status: GameDetailsStatus.success,
         game: game,
         outgoingLinks: outgoingLinks,
         recommendedLinkId: recommendedLinkId,
       );

  const GameDetailsState.failure({required String message})
    : this(status: GameDetailsStatus.failure, message: message);

  final GameDetailsStatus status;
  final GameDetails? game;
  final List<OutgoingLink> outgoingLinks;
  final String? message;
  final GameAction activeAction;
  final String? recommendedLinkId;

  bool get isBusy => activeAction != GameAction.none;

  GameDetailsState copyWith({
    String? message,
    bool clearMessage = false,
    GameAction? activeAction,
    String? recommendedLinkId,
    bool clearHint = false,
  }) {
    return GameDetailsState(
      status: status,
      game: game,
      outgoingLinks: outgoingLinks,
      message: clearMessage ? null : message ?? this.message,
      activeAction: activeAction ?? this.activeAction,
      recommendedLinkId: clearHint
          ? null
          : recommendedLinkId ?? this.recommendedLinkId,
    );
  }

  @override
  List<Object?> get props => [
    status,
    game,
    outgoingLinks,
    message,
    activeAction,
    recommendedLinkId,
  ];
}
