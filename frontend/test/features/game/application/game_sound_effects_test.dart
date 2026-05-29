import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/game/application/game_details_cubit.dart';
import 'package:lexilink_app/features/game/application/game_sound_effects.dart';
import 'package:lexilink_app/features/game/data/game_details.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

GameDetails _game(String state) => GameDetails(
  id: 'g1',
  playerId: 'p1',
  categoryId: 'c1',
  difficulty: 'Easy',
  startLinkId: 's',
  startWord: 'start',
  targetLinkId: 't',
  targetWord: 'target',
  currentLinkId: 'cur',
  currentWord: 'current',
  state: state,
  score: 0,
  maxSteps: 10,
  stepsTaken: 1,
  hintsTotal: 0,
  hintsUsed: 0,
  undosTotal: 0,
  undosUsed: 0,
  resetsTotal: 0,
  resetsUsed: 0,
  history: const [],
);

GameDetailsState _state({
  required String gameState,
  GameAction activeAction = GameAction.none,
  String? message,
}) => GameDetailsState(
  status: GameDetailsStatus.success,
  game: _game(gameState),
  activeAction: activeAction,
  message: message,
);

void main() {
  test('returns null when there is no previous state', () {
    expect(
      soundEffectForGameTransition(null, _state(gameState: 'InProgress')),
      isNull,
    );
  });

  test('a completed game plays the win cue', () {
    final previous = _state(
      gameState: 'InProgress',
      activeAction: GameAction.step,
    );
    final current = _state(gameState: 'Completed');

    expect(soundEffectForGameTransition(previous, current), SoundEffect.win);
  });

  test('a failed game plays the lose cue', () {
    final previous = _state(
      gameState: 'InProgress',
      activeAction: GameAction.step,
    );
    final current = _state(gameState: 'Failed');

    expect(soundEffectForGameTransition(previous, current), SoundEffect.lose);
  });

  test('a completed step that did not finish the game plays the step cue', () {
    final previous = _state(
      gameState: 'InProgress',
      activeAction: GameAction.step,
    );
    final current = _state(gameState: 'InProgress');

    expect(soundEffectForGameTransition(previous, current), SoundEffect.step);
  });

  test('each inventory action maps to its own cue', () {
    for (final entry in {
      GameAction.hint: SoundEffect.hint,
      GameAction.undo: SoundEffect.undo,
      GameAction.reset: SoundEffect.reset,
    }.entries) {
      final previous = _state(
        gameState: 'InProgress',
        activeAction: entry.key,
      );
      final current = _state(gameState: 'InProgress');

      expect(soundEffectForGameTransition(previous, current), entry.value);
    }
  });

  test('a failed action plays the error cue', () {
    final previous = _state(
      gameState: 'InProgress',
      activeAction: GameAction.step,
    );
    final current = _state(gameState: 'InProgress', message: 'Invalid move');

    expect(soundEffectForGameTransition(previous, current), SoundEffect.error);
  });

  test('a non-action state change stays silent', () {
    final previous = _state(gameState: 'InProgress');
    final current = _state(gameState: 'InProgress');

    expect(soundEffectForGameTransition(previous, current), isNull);
  });

  test('abandoning the game stays silent', () {
    final previous = _state(
      gameState: 'InProgress',
      activeAction: GameAction.abandon,
    );
    final current = _state(gameState: 'Abandoned');

    expect(soundEffectForGameTransition(previous, current), isNull);
  });
}
