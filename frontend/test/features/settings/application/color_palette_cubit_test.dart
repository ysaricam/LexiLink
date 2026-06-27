import 'package:bloc_test/bloc_test.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/app/theme/app_color_palette.dart';
import 'package:lexilink_app/features/settings/application/color_palette_cubit.dart';
import 'package:lexilink_app/features/settings/data/color_palette_preferences_repository.dart';

void main() {
  blocTest<ColorPaletteCubit, AppColorPalette>(
    'loads stored palette',
    build: () => ColorPaletteCubit(
      repository: InMemoryColorPalettePreferencesRepository(
        AppColorPalette.forest,
      ),
    ),
    act: (cubit) => cubit.load(),
    expect: () => [AppColorPalette.forest],
  );

  test('setPalette applies and persists selection', () async {
    final repository = InMemoryColorPalettePreferencesRepository();
    final cubit = ColorPaletteCubit(repository: repository);

    await cubit.setPalette(AppColorPalette.sunset);

    expect(cubit.state, AppColorPalette.sunset);
    expect(await repository.load(), AppColorPalette.sunset);
  });

  test('alternate palettes keep the current light surface system', () {
    for (final palette in AppColorPalette.values) {
      expect(palette.scheme.lightBackground, const Color(0xffeee9ff));
      expect(palette.scheme.focus, const Color(0xfff4b400));
      expect(palette.scheme.focusSoft, const Color(0xfffff2bf));
    }
  });
}
