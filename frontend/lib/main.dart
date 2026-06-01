import 'dart:async';

import 'package:flutter/widgets.dart';
import 'package:flutter_web_plugins/url_strategy.dart';
import 'package:lexilink_app/app/app.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  // Default hash URLs (e.g. /#/admin/login) hide deep links behind a
  // fragment, which breaks address-bar navigation to /admin/login and
  // forces the splash → /home flow to swallow the requested path.
  usePathUrlStrategy();
  final adsService = AdsService();
  // Fire-and-forget: ad SDK init is best-effort and must not block startup.
  unawaited(adsService.initialize());
  runApp(LexiLinkApp(audioService: AudioService(), adsService: adsService));
}
