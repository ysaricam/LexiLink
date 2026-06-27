import 'package:flutter/material.dart';

enum AppColorPalette {
  classic('classic'),
  forest('forest'),
  sunset('sunset'),
  graphite('graphite');

  const AppColorPalette(this.id);

  final String id;

  static const AppColorPalette fallback = classic;

  static AppColorPalette fromId(String? id) {
    return values.firstWhere(
      (palette) => palette.id == id,
      orElse: () => fallback,
    );
  }
}

class AppColorScheme {
  const AppColorScheme({
    required this.seed,
    required this.primary,
    required this.primaryPressed,
    required this.primarySoft,
    required this.focus,
    required this.focusSoft,
    required this.lightBackground,
    required this.lightSurfaceMuted,
    required this.lightOutline,
    required this.darkPrimary,
    required this.darkOnPrimary,
    required this.darkPrimaryContainer,
    required this.darkFocus,
    required this.darkOnFocus,
    required this.darkFocusContainer,
  });

  final Color seed;
  final Color primary;
  final Color primaryPressed;
  final Color primarySoft;
  final Color focus;
  final Color focusSoft;
  final Color lightBackground;
  final Color lightSurfaceMuted;
  final Color lightOutline;
  final Color darkPrimary;
  final Color darkOnPrimary;
  final Color darkPrimaryContainer;
  final Color darkFocus;
  final Color darkOnFocus;
  final Color darkFocusContainer;
}

extension AppColorPaletteScheme on AppColorPalette {
  AppColorScheme get scheme {
    return switch (this) {
      AppColorPalette.classic => const AppColorScheme(
        seed: Color(0xff6d28d9),
        primary: Color(0xff6d28d9),
        primaryPressed: Color(0xff5b21b6),
        primarySoft: Color(0xffede9fe),
        focus: Color(0xfff4b400),
        focusSoft: Color(0xfffff2bf),
        lightBackground: Color(0xffeee9ff),
        lightSurfaceMuted: Color(0xffede9fe),
        lightOutline: Color(0xffddd6fe),
        darkPrimary: Color(0xffa855f7),
        darkOnPrimary: Color(0xffffffff),
        darkPrimaryContainer: Color(0xff4c1d95),
        darkFocus: Color(0xffffcc33),
        darkOnFocus: Color(0xff120b2d),
        darkFocusContainer: Color(0xfff4b400),
      ),
      AppColorPalette.forest => const AppColorScheme(
        seed: Color(0xff2f7d4f),
        primary: Color(0xff2f7d4f),
        primaryPressed: Color(0xff205b39),
        primarySoft: Color(0xffe6f4ea),
        focus: Color(0xfff4b400),
        focusSoft: Color(0xfffff2bf),
        lightBackground: Color(0xffeee9ff),
        lightSurfaceMuted: Color(0xffe6f4ea),
        lightOutline: Color(0xffc9dfd0),
        darkPrimary: Color(0xff8fd4a5),
        darkOnPrimary: Color(0xff12351f),
        darkPrimaryContainer: Color(0xff25563a),
        darkFocus: Color(0xffffcc33),
        darkOnFocus: Color(0xff20380f),
        darkFocusContainer: Color(0xfff4b400),
      ),
      AppColorPalette.sunset => const AppColorScheme(
        seed: Color(0xffb95735),
        primary: Color(0xffb95735),
        primaryPressed: Color(0xff853d25),
        primarySoft: Color(0xffffe4dc),
        focus: Color(0xfff4b400),
        focusSoft: Color(0xfffff2bf),
        lightBackground: Color(0xffeee9ff),
        lightSurfaceMuted: Color(0xffffe4dc),
        lightOutline: Color(0xffe8c4b7),
        darkPrimary: Color(0xffffb195),
        darkOnPrimary: Color(0xff4b1f0e),
        darkPrimaryContainer: Color(0xff73331d),
        darkFocus: Color(0xffffcc33),
        darkOnFocus: Color(0xff4a230f),
        darkFocusContainer: Color(0xfff4b400),
      ),
      AppColorPalette.graphite => const AppColorScheme(
        seed: Color(0xff4f6670),
        primary: Color(0xff4f6670),
        primaryPressed: Color(0xff394a51),
        primarySoft: Color(0xffe7ecef),
        focus: Color(0xfff4b400),
        focusSoft: Color(0xfffff2bf),
        lightBackground: Color(0xffeee9ff),
        lightSurfaceMuted: Color(0xffe7ecef),
        lightOutline: Color(0xffc8d0d4),
        darkPrimary: Color(0xffb8cbd3),
        darkOnPrimary: Color(0xff203239),
        darkPrimaryContainer: Color(0xff3b4f57),
        darkFocus: Color(0xffffcc33),
        darkOnFocus: Color(0xff26353b),
        darkFocusContainer: Color(0xfff4b400),
      ),
    };
  }
}
