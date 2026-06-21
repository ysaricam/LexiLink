import 'package:lexilink_app/app/theme/app_color_palette.dart';
import 'package:shared_preferences/shared_preferences.dart';

abstract interface class ColorPalettePreferencesRepository {
  Future<AppColorPalette> load();

  Future<void> save(AppColorPalette palette);
}

class InMemoryColorPalettePreferencesRepository
    implements ColorPalettePreferencesRepository {
  InMemoryColorPalettePreferencesRepository([
    AppColorPalette? initialPalette,
  ]) : _palette = initialPalette ?? AppColorPalette.fallback;

  AppColorPalette _palette;

  @override
  Future<AppColorPalette> load() async => _palette;

  @override
  Future<void> save(AppColorPalette palette) async {
    _palette = palette;
  }
}

class SharedPreferencesColorPalettePreferencesRepository
    implements ColorPalettePreferencesRepository {
  SharedPreferencesColorPalettePreferencesRepository({
    SharedPreferencesAsync? preferences,
  }) : _preferences = preferences ?? SharedPreferencesAsync();

  static const _paletteKey = 'lexilink.theme.colorPalette';

  final SharedPreferencesAsync _preferences;

  @override
  Future<AppColorPalette> load() async {
    final id = await _preferences.getString(_paletteKey);
    return AppColorPalette.fromId(id);
  }

  @override
  Future<void> save(AppColorPalette palette) async {
    await _preferences.setString(_paletteKey, palette.id);
  }
}
