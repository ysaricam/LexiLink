import 'package:bloc/bloc.dart';
import 'package:lexilink_app/app/theme/app_color_palette.dart';
import 'package:lexilink_app/features/settings/data/color_palette_preferences_repository.dart';

class ColorPaletteCubit extends Cubit<AppColorPalette> {
  ColorPaletteCubit({
    required ColorPalettePreferencesRepository repository,
  }) : _repository = repository,
       super(AppColorPalette.fallback);

  final ColorPalettePreferencesRepository _repository;

  Future<void> load() async {
    try {
      emit(await _repository.load());
    } on Object catch (_) {
      emit(AppColorPalette.fallback);
    }
  }

  Future<void> setPalette(AppColorPalette palette) async {
    if (palette == state) return;
    emit(palette);
    try {
      await _repository.save(palette);
    } on Object catch (_) {
      // Best-effort persistence; the live palette still applied.
    }
  }
}
