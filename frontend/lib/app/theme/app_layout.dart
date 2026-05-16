import 'package:flutter/widgets.dart';

enum AppScreenSize {
  compact,
  standard,
  game,
  wide,
}

class AppBreakpoints {
  const AppBreakpoints._();

  static const mobile = 600.0;
  static const desktop = 1024.0;

  static bool isMobile(double width) => width < mobile;

  static bool isDesktop(double width) => width >= desktop;
}

class AppSpacing {
  const AppSpacing._();

  static const small = 8.0;
  static const medium = 12.0;
  static const large = 16.0;
  static const xLarge = 24.0;

  static EdgeInsets screenPaddingForWidth(double width) {
    return EdgeInsets.all(AppBreakpoints.isMobile(width) ? large : xLarge);
  }
}

class AppLayout {
  const AppLayout._();

  static const compactMaxWidth = 560.0;
  static const standardMaxWidth = 720.0;
  static const gameMaxWidth = 840.0;
  static const wideMaxWidth = 960.0;

  static double maxWidthForSize(AppScreenSize size) {
    return switch (size) {
      AppScreenSize.compact => compactMaxWidth,
      AppScreenSize.standard => standardMaxWidth,
      AppScreenSize.game => gameMaxWidth,
      AppScreenSize.wide => wideMaxWidth,
    };
  }
}
