import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/app/router/app_router.dart';
import 'package:lexilink_app/shared/config/feature_flags.dart';

void main() {
  test('monetization entries are disabled by default', () {
    expect(FeatureFlags.adsEnabled, isFalse);
    expect(FeatureFlags.rewardedAdsEnabled, isFalse);
    expect(FeatureFlags.iapEnabled, isFalse);
    expect(
      _routePaths(appRouter.configuration.routes),
      isNot(contains('/earn-diamonds')),
    );
    expect(
      _routePaths(appRouter.configuration.routes),
      isNot(contains('/payments')),
    );
  });
}

List<String> _routePaths(List<RouteBase> routes) {
  final paths = <String>[];
  for (final route in routes) {
    if (route is GoRoute) {
      paths
        ..add(route.path)
        ..addAll(_routePaths(route.routes));
    } else if (route is ShellRoute) {
      paths.addAll(_routePaths(route.routes));
    }
  }
  return paths;
}
