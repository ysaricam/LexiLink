import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';
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
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AppBackBar(title: context.l10n.settingsTitle),
              const SizedBox(height: 18),
              const _SettingsHero(),
              const SizedBox(height: 16),
              const _LanguageSection(),
              const SizedBox(height: 14),
              _SettingsSection(
                icon: Icons.volume_up_outlined,
                title: context.l10n.settingsSoundSection,
                children: [
                  _SwitchSettingRow(
                    icon: Icons.music_note_outlined,
                    title: context.l10n.settingsMusic,
                    subtitle: context.l10n.settingsMusicSubtitle,
                    value: settings.musicEnabled,
                    onChanged: (value) => cubit.setMusicEnabled(enabled: value),
                  ),
                  _VolumeSlider(
                    label: context.l10n.settingsMusicVolume,
                    value: settings.musicVolume,
                    enabled: settings.musicEnabled,
                    onChanged: cubit.setMusicVolume,
                  ),
                  const _SectionDivider(),
                  _SwitchSettingRow(
                    icon: Icons.graphic_eq_outlined,
                    title: context.l10n.settingsSfx,
                    subtitle: context.l10n.settingsSfxSubtitle,
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
                    // Preview the new level when the user lets go.
                    onChangeEnd: (_) => context.read<AudioService>().playEffect(
                      SoundEffect.step,
                    ),
                  ),
                ],
              ),
            ],
          );
        },
      ),
    );
  }
}

class _SettingsHero extends StatelessWidget {
  const _SettingsHero();

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.primaryContainer,
        border: Border.all(color: AppPalette.primary.withValues(alpha: 0.16)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: colorScheme.surface.withValues(alpha: 0.84),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(
                Icons.tune_outlined,
                color: colorScheme.primary,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                context.l10n.settingsTitle,
                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  color: colorScheme.onPrimaryContainer,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LanguageSection extends StatelessWidget {
  const _LanguageSection();

  @override
  Widget build(BuildContext context) {
    final language = context.watch<LocaleCubit>().state;
    return _SettingsSection(
      icon: Icons.language_outlined,
      title: context.l10n.languageLabel,
      children: [
        _LanguageRow(language: language),
      ],
    );
  }
}

class _LanguageRow extends StatelessWidget {
  const _LanguageRow({required this.language});

  final AppLanguage language;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: Text(
            language.nativeName,
            style: Theme.of(context).textTheme.titleMedium,
          ),
        ),
        DecoratedBox(
          decoration: BoxDecoration(
            color: AppPalette.primary.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 10),
            child: DropdownButtonHideUnderline(
              child: DropdownButton<AppLanguage>(
                value: language,
                borderRadius: BorderRadius.circular(8),
                icon: const Icon(Icons.keyboard_arrow_down),
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
            ),
          ),
        ),
      ],
    );
  }
}

class _SettingsSection extends StatelessWidget {
  const _SettingsSection({
    required this.icon,
    required this.title,
    required this.children,
  });

  final IconData icon;
  final String title;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(
          color: colorScheme.outline.withValues(alpha: 0.24),
        ),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Icon(icon, color: colorScheme.primary, size: 20),
                const SizedBox(width: 8),
                Text(title, style: Theme.of(context).textTheme.titleMedium),
              ],
            ),
            const SizedBox(height: 14),
            ...children,
          ],
        ),
      ),
    );
  }
}

class _SwitchSettingRow extends StatelessWidget {
  const _SwitchSettingRow({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.value,
    required this.onChanged,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final bool value;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return InkWell(
      borderRadius: BorderRadius.circular(8),
      onTap: () => onChanged(!value),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 4),
        child: Row(
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: colorScheme.primaryContainer,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(icon, color: colorScheme.primary, size: 20),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: Theme.of(context).textTheme.titleMedium),
                  const SizedBox(height: 2),
                  Text(
                    subtitle,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(width: 10),
            Switch(value: value, onChanged: onChanged),
          ],
        ),
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
    final percent = '${(value * 100).round()}%';

    return Opacity(
      opacity: enabled ? 1 : 0.52,
      child: Padding(
        padding: const EdgeInsets.only(top: 10),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    label,
                    style: theme.textTheme.bodyMedium,
                  ),
                ),
                Text(
                  percent,
                  style: theme.textTheme.labelLarge?.copyWith(
                    color: enabled
                        ? theme.colorScheme.primary
                        : theme.disabledColor,
                  ),
                ),
              ],
            ),
            Slider(
              value: value,
              divisions: 10,
              label: percent,
              onChanged: enabled ? onChanged : null,
              onChangeEnd: enabled ? onChangeEnd : null,
            ),
          ],
        ),
      ),
    );
  }
}

class _SectionDivider extends StatelessWidget {
  const _SectionDivider();

  @override
  Widget build(BuildContext context) {
    return Divider(
      height: 26,
      color: Theme.of(context).colorScheme.outline.withValues(alpha: 0.18),
    );
  }
}
