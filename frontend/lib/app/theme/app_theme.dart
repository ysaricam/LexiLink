import 'package:flutter/material.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';

class AppTheme {
  const AppTheme._();

  static ThemeData get light {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: AppPalette.seed,
      primary: AppPalette.primary,
      secondary: AppPalette.focus,
      surface: AppPalette.lightSurface,
      error: AppPalette.danger,
    );

    return ThemeData(
      colorScheme: colorScheme.copyWith(
        primary: AppPalette.primary,
        onPrimary: Colors.white,
        primaryContainer: AppPalette.primarySoft,
        onPrimaryContainer: AppPalette.lightText,
        secondary: AppPalette.focus,
        onSecondary: AppPalette.lightText,
        secondaryContainer: AppPalette.focusSoft,
        onSecondaryContainer: AppPalette.lightText,
        surface: AppPalette.lightSurface,
        onSurface: AppPalette.lightText,
        surfaceContainerHighest: AppPalette.lightSurfaceMuted,
        outline: const Color(0xffbdc9c4),
      ),
      useMaterial3: true,
      scaffoldBackgroundColor: AppPalette.lightBackground,
      textTheme: _textTheme,
      filledButtonTheme: _filledButtonTheme,
      outlinedButtonTheme: _outlinedButtonTheme,
      textButtonTheme: _textButtonTheme,
      cardTheme: _cardTheme,
    );
  }

  static ThemeData get dark {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: AppPalette.seed,
      primary: const Color(0xff7bc9d7),
      secondary: const Color(0xffe7b85f),
      surface: AppPalette.darkSurface,
      error: const Color(0xffffb4ab),
      brightness: Brightness.dark,
    );

    return ThemeData(
      colorScheme: colorScheme.copyWith(
        primary: const Color(0xff7bc9d7),
        onPrimary: const Color(0xff06363f),
        primaryContainer: const Color(0xff164f5c),
        onPrimaryContainer: AppPalette.darkText,
        secondary: const Color(0xffe7b85f),
        onSecondary: const Color(0xff3c2a04),
        secondaryContainer: const Color(0xff5b410c),
        onSecondaryContainer: AppPalette.darkText,
        surface: AppPalette.darkSurface,
        onSurface: AppPalette.darkText,
        surfaceContainerHighest: AppPalette.darkSurfaceMuted,
        outline: const Color(0xff53625d),
      ),
      useMaterial3: true,
      scaffoldBackgroundColor: AppPalette.darkBackground,
      textTheme: _textTheme,
      filledButtonTheme: _filledButtonTheme,
      outlinedButtonTheme: _outlinedButtonTheme,
      textButtonTheme: _textButtonTheme,
      cardTheme: _cardTheme,
    );
  }

  static const _textTheme = TextTheme(
    displaySmall: TextStyle(
      fontSize: 32,
      fontWeight: FontWeight.w700,
      height: 1.12,
    ),
    headlineMedium: TextStyle(
      fontSize: 28,
      fontWeight: FontWeight.w700,
      height: 1.16,
    ),
    titleLarge: TextStyle(
      fontSize: 22,
      fontWeight: FontWeight.w700,
      height: 1.2,
    ),
    titleMedium: TextStyle(
      fontSize: 17,
      fontWeight: FontWeight.w600,
      height: 1.28,
    ),
    bodyLarge: TextStyle(
      fontSize: 16,
      fontWeight: FontWeight.w400,
      height: 1.45,
    ),
    bodyMedium: TextStyle(
      fontSize: 14,
      fontWeight: FontWeight.w400,
      height: 1.42,
    ),
    labelLarge: TextStyle(
      fontSize: 15,
      fontWeight: FontWeight.w600,
      height: 1.2,
    ),
    labelSmall: TextStyle(
      fontSize: 12,
      fontWeight: FontWeight.w600,
      height: 1.2,
    ),
  );

  static final _filledButtonTheme = FilledButtonThemeData(
    style: FilledButton.styleFrom(
      minimumSize: const Size(48, 48),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      textStyle: _textTheme.labelLarge,
    ),
  );

  static final _outlinedButtonTheme = OutlinedButtonThemeData(
    style: OutlinedButton.styleFrom(
      minimumSize: const Size(48, 48),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      textStyle: _textTheme.labelLarge,
    ),
  );

  static final _textButtonTheme = TextButtonThemeData(
    style: TextButton.styleFrom(
      minimumSize: const Size(48, 48),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      textStyle: _textTheme.labelLarge,
    ),
  );

  static const _cardTheme = CardThemeData(
    elevation: 0,
    margin: EdgeInsets.zero,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.all(Radius.circular(8)),
    ),
  );
}
