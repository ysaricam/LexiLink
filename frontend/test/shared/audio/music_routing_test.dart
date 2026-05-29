import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/audio/music_routing.dart';

void main() {
  test('game routes use the in-game track', () {
    expect(musicTrackForLocation('/games/abc-123'), MusicTrack.game);
  });

  test('player areas use the menu track', () {
    for (final path in [
      '/home',
      '/categories',
      '/profile',
      '/leaderboard',
      '/quests',
      '/market',
      '/payments',
      '/settings',
    ]) {
      expect(musicTrackForLocation(path), MusicTrack.menu, reason: path);
    }
  });

  test('splash and admin console stay silent', () {
    expect(musicTrackForLocation('/'), isNull);
    expect(musicTrackForLocation('/admin'), isNull);
    expect(musicTrackForLocation('/admin/quests'), isNull);
    expect(musicTrackForLocation('/admin/login'), isNull);
  });
}
