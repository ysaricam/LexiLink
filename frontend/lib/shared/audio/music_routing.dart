import 'package:lexilink_app/shared/audio/audio_service.dart';

/// Maps a router location path to the background-music track that should play
/// there, or `null` for screens that should be silent (splash, admin console).
/// Pure so it can be unit-tested without a router.
MusicTrack? musicTrackForLocation(String path) {
  if (path.startsWith('/games/')) return MusicTrack.game;
  if (path.startsWith('/admin')) return null;
  if (path == '/') return null; // splash
  return MusicTrack.menu;
}
