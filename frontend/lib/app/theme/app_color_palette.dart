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
        seed: Color(0xff1f7a8c),
        primary: Color(0xff1f7a8c),
        primaryPressed: Color(0xff155968),
        primarySoft: Color(0xffd8eef1),
        focus: Color(0xffd49a35),
        focusSoft: Color(0xffffedc4),
        lightBackground: Color(0xfff6f8f2),
        lightSurfaceMuted: Color(0xffe9efe5),
        lightOutline: Color(0xffbdc9c4),
        darkPrimary: Color(0xff7bc9d7),
        darkOnPrimary: Color(0xff06363f),
        darkPrimaryContainer: Color(0xff164f5c),
        darkFocus: Color(0xffe7b85f),
        darkOnFocus: Color(0xff3c2a04),
        darkFocusContainer: Color(0xff5b410c),
      ),
      AppColorPalette.forest => const AppColorScheme(
        seed: Color(0xff2f7d4f),
        primary: Color(0xff2f7d4f),
        primaryPressed: Color(0xff205b39),
        primarySoft: Color(0xffd9efdf),
        focus: Color(0xffb7653d),
        focusSoft: Color(0xffffe3d3),
        lightBackground: Color(0xfff5f8f1),
        lightSurfaceMuted: Color(0xffe7eee2),
        lightOutline: Color(0xffbdc8b8),
        darkPrimary: Color(0xff8fd4a5),
        darkOnPrimary: Color(0xff12351f),
        darkPrimaryContainer: Color(0xff25563a),
        darkFocus: Color(0xffffb28a),
        darkOnFocus: Color(0xff47220e),
        darkFocusContainer: Color(0xff6b3518),
      ),
      AppColorPalette.sunset => const AppColorScheme(
        seed: Color(0xffb95735),
        primary: Color(0xffb95735),
        primaryPressed: Color(0xff853d25),
        primarySoft: Color(0xffffded2),
        focus: Color(0xff476c92),
        focusSoft: Color(0xffdce9f7),
        lightBackground: Color(0xfffbf6f0),
        lightSurfaceMuted: Color(0xfff1e7dd),
        lightOutline: Color(0xffd2beb0),
        darkPrimary: Color(0xffffb195),
        darkOnPrimary: Color(0xff4b1f0e),
        darkPrimaryContainer: Color(0xff73331d),
        darkFocus: Color(0xffa8c8ef),
        darkOnFocus: Color(0xff17324d),
        darkFocusContainer: Color(0xff294d70),
      ),
      AppColorPalette.graphite => const AppColorScheme(
        seed: Color(0xff4f6670),
        primary: Color(0xff4f6670),
        primaryPressed: Color(0xff394a51),
        primarySoft: Color(0xffdde7eb),
        focus: Color(0xff8b6f43),
        focusSoft: Color(0xfff2e6d0),
        lightBackground: Color(0xfff5f6f4),
        lightSurfaceMuted: Color(0xffe7e9e6),
        lightOutline: Color(0xffc2c8c3),
        darkPrimary: Color(0xffb8cbd3),
        darkOnPrimary: Color(0xff203239),
        darkPrimaryContainer: Color(0xff3b4f57),
        darkFocus: Color(0xffd8bd87),
        darkOnFocus: Color(0xff3d2d12),
        darkFocusContainer: Color(0xff5d4620),
      ),
    };
  }
}
