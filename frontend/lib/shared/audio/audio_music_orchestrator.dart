import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/audio/music_routing.dart';

/// Drives background music from a single place instead of scattering
/// `playMusic` calls through feature screens. It switches the track when the
/// route changes, pauses/resumes with the app lifecycle, and — on web, where
/// browsers block autoplay — defers the first playback until the user's first
/// pointer interaction.
class AudioMusicOrchestrator extends StatefulWidget {
  const AudioMusicOrchestrator({
    required this.audioService,
    required this.router,
    required this.child,
    super.key,
  });

  final AudioService audioService;
  final GoRouter router;
  final Widget child;

  @override
  State<AudioMusicOrchestrator> createState() => _AudioMusicOrchestratorState();
}

class _AudioMusicOrchestratorState extends State<AudioMusicOrchestrator>
    with WidgetsBindingObserver {
  // Native platforms allow autoplay; only web must wait for a gesture.
  bool _awaitingFirstGesture = kIsWeb;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    widget.router.routeInformationProvider.addListener(_syncTrack);
    if (!_awaitingFirstGesture) {
      WidgetsBinding.instance.addPostFrameCallback((_) => _syncTrack());
    }
  }

  @override
  void dispose() {
    widget.router.routeInformationProvider.removeListener(_syncTrack);
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      unawaited(widget.audioService.resumeMusic());
    } else {
      unawaited(widget.audioService.pauseMusic());
    }
  }

  void _syncTrack() {
    if (_awaitingFirstGesture) return;
    final path = widget.router.routeInformationProvider.value.uri.path;
    final track = musicTrackForLocation(path);
    if (track == null) {
      unawaited(widget.audioService.stopMusic());
    } else {
      unawaited(widget.audioService.playMusic(track));
    }
  }

  void _handleFirstGesture() {
    if (!_awaitingFirstGesture) return;
    setState(() => _awaitingFirstGesture = false);
    _syncTrack();
  }

  @override
  Widget build(BuildContext context) {
    if (_awaitingFirstGesture) {
      return Listener(
        behavior: HitTestBehavior.translucent,
        onPointerDown: (_) => _handleFirstGesture(),
        child: widget.child,
      );
    }
    return widget.child;
  }
}
