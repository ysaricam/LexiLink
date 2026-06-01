import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/settings/application/audio_settings_cubit.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:lexilink_app/features/settings/data/audio_settings.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

/// Audio preferences screen. Reads the app-wide [AudioSettingsCubit] (provided
/// above the router), so it needs no providers of its own. Changes apply to
/// the audio engine and persist immediately.
class SettingsScreen extends StatelessWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      size: AppScreenSize.compact,
      child: BlocBuilder<AudioSettingsCubit, AudioSettings>(
        builder: (context, settings) {
          final cubit = context.read<AudioSettingsCubit>();
          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              AppBackBar(title: context.l10n.settingsTitle),
              const SizedBox(height: 24),
              const _LanguageSection(),
              const Divider(height: 32),
              Text(
                context.l10n.settingsSoundSection,
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              SwitchListTile(
                title: Text(context.l10n.settingsMusic),
                subtitle: Text(context.l10n.settingsMusicSubtitle),
                value: settings.musicEnabled,
                onChanged: (value) => cubit.setMusicEnabled(enabled: value),
              ),
              _VolumeSlider(
                label: context.l10n.settingsMusicVolume,
                value: settings.musicVolume,
                enabled: settings.musicEnabled,
                onChanged: cubit.setMusicVolume,
              ),
              const Divider(height: 32),
              SwitchListTile(
                title: Text(context.l10n.settingsSfx),
                subtitle: Text(context.l10n.settingsSfxSubtitle),
                value: settings.sfxEnabled,
                onChanged: (value) {
                  cubit.setSfxEnabled(enabled: value);
                  if (value) {
                    // Immediate audible confirmation that SFX are back on.
                    context.read<AudioService>().playEffect(
                      SoundEffect.buttonTap,
                    );
                  }
                },
              ),
              _VolumeSlider(
                label: context.l10n.settingsSfxVolume,
                value: settings.sfxVolume,
                enabled: settings.sfxEnabled,
                onChanged: cubit.setSfxVolume,
                // Preview the new level when the user lets go of the slider.
                onChangeEnd: (_) =>
                    context.read<AudioService>().playEffect(SoundEffect.step),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _LanguageSection extends StatelessWidget {
  const _LanguageSection();

  @override
  Widget build(BuildContext context) {
    final language = context.watch<LocaleCubit>().state;
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          Expanded(
            child: Text(
              context.l10n.languageLabel,
              style: Theme.of(context).textTheme.titleMedium,
            ),
          ),
          DropdownButton<AppLanguage>(
            value: language,
            onChanged: (selected) {
              if (selected != null) {
                context.read<LocaleCubit>().setLanguage(selected);
              }
            },
            items: [
              for (final option in AppLanguage.values)
                DropdownMenuItem<AppLanguage>(
                  value: option,
                  child: Text(option.nativeName),
                ),
            ],
          ),
        ],
      ),
    );
  }
}

class _VolumeSlider extends StatelessWidget {
  const _VolumeSlider({
    required this.label,
    required this.value,
    required this.enabled,
    required this.onChanged,
    this.onChangeEnd,
  });

  final String label;
  final double value;
  final bool enabled;
  final ValueChanged<double> onChanged;
  final ValueChanged<double>? onChangeEnd;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          SizedBox(
            width: 140,
            child: Text(
              label,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: enabled ? null : theme.disabledColor,
              ),
            ),
          ),
          Expanded(
            child: Slider(
              value: value,
              divisions: 10,
              label: '${(value * 100).round()}%',
              onChanged: enabled ? onChanged : null,
              onChangeEnd: enabled ? onChangeEnd : null,
            ),
          ),
        ],
      ),
    );
  }
}
