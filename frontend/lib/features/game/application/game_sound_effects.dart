import 'package:lexilink_app/features/game/application/game_details_cubit.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

/// Pure mapping from a [GameDetailsCubit] state transition to the sound effect
/// that should play (or `null` for silence). Kept side-effect free so it can
/// be unit-tested without a widget tree or audio engine.
///
/// Priority: a win/lose outcome change wins over the action that produced it;
/// an action that finished with an error message plays the error cue; an
/// action that finished cleanly plays its own cue.
SoundEffect? soundEffectForGameTransition(
  GameDetailsState? previous,
  GameDetailsState current,
) {
  if (previous == null) return null;

  final previousGameState = previous.game?.state;
  final currentGameState = current.game?.state;
  if (previousGameState != currentGameState) {
    if (currentGameState == 'Completed') return SoundEffect.win;
    if (currentGameState == 'Failed') return SoundEffect.lose;
  }

  final justFinishedAction =
      previous.activeAction != GameAction.none &&
      current.activeAction == GameAction.none;
  if (!justFinishedAction) return null;

  if (current.message != null) return SoundEffect.error;

  return switch (previous.activeAction) {
    GameAction.step => SoundEffect.step,
    GameAction.hint => SoundEffect.hint,
    GameAction.undo => SoundEffect.undo,
    GameAction.reset => SoundEffect.reset,
    GameAction.abandon => null,
    GameAction.none => null,
  };
}
