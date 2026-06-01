import 'package:flutter/widgets.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';

/// Ergonomic access to the generated localizations: `context.l10n.appTitle`.
extension L10nX on BuildContext {
  AppLocalizations get l10n => AppLocalizations.of(this);
}
