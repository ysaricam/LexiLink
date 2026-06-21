import 'package:flutter/material.dart';
import 'package:lexilink_app/app/theme/app_color_palette.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';

class AppTheme {
  const AppTheme._();

  static ThemeData light([
    AppColorPalette palette = AppColorPalette.fallback,
  ]) {
    final appColors = palette.scheme;
    final colorScheme = ColorScheme.fromSeed(
      seedColor: appColors.seed,
      primary: appColors.primary,
      secondary: appColors.focus,
      surface: AppPalette.lightSurface,
      error: AppPalette.danger,
    );

    return ThemeData(
      colorScheme: colorScheme.copyWith(
        primary: appColors.primary,
        onPrimary: Colors.white,
        primaryContainer: appColors.primarySoft,
        onPrimaryContainer: AppPalette.lightText,
        secondary: appColors.focus,
        onSecondary: AppPalette.lightText,
        secondaryContainer: appColors.focusSoft,
        onSecondaryContainer: AppPalette.lightText,
        surface: AppPalette.lightSurface,
        onSurface: AppPalette.lightText,
        surfaceContainerHighest: appColors.lightSurfaceMuted,
        outline: appColors.lightOutline,
      ),
      useMaterial3: true,
      scaffoldBackgroundColor: appColors.lightBackground,
      textTheme: _textTheme,
      filledButtonTheme: _filledButtonTheme,
      outlinedButtonTheme: _outlinedButtonTheme,
      textButtonTheme: _textButtonTheme,
      cardTheme: _cardTheme,
    );
  }

  static ThemeData dark([
    AppColorPalette palette = AppColorPalette.fallback,
  ]) {
    final appColors = palette.scheme;
    final colorScheme = ColorScheme.fromSeed(
      seedColor: appColors.seed,
      primary: appColors.darkPrimary,
      secondary: appColors.darkFocus,
      surface: AppPalette.darkSurface,
      error: const Color(0xffffb4ab),
      brightness: Brightness.dark,
    );

    return ThemeData(
      colorScheme: colorScheme.copyWith(
        primary: appColors.darkPrimary,
        onPrimary: appColors.darkOnPrimary,
        primaryContainer: appColors.darkPrimaryContainer,
        onPrimaryContainer: AppPalette.darkText,
        secondary: appColors.darkFocus,
        onSecondary: appColors.darkOnFocus,
        secondaryContainer: appColors.darkFocusContainer,
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
