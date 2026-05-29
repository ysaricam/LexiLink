import 'package:equatable/equatable.dart';

/// Player-owned audio preferences. Immutable; persisted device-local.
class AudioSettings extends Equatable {
  const AudioSettings({
    this.musicEnabled = true,
    this.sfxEnabled = true,
    this.musicVolume = 0.5,
    this.sfxVolume = 0.8,
  });

  /// The shipped defaults: both channels on at moderate volume.
  static const AudioSettings defaults = AudioSettings();

  final bool musicEnabled;
  final bool sfxEnabled;
  final double musicVolume;
  final double sfxVolume;

  AudioSettings copyWith({
    bool? musicEnabled,
    bool? sfxEnabled,
    double? musicVolume,
    double? sfxVolume,
  }) {
    return AudioSettings(
      musicEnabled: musicEnabled ?? this.musicEnabled,
      sfxEnabled: sfxEnabled ?? this.sfxEnabled,
      musicVolume: musicVolume ?? this.musicVolume,
      sfxVolume: sfxVolume ?? this.sfxVolume,
    );
  }

  @override
  List<Object?> get props => [
    musicEnabled,
    sfxEnabled,
    musicVolume,
    sfxVolume,
  ];
}
