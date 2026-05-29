import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/app/router/app_router.dart';
import 'package:lexilink_app/app/theme/app_theme.dart';
import 'package:lexilink_app/features/settings/application/audio_settings_cubit.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/shared/audio/audio_music_orchestrator.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

class LexiLinkApp extends StatelessWidget {
  const LexiLinkApp({
    required this.audioService,
    this.audioPreferencesRepository,
    super.key,
  });

  /// App-wide audio facade. Provided above the router so every screen can
  /// reach the same instance via `context.read<AudioService>()` — audio is a
  /// genuinely global concern, like session and theme.
  final AudioService audioService;

  /// Overridable for tests; defaults to the device-local SharedPreferences
  /// store in production.
  final AudioPreferencesRepository? audioPreferencesRepository;

  @override
  Widget build(BuildContext context) {
    return RepositoryProvider<AudioService>.value(
      value: audioService,
      child: BlocProvider<AudioSettingsCubit>(
        create: (_) => AudioSettingsCubit(
          audioService: audioService,
          repository: audioPreferencesRepository ??
              SharedPreferencesAudioPreferencesRepository(),
        )..load(),
        child: AudioMusicOrchestrator(
          audioService: audioService,
          router: appRouter,
          child: MaterialApp.router(
            title: 'LexiLink',
            theme: AppTheme.light,
            darkTheme: AppTheme.dark,
            routerConfig: appRouter,
          ),
        ),
      ),
    );
  }
}
