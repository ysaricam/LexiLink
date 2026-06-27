// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for German (`de`).
class AppLocalizationsDe extends AppLocalizations {
  AppLocalizationsDe([String locale = 'de']) : super(locale);

  @override
  String get appTitle => 'WordLope';

  @override
  String get settingsTitle => 'Einstellungen';

  @override
  String get languageLabel => 'Sprache';

  @override
  String get settingsColorSection => 'Farbpalette';

  @override
  String get colorPaletteClassic => 'Klassisch';

  @override
  String get colorPaletteForest => 'Wald';

  @override
  String get colorPaletteSunset => 'Sonnenuntergang';

  @override
  String get colorPaletteGraphite => 'Graphit';

  @override
  String get commonCancel => 'Abbrechen';

  @override
  String get commonApply => 'Übernehmen';

  @override
  String get commonRetry => 'Erneut versuchen';

  @override
  String get commonTryAgain => 'Bitte erneut versuchen.';

  @override
  String get commonStart => 'Start';

  @override
  String get commonStarting => 'Wird gestartet...';

  @override
  String get commonRefresh => 'Aktualisieren';

  @override
  String get sessionStorageFailedTitle => 'Sitzungsspeicher fehlgeschlagen';

  @override
  String get sessionStorageFailedMessage =>
      'Starte die App neu und versuche es erneut.';

  @override
  String get preparingSession => 'Sitzung wird vorbereitet...';

  @override
  String get splashStageSession => 'Sitzung wird geprüft';

  @override
  String get splashStagePlayer => 'Spieler wird vorbereitet';

  @override
  String get splashStageCategories => 'Wortpfade werden geladen';

  @override
  String get splashStageResources => 'Ressourcen werden vorbereitet';

  @override
  String get splashStageReady => 'Bereit';

  @override
  String get splashLoadingSubtitle => 'Startdaten werden vorbereitet.';

  @override
  String get splashFailedTitle => 'Start konnte nicht abgeschlossen werden';

  @override
  String get splashFailedMessage =>
      'Prüfe deine Verbindung und versuche es erneut.';

  @override
  String get navProfile => 'Profil';

  @override
  String get navQuests => 'Aufgaben';

  @override
  String get navMarket => 'Markt';

  @override
  String get navDiamonds => 'Diamanten';

  @override
  String get navEarnDiamonds => 'Diamanten verdienen';

  @override
  String get navSettings => 'Einstellungen';

  @override
  String get loadingCategories => 'Kategorien werden geladen...';

  @override
  String get couldNotLoadCategories =>
      'Kategorien konnten nicht geladen werden';

  @override
  String get preparingCategories => 'Kategorien werden vorbereitet...';

  @override
  String get couldNotStartGame => 'Spiel konnte nicht gestartet werden';

  @override
  String get chooseCategory => 'Kategorie wählen';

  @override
  String get chooseCategorySubtitle =>
      'Wähle ein Wortfeld für deinen nächsten Pfad.';

  @override
  String get noCategoriesTitle => 'Noch keine Kategorien';

  @override
  String get noCategoriesMessage =>
      'Füge Kategorieinhalte hinzu, bevor du ein Spiel startest.';

  @override
  String get startEasyGame => 'Einfaches Spiel starten';

  @override
  String get preparingGame => 'Spiel wird vorbereitet...';

  @override
  String get loadingGame => 'Spiel wird geladen...';

  @override
  String get couldNotLoadGame => 'Spiel konnte nicht geladen werden';

  @override
  String get gameTitle => 'Spiel';

  @override
  String get gameTutorialTitle => 'So spielst du';

  @override
  String get gameTutorialHelpTooltip => 'Tutorial öffnen';

  @override
  String get gameTutorialSkip => 'Überspringen';

  @override
  String get gameTutorialBack => 'Zurück';

  @override
  String get gameTutorialNext => 'Weiter';

  @override
  String get gameTutorialDone => 'Losspielen';

  @override
  String get gameTutorialGoalTitle => 'Erreiche das Zielwort';

  @override
  String get gameTutorialGoalBody =>
      'Jedes Spiel gibt dir ein Startwort und ein Zielwort. Bewege dich über verbundene Wörter, bis du das Ziel erreichst.';

  @override
  String get gameTutorialMoveTitle => 'Wähle das nächste Wort';

  @override
  String get gameTutorialMoveBody =>
      'Die Optionen in der Mitte sind Verbindungen vom aktuellen Wort. Tippe eine an, um deinen Pfad dorthin zu bewegen.';

  @override
  String get gameTutorialStepsTitle => 'Achte auf deine Schritte';

  @override
  String get gameTutorialStepsBody =>
      'Der Zähler oben zeigt, wie viele Schritte du nutzen kannst. Erreiche das Ziel, bevor sie aufgebraucht sind.';

  @override
  String get gameTutorialPowerTitle => 'Nutze Hilfe, wenn du feststeckst';

  @override
  String get gameTutorialPowerBody =>
      'Hinweis führt dich in Richtung des richtigen Pfads. Zurücksetzen bringt dich zum Start; das vorherige Wort nutzt Rückgängig.';

  @override
  String get pickNextWord => 'Wähle das nächste Wort';

  @override
  String get noMovesTitle => 'Keine Züge verfügbar';

  @override
  String get noMovesMessage => 'Dieser Link hat keine ausgehenden Optionen.';

  @override
  String get actionFailed => 'Aktion fehlgeschlagen';

  @override
  String get backToHome => 'Zurück zur Startseite';

  @override
  String get quitGameTitle => 'Spiel beenden?';

  @override
  String get quitGameMessage =>
      'Dadurch brichst du dein aktuelles Spiel ab und erhältst keine Punkte.';

  @override
  String get keepPlaying => 'Weiterspielen';

  @override
  String get quit => 'Beenden';

  @override
  String get anchorTarget => 'Ziel';

  @override
  String get currentLabel => 'Aktuell';

  @override
  String get moreActions => 'Weitere Aktionen';

  @override
  String get resultScore => 'Punkte';

  @override
  String get resultSteps => 'Schritte';

  @override
  String get resultHintsUsed => 'Verwendete Hinweise';

  @override
  String get resultPath => 'Pfad';

  @override
  String hudSteps(int taken, int max) {
    return 'Schritte $taken/$max';
  }

  @override
  String hudHints(int count) {
    return 'Hinweise $count';
  }

  @override
  String hudScore(int score) {
    return 'Punkte $score';
  }

  @override
  String hintAction(int balance) {
    return 'Hinweis ($balance)';
  }

  @override
  String undoAction(int balance) {
    return 'Rückgängig ($balance)';
  }

  @override
  String resetProgress(int balance) {
    return 'Fortschritt zurücksetzen ($balance)';
  }

  @override
  String get actionMakingStep => 'Schritt wird gemacht...';

  @override
  String get actionFindingHint => 'Hinweis wird gesucht...';

  @override
  String get actionUndoing => 'Wird rückgängig gemacht...';

  @override
  String get actionResetting => 'Wird zurückgesetzt...';

  @override
  String get actionAbandoning => 'Wird abgebrochen...';

  @override
  String get actionWorking => 'Wird ausgeführt...';

  @override
  String get outcomeCompletedTitle => 'Abgeschlossen';

  @override
  String outcomeCompletedSubtitle(String target) {
    return 'Du hast $target erreicht.';
  }

  @override
  String get outcomeFailedTitle => 'Keine Schritte mehr';

  @override
  String outcomeFailedSubtitle(String target) {
    return 'Dir gingen die Schritte aus, bevor du $target erreicht hast.';
  }

  @override
  String get outcomeAbandonedTitle => 'Abgebrochen';

  @override
  String get outcomeAbandonedSubtitle => 'Dieses Spiel wurde abgebrochen.';

  @override
  String get outcomeEndedSubtitle => 'Spiel beendet.';

  @override
  String get commonBuy => 'Kaufen';

  @override
  String get commonUnavailable => 'Nicht verfügbar';

  @override
  String get commonProcessing => 'Wird verarbeitet...';

  @override
  String get commonCheckBackLater => 'Schau später wieder vorbei.';

  @override
  String get commonUnlimited => 'unbegrenzt';

  @override
  String get marketTitle => 'Markt';

  @override
  String get openingMarket => 'Markt wird geöffnet...';

  @override
  String get fetchingOffers => 'Angebote werden geladen...';

  @override
  String get marketUnavailable => 'Markt nicht verfügbar';

  @override
  String get noOffersTitle => 'Noch keine Angebote';

  @override
  String promoPrice(Object price) {
    return 'Aktion $price ◆';
  }

  @override
  String price(Object price) {
    return '$price ◆';
  }

  @override
  String stockLabel(Object stock) {
    return 'Bestand: $stock';
  }

  @override
  String yourRemaining(Object remaining) {
    return 'Dein Rest: $remaining';
  }

  @override
  String buyConfirmTitle(Object quantity, Object type) {
    return '$quantity $type kaufen?';
  }

  @override
  String buyConfirmMessage(Object price) {
    return 'Dies kostet $price Diamanten.';
  }

  @override
  String get diamondUnavailable => 'Diamanten nicht verfügbar';

  @override
  String get energyTitle => 'Energie';

  @override
  String get loadingDiamonds => 'Diamanten werden geladen...';

  @override
  String get openingDiamonds => 'Diamanten werden geöffnet...';

  @override
  String get diamondsTitle => 'Diamanten';

  @override
  String get fetchingBundles => 'Diamant-Pakete werden geladen...';

  @override
  String get purchasesUnavailable => 'Käufe nicht verfügbar';

  @override
  String get purchasesUnavailableMessage =>
      'Diamantenkauf ist hier nicht verfügbar.';

  @override
  String get couldNotLoadDiamonds => 'Diamanten konnten nicht geladen werden';

  @override
  String get noBundlesTitle => 'Noch keine Pakete';

  @override
  String diamondBundleAmount(Object amount) {
    return '$amount Diamanten';
  }

  @override
  String diamondsAddedSnack(Object amount) {
    return '+$amount Diamanten hinzugefügt.';
  }

  @override
  String get openingRewards => 'Belohnungen werden geöffnet...';

  @override
  String get rewardWatchedSnack =>
      'Danke fürs Zuschauen! Diamanten kommen nach der Bestätigung an.';

  @override
  String get loadingRewards => 'Belohnungen werden geladen...';

  @override
  String get rewardedUnavailableTitle => 'Belohnte Anzeigen nicht verfügbar';

  @override
  String get rewardedUnavailableMessage =>
      'Öffne die mobile App, um zu schauen und zu verdienen.';

  @override
  String get couldNotLoadRewards => 'Belohnungen konnten nicht geladen werden';

  @override
  String get rewardLoadingAd => 'Anzeige wird geladen...';

  @override
  String get rewardDailyLimitReached => 'Tageslimit erreicht';

  @override
  String rewardWatchEarn(Object amount) {
    return 'Schauen & $amount 💎 verdienen';
  }

  @override
  String rewardCardTitle(Object amount) {
    return 'Schau eine kurze Anzeige und verdiene $amount Diamanten';
  }

  @override
  String rewardToday(Object grants, Object limit, Object remaining) {
    return 'Heute: $grants / $limit gesehen • $remaining übrig';
  }

  @override
  String get rewardFooter =>
      'Diamanten werden gutgeschrieben, nachdem die Belohnung vom Werbenetzwerk bestätigt wurde; das kann einen Moment dauern.';

  @override
  String get commonUnknown => 'unbekannt';

  @override
  String get questsSubtitle =>
      'Schließe Aufgaben ab und verdiene Bonus-Energie.';

  @override
  String get questsLoading => 'Aufgaben werden geladen...';

  @override
  String get questsLoadError => 'Aufgaben konnten nicht geladen werden';

  @override
  String get noQuestsTitle => 'Noch keine Aufgaben';

  @override
  String get noQuestsMessage =>
      'Schließe ein Spiel ab, dann erscheinen hier Aufgaben.';

  @override
  String get questClaiming => 'Wird abgeholt...';

  @override
  String get questClaimReward => 'Belohnung abholen';

  @override
  String get questStateReady => 'Bereit';

  @override
  String get questStateActive => 'Aktiv';

  @override
  String get questStateClaimed => 'Abgeholt';

  @override
  String get loadingProfile => 'Profil wird geladen...';

  @override
  String get couldNotLoadProfile => 'Profil konnte nicht geladen werden';

  @override
  String get noProfileTitle => 'Noch kein Profil';

  @override
  String get noProfileMessage =>
      'Starte eine Gastsitzung, um dein Profil zu sehen.';

  @override
  String get viewLeaderboard => 'Bestenliste ansehen';

  @override
  String get guestPlayer => 'Gastspieler';

  @override
  String get guestSession => 'Gastsitzung';

  @override
  String providersLinked(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count Anbieter verknüpft',
      one: '1 Anbieter verknüpft',
    );
    return '$_temp0';
  }

  @override
  String get linkAccount => 'Konto verknüpfen';

  @override
  String get linkGoogle => 'Google verknüpfen';

  @override
  String get linkApple => 'Apple verknüpfen';

  @override
  String get linkingAccount => 'Wird verknüpft...';

  @override
  String get accountLinked => 'Konto verknüpft.';

  @override
  String get statGamesCompleted => 'Abgeschlossene Spiele';

  @override
  String get statBestScore => 'Beste Punktzahl';

  @override
  String get statTotalScore => 'Gesamtpunktzahl';

  @override
  String get statLastCompleted => 'Zuletzt abgeschlossen';

  @override
  String get leaderboardTitle => 'Bestenliste';

  @override
  String get loadingLeaderboard => 'Bestenliste wird geladen...';

  @override
  String get couldNotLoadLeaderboard =>
      'Bestenliste konnte nicht geladen werden';

  @override
  String get noScoresTitle => 'Noch keine Punkte';

  @override
  String get leaderboardAllTime => 'Allzeit';

  @override
  String get leaderboardDaily => 'Täglich';

  @override
  String get leaderboardWeekly => 'Wöchentlich';

  @override
  String get leaderboardAllTimeDesc =>
      'Gesamtpunktzahl aller Zeiten über alle Spieler.';

  @override
  String get leaderboardDailyDesc => 'Gesamtpunktzahl heute (UTC).';

  @override
  String get leaderboardWeeklyDesc =>
      'Gesamtpunktzahl diese Woche (UTC, Start Montag).';

  @override
  String get leaderboardAllTimeEmpty =>
      'Schließe ein Spiel ab, um in der Bestenliste zu erscheinen.';

  @override
  String get leaderboardDailyEmpty => 'Heute wurden noch keine Punkte erfasst.';

  @override
  String get leaderboardWeeklyEmpty =>
      'Diese Woche wurden noch keine Punkte erfasst.';

  @override
  String get settingsSoundSection => 'Ton';

  @override
  String get settingsMusic => 'Musik';

  @override
  String get settingsMusicSubtitle => 'Hintergrundmusik während des Spiels';

  @override
  String get settingsMusicVolume => 'Musiklautstärke';

  @override
  String get settingsSfx => 'Soundeffekte';

  @override
  String get settingsSfxSubtitle => 'Tippen, Züge, Siege und Belohnungen';

  @override
  String get settingsSfxVolume => 'Soundeffekt-Lautstärke';

  @override
  String get commonSave => 'Speichern';

  @override
  String get commonCreate => 'Erstellen';

  @override
  String get commonClose => 'Schließen';

  @override
  String get commonClear => 'Leeren';

  @override
  String get commonEdit => 'Bearbeiten';

  @override
  String get commonDeactivate => 'Deaktivieren';

  @override
  String get commonReset => 'Zurücksetzen';

  @override
  String get commonLoad => 'Laden';

  @override
  String get commonRequired => 'Erforderlich';

  @override
  String get commonMustBeNumber => 'Muss eine Zahl sein';

  @override
  String get commonEnterNumber => 'Zahl eingeben';

  @override
  String get commonGreaterThanZero => 'Muss größer als 0 sein';

  @override
  String get commonNonNegative => 'Muss >= 0 sein';

  @override
  String get commonNoDash => '—';

  @override
  String get adminLabel => 'Admin';

  @override
  String get adminConsole => 'Admin-Konsole';

  @override
  String adminMobileTitle(Object title) {
    return 'Admin · $title';
  }

  @override
  String get adminSignOut => 'Abmelden';

  @override
  String get adminNavQuests => 'Aufgaben';

  @override
  String get adminNavPlayers => 'Spieler';

  @override
  String get adminNavEnergy => 'Energie';

  @override
  String get adminNavHint => 'Hinweis';

  @override
  String get adminNavUndo => 'Rückgängig';

  @override
  String get adminNavReset => 'Reset';

  @override
  String get adminNavDiamond => 'Diamant';

  @override
  String get adminNavMarket => 'Markt';

  @override
  String get adminNavContent => 'Inhalt';

  @override
  String get adminNavAudit => 'Audit';

  @override
  String get adminSignInTitle => 'Admin-Anmeldung';

  @override
  String get adminSignInHelp =>
      'Entwicklungsprüfer: Gib deine Admin-E-Mail und exakt das Token \"dev:admin:<email>\" ein. Production-SSO folgt später.';

  @override
  String get adminEmailLabel => 'E-Mail';

  @override
  String get adminExternalTokenLabel => 'Externes Token';

  @override
  String get adminSigningIn => 'Anmeldung läuft...';

  @override
  String get adminSignIn => 'Anmelden';

  @override
  String get adminSignInFailed => 'Anmeldung fehlgeschlagen.';

  @override
  String get adminLookUp => 'Suchen';

  @override
  String get adminPlayerGuid => 'Spieler-GUID';

  @override
  String get adminPlayerHandle => 'Spieler-Handle';

  @override
  String get adminPlayerId => 'Spieler-ID';

  @override
  String get adminLookupFailed => 'Suche fehlgeschlagen.';

  @override
  String get adminNoPlayerFound => 'Kein Spieler gefunden.';

  @override
  String get adminPlayerConsoleTitle => 'Spielerkonsole';

  @override
  String get adminPlayerConsoleHelp =>
      'Spieler im Format DisplayName#1234 nach Handle suchen.';

  @override
  String get adminId => 'ID';

  @override
  String get adminLocale => 'Sprache';

  @override
  String get adminAuthProvidersLinked => 'Verknüpfte Auth-Anbieter';

  @override
  String get adminCreated => 'Erstellt';

  @override
  String get adminBannedAt => 'Gesperrt am';

  @override
  String get adminReason => 'Grund';

  @override
  String get adminBan => 'Sperren';

  @override
  String get adminUnban => 'Sperre aufheben';

  @override
  String get adminBanned => 'Gesperrt';

  @override
  String get adminGuest => 'Gast';

  @override
  String get adminBanPlayerTitle => 'Spieler sperren';

  @override
  String get adminUnbanPlayerTitle => 'Spieler entsperren?';

  @override
  String adminUnbanPlayerMessage(Object handle) {
    return '$handle kann sich wieder anmelden. Der Sperrgrund bleibt im Audit-Protokoll erhalten.';
  }

  @override
  String get adminQuestsTitle => 'Aufgabendefinitionen';

  @override
  String get adminNewQuest => 'Neue Aufgabe';

  @override
  String get adminQuestLoadError =>
      'Aufgabendefinitionen konnten nicht geladen werden.';

  @override
  String get adminNoQuestDefinitions =>
      'Noch keine Aufgabendefinitionen. Starte mit \"Neue Aufgabe\".';

  @override
  String adminQuestPrerequisite(Object name) {
    return 'Voraussetzung: $name';
  }

  @override
  String get adminQuestEditTooltip => 'Bearbeiten';

  @override
  String get adminQuestDeactivateTooltip => 'Deaktivieren';

  @override
  String get adminQuestReactivateTooltip => 'Reaktivieren';

  @override
  String get adminInactive => 'Inaktiv';

  @override
  String get adminQuestDeactivateTitle => 'Aufgabendefinition deaktivieren?';

  @override
  String adminQuestDeactivateMessage(Object name) {
    return '\"$name\" wird nicht mehr an Spieler ausgegeben. Bestehender Spielerfortschritt bleibt unverändert.';
  }

  @override
  String get adminQuestFormEditTitle => 'Aufgabendefinition bearbeiten';

  @override
  String get adminQuestFormCreateTitle => 'Neue Aufgabendefinition';

  @override
  String get adminName => 'Name';

  @override
  String get adminImmutableAfterCreate =>
      'Kann nach der Erstellung nicht geändert werden.';

  @override
  String get adminNameRequired => 'Name ist erforderlich';

  @override
  String get adminMax64 => 'Maximal 64 Zeichen';

  @override
  String get adminDescription => 'Beschreibung';

  @override
  String get adminMax256 => 'Maximal 256 Zeichen';

  @override
  String get adminTrigger => 'Auslöser';

  @override
  String get adminTriggerRequired => 'Auslöser ist erforderlich';

  @override
  String get adminThreshold => 'Schwelle';

  @override
  String get adminEnergyReward => 'Energiebelohnung';

  @override
  String get adminHintReward => 'Hinweisbelohnung';

  @override
  String get adminUndoReward => 'Rückgängig-Belohnung';

  @override
  String get adminResetReward => 'Reset-Belohnung';

  @override
  String get adminDiamondReward => 'Diamantbelohnung';

  @override
  String get adminProgressBaseline => 'Fortschrittsbasis';

  @override
  String get adminProgressBaselineHelp => 'Nur für \"Gesamtspiele\" relevant.';

  @override
  String get adminPrerequisiteOptional => 'Voraussetzung (optional)';

  @override
  String get adminNoPrerequisite => '— keine —';

  @override
  String get adminRewardPositiveRequired =>
      'Mindestens eine Belohnung (Energie, Hinweis, Rückgängig, Reset, Diamant) muss größer als 0 sein.';

  @override
  String get adminPositiveRequired => 'Muss größer als 0 sein';

  @override
  String get adminNotNegative => 'Darf nicht kleiner als 0 sein';

  @override
  String get adminEnergyConsoleTitle => 'Energiekonsole';

  @override
  String get adminEnergyConsoleHelp =>
      'Per Spieler-GUID suchen, dann Wert setzen / Bonus gewähren / zurücksetzen. Bonus kann den Maximalwert bewusst überschreiten.';

  @override
  String get adminNoEnergyAggregate => 'Kein Energie-Aggregat.';

  @override
  String get adminOverMax => 'Über Maximum';

  @override
  String get adminFull => 'Voll';

  @override
  String get adminRechargeInterval => 'Aufladeintervall';

  @override
  String get adminLastRefilled => 'Zuletzt aufgefüllt';

  @override
  String get adminNextRefillIn => 'Nächste Auffüllung in';

  @override
  String get adminFullyRefilledAt => 'Vollständig aufgefüllt um';

  @override
  String get adminSetAmount => 'Menge setzen';

  @override
  String get adminGrantBonus => 'Bonus gewähren';

  @override
  String get adminResetToFull => 'Auf voll zurücksetzen';

  @override
  String get adminSetEnergyAmountTitle => 'Energiemenge setzen';

  @override
  String get adminNewCurrentAmount => 'Neue aktuelle Menge';

  @override
  String get adminSetEnergyHelper =>
      'Setzt die aktuelle Energie des Spielers auf diesen Wert (>= 0).';

  @override
  String get adminGrantBonusEnergyTitle => 'Bonusenergie gewähren';

  @override
  String get adminBonusAmount => 'Bonusmenge';

  @override
  String get adminGrantEnergyHelper =>
      'Wird zur aktuellen Energie addiert, aber nur bis zum Maximum.';

  @override
  String get adminResetEnergyTitle => 'Energie zurücksetzen?';

  @override
  String get adminResetEnergyMessage =>
      'Setzt den Spieler auf maximale Energie zurück.';

  @override
  String get adminHintConsoleTitle => 'Hinweiskonsole';

  @override
  String get adminHintConsoleHelp =>
      'Per Spieler-GUID suchen, dann Guthaben setzen / gewähren / zurücksetzen. Hinweis-Inventar hat kein Maximum.';

  @override
  String get adminNoHintInventory => 'Kein Hinweis-Inventar.';

  @override
  String get adminSetBalance => 'Guthaben setzen';

  @override
  String get adminGrantHints => 'Hinweise gewähren';

  @override
  String get adminResetToZero => 'Auf null setzen';

  @override
  String get adminSetHintBalanceTitle => 'Hinweisguthaben setzen';

  @override
  String get adminNewBalance => 'Neues Guthaben';

  @override
  String get adminSetHintHelper =>
      'Setzt das Hinweisguthaben des Spielers auf diesen Wert (>= 0).';

  @override
  String get adminHintAmount => 'Hinweismenge';

  @override
  String get adminGrantHintHelper =>
      'Wird zum vorhandenen Guthaben addiert; kein Maximum.';

  @override
  String get adminResetHintTitle => 'Hinweisguthaben zurücksetzen?';

  @override
  String get adminResetHintMessage =>
      'Setzt das Hinweisguthaben des Spielers auf null.';

  @override
  String get adminUndoConsoleTitle => 'Rückgängig-Konsole';

  @override
  String get adminUndoConsoleHelp =>
      'Per Spieler-GUID suchen, dann Guthaben setzen / gewähren / zurücksetzen. Rückgängig-Inventar hat kein Maximum.';

  @override
  String get adminNoUndoInventory => 'Kein Rückgängig-Inventar.';

  @override
  String get adminGrantUndos => 'Rückgängig gewähren';

  @override
  String get adminSetUndoBalanceTitle => 'Rückgängig-Guthaben setzen';

  @override
  String get adminSetUndoHelper =>
      'Setzt das Rückgängig-Guthaben des Spielers auf diesen Wert (>= 0).';

  @override
  String get adminUndoAmount => 'Rückgängig-Menge';

  @override
  String get adminGrantUndoHelper =>
      'Wird zum vorhandenen Guthaben addiert; kein Maximum.';

  @override
  String get adminResetUndoTitle => 'Rückgängig-Guthaben zurücksetzen?';

  @override
  String get adminResetUndoMessage =>
      'Setzt das Rückgängig-Guthaben des Spielers auf null.';

  @override
  String get adminResetConsoleTitle => 'Reset-Konsole';

  @override
  String get adminResetConsoleHelp =>
      'Per Spieler-GUID suchen, dann Guthaben setzen / gewähren / zurücksetzen. Reset-Inventar hat kein Maximum.';

  @override
  String get adminNoResetInventory => 'Kein Reset-Inventar.';

  @override
  String get adminGrantResets => 'Resets gewähren';

  @override
  String get adminSetResetBalanceTitle => 'Reset-Guthaben setzen';

  @override
  String get adminSetResetHelper =>
      'Setzt das Reset-Guthaben des Spielers auf diesen Wert (>= 0).';

  @override
  String get adminResetAmount => 'Reset-Menge';

  @override
  String get adminGrantResetHelper =>
      'Wird zum vorhandenen Guthaben addiert; kein Maximum.';

  @override
  String get adminResetResetTitle => 'Reset-Guthaben zurücksetzen?';

  @override
  String get adminResetResetMessage =>
      'Setzt das Reset-Guthaben des Spielers auf null.';

  @override
  String get adminDiamondConsoleTitle => 'Diamantkonsole';

  @override
  String get adminDiamondConsoleHelp =>
      'Per Spieler-GUID suchen, dann setzen / gewähren / zurücksetzen. Diamant ist eine unbegrenzte Währung.';

  @override
  String get adminNoDiamondInventory => 'Kein Diamant-Inventar.';

  @override
  String get adminGrantDiamonds => 'Diamanten gewähren';

  @override
  String get adminSetDiamondBalanceTitle => 'Diamantguthaben setzen';

  @override
  String get adminBalance => 'Guthaben';

  @override
  String get adminAmount => 'Menge';

  @override
  String get adminResetDiamondTitle => 'Diamantguthaben zurücksetzen?';

  @override
  String get adminResetDiamondMessage =>
      'Dies setzt das Diamantguthaben auf null.';

  @override
  String get adminMarketConsoleTitle => 'Marktkonsole';

  @override
  String get adminMarketConsoleHelp =>
      'Shop-Kategorien, diamantbepreiste Artikel und Kaufhistorie von Spielern verwalten.';

  @override
  String get adminMarketCategories => 'Kategorien';

  @override
  String get adminMarketItems => 'Artikel';

  @override
  String get adminMarketOrders => 'Bestellungen';

  @override
  String get adminNewCategory => 'Neue Kategorie';

  @override
  String get adminEditCategory => 'Kategorie bearbeiten';

  @override
  String adminSortStatus(Object sortOrder, Object status) {
    return 'Sortierung $sortOrder - $status';
  }

  @override
  String get adminActive => 'Aktiv';

  @override
  String get adminNoMarketCategories => 'Noch keine Marktkategorien.';

  @override
  String get adminNewItem => 'Neuer Artikel';

  @override
  String get adminEditItem => 'Artikel bearbeiten';

  @override
  String adminMarketItemSubtitle(Object category, Object price, Object stock) {
    return '$category - $price Diamanten - Bestand $stock';
  }

  @override
  String get adminStock => 'Bestand';

  @override
  String get adminNoMarketItems => 'Noch keine Marktartikel.';

  @override
  String get adminNoMarketOrders =>
      'Keine Marktbestellungen für diesen Spieler.';

  @override
  String get adminSortOrder => 'Sortierung';

  @override
  String get adminIcon => 'Icon';

  @override
  String get adminVisibilityStarts => 'Sichtbarkeit beginnt (ISO, optional)';

  @override
  String get adminVisibilityEnds => 'Sichtbarkeit endet (ISO, optional)';

  @override
  String get adminNormal => 'Normal';

  @override
  String get adminPromotion => 'Aktion';

  @override
  String get adminCategory => 'Kategorie';

  @override
  String get adminItemType => 'Artikeltyp';

  @override
  String get adminQuantity => 'Menge';

  @override
  String get adminPriceDiamonds => 'Preis in Diamanten';

  @override
  String get adminPromoPrice => 'Aktionspreis';

  @override
  String get adminPromotionStarts => 'Aktion beginnt';

  @override
  String get adminPromotionEnds => 'Aktion endet';

  @override
  String get adminMaxStock => 'Maximaler Bestand';

  @override
  String get adminPerPlayerLimit => 'Limit pro Spieler';

  @override
  String get adminLimitWindow => 'Limitfenster';

  @override
  String get adminMustBeLowerThanPrice => 'Muss niedriger als der Preis sein';

  @override
  String get adminMustBeAfterStart => 'Muss nach dem Start liegen';

  @override
  String get adminContentConsoleTitle => 'Inhaltskonsole';

  @override
  String get adminContentLanguageFilter => 'Sprachfilter';

  @override
  String get adminContentAllLanguages => 'Alle Sprachen';

  @override
  String get adminContentNewCategory => 'Neue Inhaltskategorie';

  @override
  String get adminContentEditCategory => 'Inhaltskategorie bearbeiten';

  @override
  String get adminContentLanguage => 'Inhaltssprache';

  @override
  String get adminContentNoCategories => 'Noch keine Inhaltskategorien.';

  @override
  String adminContentLinkCount(Object count) {
    return '$count Links';
  }

  @override
  String get adminAuditLogTitle => 'Audit-Protokoll';

  @override
  String get adminAuditHelp =>
      'Neueste zuerst. Filter sind optional; Seitengröße 50.';

  @override
  String get adminAdminUserId => 'Admin-Benutzer-ID (GUID)';

  @override
  String get adminTargetType => 'Zieltyp (z. B. Games.Category)';

  @override
  String get adminTargetId => 'Ziel-ID';

  @override
  String get adminApplyFilters => 'Filter anwenden';

  @override
  String get adminFailedLoadAudit =>
      'Audit-Protokoll konnte nicht geladen werden.';

  @override
  String get adminNoAuditEntries =>
      'Keine Audit-Einträge für die aktuellen Filter.';

  @override
  String adminOffset(Object offset) {
    return 'Offset $offset';
  }

  @override
  String get adminPrev => 'Zurück';

  @override
  String get adminNext => 'Weiter';

  @override
  String adminAuditAdmin(Object id) {
    return 'Admin: $id';
  }

  @override
  String get adminViewPayload => 'Payload anzeigen';

  @override
  String get commonLoading => 'Wird geladen...';

  @override
  String get adminQuestTriggerTotal => 'Gesamtspiele';

  @override
  String get adminQuestTriggerDaily => 'Tägliche Spiele';

  @override
  String get adminQuestTriggerAuthProvider => 'Konto verknüpft';

  @override
  String get adminProgressFromSnapshot => 'Ab diesem Punkt';

  @override
  String get adminProgressFromExistingTotal => 'Alle Zeiten';

  @override
  String get adminMarketTypeEnergy => 'Energie';

  @override
  String get adminMarketTypeHint => 'Hinweis';

  @override
  String get adminMarketTypeUndo => 'Rückgängig';

  @override
  String get adminMarketTypeReset => 'Reset';

  @override
  String get adminMarketTypeDiamond => 'Diamant';

  @override
  String get adminLimitLifetime => 'Lebenslang';

  @override
  String get adminLimitDaily => 'Täglich';

  @override
  String get adminLimitPerPromo => 'Pro Aktion';

  @override
  String adminMarketOrderSubtitle(Object price, Object purchasedAt) {
    return '$price Diamanten - $purchasedAt';
  }
}
