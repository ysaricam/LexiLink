import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_de.dart';
import 'app_localizations_en.dart';
import 'app_localizations_es.dart';
import 'app_localizations_fr.dart';
import 'app_localizations_tr.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('de'),
    Locale('en'),
    Locale('es'),
    Locale('fr'),
    Locale('tr'),
  ];

  /// The application name shown as the app title.
  ///
  /// In en, this message translates to:
  /// **'WordLope'**
  String get appTitle;

  /// Title of the settings screen.
  ///
  /// In en, this message translates to:
  /// **'Settings'**
  String get settingsTitle;

  /// Label for the language selector in settings.
  ///
  /// In en, this message translates to:
  /// **'Language'**
  String get languageLabel;

  /// No description provided for @settingsColorSection.
  ///
  /// In en, this message translates to:
  /// **'Color palette'**
  String get settingsColorSection;

  /// No description provided for @colorPaletteClassic.
  ///
  /// In en, this message translates to:
  /// **'Classic'**
  String get colorPaletteClassic;

  /// No description provided for @colorPaletteForest.
  ///
  /// In en, this message translates to:
  /// **'Forest'**
  String get colorPaletteForest;

  /// No description provided for @colorPaletteSunset.
  ///
  /// In en, this message translates to:
  /// **'Sunset'**
  String get colorPaletteSunset;

  /// No description provided for @colorPaletteGraphite.
  ///
  /// In en, this message translates to:
  /// **'Graphite'**
  String get colorPaletteGraphite;

  /// No description provided for @commonCancel.
  ///
  /// In en, this message translates to:
  /// **'Cancel'**
  String get commonCancel;

  /// No description provided for @commonApply.
  ///
  /// In en, this message translates to:
  /// **'Apply'**
  String get commonApply;

  /// No description provided for @commonRetry.
  ///
  /// In en, this message translates to:
  /// **'Retry'**
  String get commonRetry;

  /// No description provided for @commonTryAgain.
  ///
  /// In en, this message translates to:
  /// **'Try again.'**
  String get commonTryAgain;

  /// No description provided for @commonStart.
  ///
  /// In en, this message translates to:
  /// **'Start'**
  String get commonStart;

  /// No description provided for @commonStarting.
  ///
  /// In en, this message translates to:
  /// **'Starting...'**
  String get commonStarting;

  /// No description provided for @commonRefresh.
  ///
  /// In en, this message translates to:
  /// **'Refresh'**
  String get commonRefresh;

  /// No description provided for @sessionStorageFailedTitle.
  ///
  /// In en, this message translates to:
  /// **'Session storage failed'**
  String get sessionStorageFailedTitle;

  /// No description provided for @sessionStorageFailedMessage.
  ///
  /// In en, this message translates to:
  /// **'Restart the app and try again.'**
  String get sessionStorageFailedMessage;

  /// No description provided for @preparingSession.
  ///
  /// In en, this message translates to:
  /// **'Preparing session...'**
  String get preparingSession;

  /// No description provided for @splashStageSession.
  ///
  /// In en, this message translates to:
  /// **'Checking session'**
  String get splashStageSession;

  /// No description provided for @splashStagePlayer.
  ///
  /// In en, this message translates to:
  /// **'Preparing player'**
  String get splashStagePlayer;

  /// No description provided for @splashStageCategories.
  ///
  /// In en, this message translates to:
  /// **'Loading word paths'**
  String get splashStageCategories;

  /// No description provided for @splashStageResources.
  ///
  /// In en, this message translates to:
  /// **'Preparing resources'**
  String get splashStageResources;

  /// No description provided for @splashStageReady.
  ///
  /// In en, this message translates to:
  /// **'Ready'**
  String get splashStageReady;

  /// No description provided for @splashLoadingSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Preparing your starting data.'**
  String get splashLoadingSubtitle;

  /// No description provided for @splashFailedTitle.
  ///
  /// In en, this message translates to:
  /// **'Startup could not finish'**
  String get splashFailedTitle;

  /// No description provided for @splashFailedMessage.
  ///
  /// In en, this message translates to:
  /// **'Check your connection and try again.'**
  String get splashFailedMessage;

  /// No description provided for @navProfile.
  ///
  /// In en, this message translates to:
  /// **'Profile'**
  String get navProfile;

  /// No description provided for @navQuests.
  ///
  /// In en, this message translates to:
  /// **'Quests'**
  String get navQuests;

  /// No description provided for @navMarket.
  ///
  /// In en, this message translates to:
  /// **'Market'**
  String get navMarket;

  /// No description provided for @navDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Diamonds'**
  String get navDiamonds;

  /// No description provided for @navEarnDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Earn Diamonds'**
  String get navEarnDiamonds;

  /// No description provided for @navSettings.
  ///
  /// In en, this message translates to:
  /// **'Settings'**
  String get navSettings;

  /// No description provided for @loadingCategories.
  ///
  /// In en, this message translates to:
  /// **'Loading categories...'**
  String get loadingCategories;

  /// No description provided for @couldNotLoadCategories.
  ///
  /// In en, this message translates to:
  /// **'Could not load categories'**
  String get couldNotLoadCategories;

  /// No description provided for @preparingCategories.
  ///
  /// In en, this message translates to:
  /// **'Preparing categories...'**
  String get preparingCategories;

  /// No description provided for @couldNotStartGame.
  ///
  /// In en, this message translates to:
  /// **'Could not start game'**
  String get couldNotStartGame;

  /// No description provided for @chooseCategory.
  ///
  /// In en, this message translates to:
  /// **'Choose category'**
  String get chooseCategory;

  /// No description provided for @chooseCategorySubtitle.
  ///
  /// In en, this message translates to:
  /// **'Pick a word field for your next link path.'**
  String get chooseCategorySubtitle;

  /// No description provided for @noCategoriesTitle.
  ///
  /// In en, this message translates to:
  /// **'No categories yet'**
  String get noCategoriesTitle;

  /// No description provided for @noCategoriesMessage.
  ///
  /// In en, this message translates to:
  /// **'Add category content before starting a game.'**
  String get noCategoriesMessage;

  /// No description provided for @startEasyGame.
  ///
  /// In en, this message translates to:
  /// **'Start easy game'**
  String get startEasyGame;

  /// No description provided for @preparingGame.
  ///
  /// In en, this message translates to:
  /// **'Preparing game...'**
  String get preparingGame;

  /// No description provided for @loadingGame.
  ///
  /// In en, this message translates to:
  /// **'Loading game...'**
  String get loadingGame;

  /// No description provided for @couldNotLoadGame.
  ///
  /// In en, this message translates to:
  /// **'Could not load game'**
  String get couldNotLoadGame;

  /// No description provided for @gameTitle.
  ///
  /// In en, this message translates to:
  /// **'Game'**
  String get gameTitle;

  /// No description provided for @gameTutorialTitle.
  ///
  /// In en, this message translates to:
  /// **'How to play'**
  String get gameTutorialTitle;

  /// No description provided for @gameTutorialHelpTooltip.
  ///
  /// In en, this message translates to:
  /// **'Open tutorial'**
  String get gameTutorialHelpTooltip;

  /// No description provided for @gameTutorialSkip.
  ///
  /// In en, this message translates to:
  /// **'Skip'**
  String get gameTutorialSkip;

  /// No description provided for @gameTutorialBack.
  ///
  /// In en, this message translates to:
  /// **'Back'**
  String get gameTutorialBack;

  /// No description provided for @gameTutorialNext.
  ///
  /// In en, this message translates to:
  /// **'Next'**
  String get gameTutorialNext;

  /// No description provided for @gameTutorialDone.
  ///
  /// In en, this message translates to:
  /// **'Start playing'**
  String get gameTutorialDone;

  /// No description provided for @gameTutorialGoalTitle.
  ///
  /// In en, this message translates to:
  /// **'Reach the target word'**
  String get gameTutorialGoalTitle;

  /// No description provided for @gameTutorialGoalBody.
  ///
  /// In en, this message translates to:
  /// **'Each game gives you a start word and a target word. Move through connected words until you reach the target.'**
  String get gameTutorialGoalBody;

  /// No description provided for @gameTutorialMoveTitle.
  ///
  /// In en, this message translates to:
  /// **'Choose the next word'**
  String get gameTutorialMoveTitle;

  /// No description provided for @gameTutorialMoveBody.
  ///
  /// In en, this message translates to:
  /// **'The options in the middle are links from your current word. Tap one to move your path to that word.'**
  String get gameTutorialMoveBody;

  /// No description provided for @gameTutorialStepsTitle.
  ///
  /// In en, this message translates to:
  /// **'Watch your steps'**
  String get gameTutorialStepsTitle;

  /// No description provided for @gameTutorialStepsBody.
  ///
  /// In en, this message translates to:
  /// **'The counter at the top shows how many steps you can use. Reach the target before your steps run out to complete the game.'**
  String get gameTutorialStepsBody;

  /// No description provided for @gameTutorialPowerTitle.
  ///
  /// In en, this message translates to:
  /// **'Use help when stuck'**
  String get gameTutorialPowerTitle;

  /// No description provided for @gameTutorialPowerBody.
  ///
  /// In en, this message translates to:
  /// **'Hint points you toward the right path. Reset returns to the start; tapping the previous word uses an undo.'**
  String get gameTutorialPowerBody;

  /// No description provided for @pickNextWord.
  ///
  /// In en, this message translates to:
  /// **'Pick the next word'**
  String get pickNextWord;

  /// No description provided for @noMovesTitle.
  ///
  /// In en, this message translates to:
  /// **'No moves available'**
  String get noMovesTitle;

  /// No description provided for @noMovesMessage.
  ///
  /// In en, this message translates to:
  /// **'This link has no outgoing choices.'**
  String get noMovesMessage;

  /// No description provided for @actionFailed.
  ///
  /// In en, this message translates to:
  /// **'Action failed'**
  String get actionFailed;

  /// No description provided for @backToHome.
  ///
  /// In en, this message translates to:
  /// **'Back to home'**
  String get backToHome;

  /// No description provided for @quitGameTitle.
  ///
  /// In en, this message translates to:
  /// **'Quit game?'**
  String get quitGameTitle;

  /// No description provided for @quitGameMessage.
  ///
  /// In en, this message translates to:
  /// **'This will abandon your current game and you will not earn any score.'**
  String get quitGameMessage;

  /// No description provided for @keepPlaying.
  ///
  /// In en, this message translates to:
  /// **'Keep playing'**
  String get keepPlaying;

  /// No description provided for @quit.
  ///
  /// In en, this message translates to:
  /// **'Quit'**
  String get quit;

  /// No description provided for @anchorTarget.
  ///
  /// In en, this message translates to:
  /// **'Target'**
  String get anchorTarget;

  /// No description provided for @currentLabel.
  ///
  /// In en, this message translates to:
  /// **'Current'**
  String get currentLabel;

  /// No description provided for @moreActions.
  ///
  /// In en, this message translates to:
  /// **'More actions'**
  String get moreActions;

  /// No description provided for @resultScore.
  ///
  /// In en, this message translates to:
  /// **'Score'**
  String get resultScore;

  /// No description provided for @resultSteps.
  ///
  /// In en, this message translates to:
  /// **'Steps'**
  String get resultSteps;

  /// No description provided for @resultHintsUsed.
  ///
  /// In en, this message translates to:
  /// **'Hints used'**
  String get resultHintsUsed;

  /// No description provided for @resultPath.
  ///
  /// In en, this message translates to:
  /// **'Path'**
  String get resultPath;

  /// No description provided for @hudSteps.
  ///
  /// In en, this message translates to:
  /// **'Steps {taken}/{max}'**
  String hudSteps(int taken, int max);

  /// No description provided for @hudHints.
  ///
  /// In en, this message translates to:
  /// **'Hints {count}'**
  String hudHints(int count);

  /// No description provided for @hudScore.
  ///
  /// In en, this message translates to:
  /// **'Score {score}'**
  String hudScore(int score);

  /// No description provided for @hintAction.
  ///
  /// In en, this message translates to:
  /// **'Hint ({balance})'**
  String hintAction(int balance);

  /// No description provided for @undoAction.
  ///
  /// In en, this message translates to:
  /// **'Undo ({balance})'**
  String undoAction(int balance);

  /// No description provided for @resetProgress.
  ///
  /// In en, this message translates to:
  /// **'Reset progress ({balance})'**
  String resetProgress(int balance);

  /// No description provided for @actionMakingStep.
  ///
  /// In en, this message translates to:
  /// **'Making step...'**
  String get actionMakingStep;

  /// No description provided for @actionFindingHint.
  ///
  /// In en, this message translates to:
  /// **'Finding hint...'**
  String get actionFindingHint;

  /// No description provided for @actionUndoing.
  ///
  /// In en, this message translates to:
  /// **'Undoing...'**
  String get actionUndoing;

  /// No description provided for @actionResetting.
  ///
  /// In en, this message translates to:
  /// **'Resetting...'**
  String get actionResetting;

  /// No description provided for @actionAbandoning.
  ///
  /// In en, this message translates to:
  /// **'Abandoning...'**
  String get actionAbandoning;

  /// No description provided for @actionWorking.
  ///
  /// In en, this message translates to:
  /// **'Working...'**
  String get actionWorking;

  /// No description provided for @outcomeCompletedTitle.
  ///
  /// In en, this message translates to:
  /// **'Completed'**
  String get outcomeCompletedTitle;

  /// No description provided for @outcomeCompletedSubtitle.
  ///
  /// In en, this message translates to:
  /// **'You reached {target}.'**
  String outcomeCompletedSubtitle(String target);

  /// No description provided for @outcomeFailedTitle.
  ///
  /// In en, this message translates to:
  /// **'No steps left'**
  String get outcomeFailedTitle;

  /// No description provided for @outcomeFailedSubtitle.
  ///
  /// In en, this message translates to:
  /// **'You ran out of steps before reaching {target}.'**
  String outcomeFailedSubtitle(String target);

  /// No description provided for @outcomeAbandonedTitle.
  ///
  /// In en, this message translates to:
  /// **'Abandoned'**
  String get outcomeAbandonedTitle;

  /// No description provided for @outcomeAbandonedSubtitle.
  ///
  /// In en, this message translates to:
  /// **'This game was abandoned.'**
  String get outcomeAbandonedSubtitle;

  /// No description provided for @outcomeEndedSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Game ended.'**
  String get outcomeEndedSubtitle;

  /// No description provided for @commonBuy.
  ///
  /// In en, this message translates to:
  /// **'Buy'**
  String get commonBuy;

  /// No description provided for @commonUnavailable.
  ///
  /// In en, this message translates to:
  /// **'Unavailable'**
  String get commonUnavailable;

  /// No description provided for @commonProcessing.
  ///
  /// In en, this message translates to:
  /// **'Processing...'**
  String get commonProcessing;

  /// No description provided for @commonCheckBackLater.
  ///
  /// In en, this message translates to:
  /// **'Check back later.'**
  String get commonCheckBackLater;

  /// No description provided for @commonUnlimited.
  ///
  /// In en, this message translates to:
  /// **'unlimited'**
  String get commonUnlimited;

  /// No description provided for @marketTitle.
  ///
  /// In en, this message translates to:
  /// **'Market'**
  String get marketTitle;

  /// No description provided for @openingMarket.
  ///
  /// In en, this message translates to:
  /// **'Opening market...'**
  String get openingMarket;

  /// No description provided for @fetchingOffers.
  ///
  /// In en, this message translates to:
  /// **'Fetching offers...'**
  String get fetchingOffers;

  /// No description provided for @marketUnavailable.
  ///
  /// In en, this message translates to:
  /// **'Market unavailable'**
  String get marketUnavailable;

  /// No description provided for @noOffersTitle.
  ///
  /// In en, this message translates to:
  /// **'No offers yet'**
  String get noOffersTitle;

  /// No description provided for @promoPrice.
  ///
  /// In en, this message translates to:
  /// **'Promo {price} ◆'**
  String promoPrice(Object price);

  /// No description provided for @price.
  ///
  /// In en, this message translates to:
  /// **'{price} ◆'**
  String price(Object price);

  /// No description provided for @stockLabel.
  ///
  /// In en, this message translates to:
  /// **'Stock: {stock}'**
  String stockLabel(Object stock);

  /// No description provided for @yourRemaining.
  ///
  /// In en, this message translates to:
  /// **'Your remaining: {remaining}'**
  String yourRemaining(Object remaining);

  /// No description provided for @buyConfirmTitle.
  ///
  /// In en, this message translates to:
  /// **'Buy {quantity} {type}?'**
  String buyConfirmTitle(Object quantity, Object type);

  /// No description provided for @buyConfirmMessage.
  ///
  /// In en, this message translates to:
  /// **'This will spend {price} diamonds.'**
  String buyConfirmMessage(Object price);

  /// No description provided for @diamondUnavailable.
  ///
  /// In en, this message translates to:
  /// **'Diamond unavailable'**
  String get diamondUnavailable;

  /// No description provided for @energyTitle.
  ///
  /// In en, this message translates to:
  /// **'Energy'**
  String get energyTitle;

  /// No description provided for @loadingDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Loading diamonds...'**
  String get loadingDiamonds;

  /// No description provided for @openingDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Opening diamonds...'**
  String get openingDiamonds;

  /// No description provided for @diamondsTitle.
  ///
  /// In en, this message translates to:
  /// **'Diamonds'**
  String get diamondsTitle;

  /// No description provided for @fetchingBundles.
  ///
  /// In en, this message translates to:
  /// **'Fetching diamond bundles...'**
  String get fetchingBundles;

  /// No description provided for @purchasesUnavailable.
  ///
  /// In en, this message translates to:
  /// **'Purchases unavailable'**
  String get purchasesUnavailable;

  /// No description provided for @purchasesUnavailableMessage.
  ///
  /// In en, this message translates to:
  /// **'Diamonds purchase is not available here.'**
  String get purchasesUnavailableMessage;

  /// No description provided for @couldNotLoadDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Could not load diamonds'**
  String get couldNotLoadDiamonds;

  /// No description provided for @noBundlesTitle.
  ///
  /// In en, this message translates to:
  /// **'No bundles yet'**
  String get noBundlesTitle;

  /// No description provided for @diamondBundleAmount.
  ///
  /// In en, this message translates to:
  /// **'{amount} diamonds'**
  String diamondBundleAmount(Object amount);

  /// No description provided for @diamondsAddedSnack.
  ///
  /// In en, this message translates to:
  /// **'+{amount} diamonds added.'**
  String diamondsAddedSnack(Object amount);

  /// No description provided for @openingRewards.
  ///
  /// In en, this message translates to:
  /// **'Opening rewards...'**
  String get openingRewards;

  /// No description provided for @rewardWatchedSnack.
  ///
  /// In en, this message translates to:
  /// **'Thanks for watching! Diamonds arrive once verified.'**
  String get rewardWatchedSnack;

  /// No description provided for @loadingRewards.
  ///
  /// In en, this message translates to:
  /// **'Loading rewards...'**
  String get loadingRewards;

  /// No description provided for @rewardedUnavailableTitle.
  ///
  /// In en, this message translates to:
  /// **'Rewarded ads unavailable'**
  String get rewardedUnavailableTitle;

  /// No description provided for @rewardedUnavailableMessage.
  ///
  /// In en, this message translates to:
  /// **'Open the mobile app to watch and earn.'**
  String get rewardedUnavailableMessage;

  /// No description provided for @couldNotLoadRewards.
  ///
  /// In en, this message translates to:
  /// **'Could not load rewards'**
  String get couldNotLoadRewards;

  /// No description provided for @rewardLoadingAd.
  ///
  /// In en, this message translates to:
  /// **'Loading ad...'**
  String get rewardLoadingAd;

  /// No description provided for @rewardDailyLimitReached.
  ///
  /// In en, this message translates to:
  /// **'Daily limit reached'**
  String get rewardDailyLimitReached;

  /// No description provided for @rewardWatchEarn.
  ///
  /// In en, this message translates to:
  /// **'Watch & earn {amount} 💎'**
  String rewardWatchEarn(Object amount);

  /// No description provided for @rewardCardTitle.
  ///
  /// In en, this message translates to:
  /// **'Watch a short ad, earn {amount} diamonds'**
  String rewardCardTitle(Object amount);

  /// No description provided for @rewardToday.
  ///
  /// In en, this message translates to:
  /// **'Today: {grants} / {limit} watched • {remaining} left'**
  String rewardToday(Object grants, Object limit, Object remaining);

  /// No description provided for @rewardFooter.
  ///
  /// In en, this message translates to:
  /// **'Diamonds are credited after the reward is verified by the ad network, so they may take a moment to appear.'**
  String get rewardFooter;

  /// No description provided for @commonUnknown.
  ///
  /// In en, this message translates to:
  /// **'unknown'**
  String get commonUnknown;

  /// No description provided for @questsSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Complete quests, earn bonus energy.'**
  String get questsSubtitle;

  /// No description provided for @questsLoading.
  ///
  /// In en, this message translates to:
  /// **'Loading quests...'**
  String get questsLoading;

  /// No description provided for @questsLoadError.
  ///
  /// In en, this message translates to:
  /// **'Could not load quests'**
  String get questsLoadError;

  /// No description provided for @noQuestsTitle.
  ///
  /// In en, this message translates to:
  /// **'No quests yet'**
  String get noQuestsTitle;

  /// No description provided for @noQuestsMessage.
  ///
  /// In en, this message translates to:
  /// **'Complete a game and quests will start appearing here.'**
  String get noQuestsMessage;

  /// No description provided for @questClaiming.
  ///
  /// In en, this message translates to:
  /// **'Claiming...'**
  String get questClaiming;

  /// No description provided for @questClaimReward.
  ///
  /// In en, this message translates to:
  /// **'Claim reward'**
  String get questClaimReward;

  /// No description provided for @questStateReady.
  ///
  /// In en, this message translates to:
  /// **'Ready'**
  String get questStateReady;

  /// No description provided for @questStateActive.
  ///
  /// In en, this message translates to:
  /// **'Active'**
  String get questStateActive;

  /// No description provided for @questStateClaimed.
  ///
  /// In en, this message translates to:
  /// **'Claimed'**
  String get questStateClaimed;

  /// No description provided for @loadingProfile.
  ///
  /// In en, this message translates to:
  /// **'Loading profile...'**
  String get loadingProfile;

  /// No description provided for @couldNotLoadProfile.
  ///
  /// In en, this message translates to:
  /// **'Could not load profile'**
  String get couldNotLoadProfile;

  /// No description provided for @noProfileTitle.
  ///
  /// In en, this message translates to:
  /// **'No profile yet'**
  String get noProfileTitle;

  /// No description provided for @noProfileMessage.
  ///
  /// In en, this message translates to:
  /// **'Start a guest session to see your profile.'**
  String get noProfileMessage;

  /// No description provided for @viewLeaderboard.
  ///
  /// In en, this message translates to:
  /// **'View leaderboard'**
  String get viewLeaderboard;

  /// No description provided for @guestPlayer.
  ///
  /// In en, this message translates to:
  /// **'Guest player'**
  String get guestPlayer;

  /// No description provided for @guestSession.
  ///
  /// In en, this message translates to:
  /// **'Guest session'**
  String get guestSession;

  /// No description provided for @providersLinked.
  ///
  /// In en, this message translates to:
  /// **'{count, plural, =1{1 provider linked} other{{count} providers linked}}'**
  String providersLinked(int count);

  /// No description provided for @linkAccount.
  ///
  /// In en, this message translates to:
  /// **'Link account'**
  String get linkAccount;

  /// No description provided for @linkGoogle.
  ///
  /// In en, this message translates to:
  /// **'Link Google'**
  String get linkGoogle;

  /// No description provided for @linkApple.
  ///
  /// In en, this message translates to:
  /// **'Link Apple'**
  String get linkApple;

  /// No description provided for @linkingAccount.
  ///
  /// In en, this message translates to:
  /// **'Linking...'**
  String get linkingAccount;

  /// No description provided for @accountLinked.
  ///
  /// In en, this message translates to:
  /// **'Account linked.'**
  String get accountLinked;

  /// No description provided for @statGamesCompleted.
  ///
  /// In en, this message translates to:
  /// **'Games completed'**
  String get statGamesCompleted;

  /// No description provided for @statBestScore.
  ///
  /// In en, this message translates to:
  /// **'Best score'**
  String get statBestScore;

  /// No description provided for @statTotalScore.
  ///
  /// In en, this message translates to:
  /// **'Total score'**
  String get statTotalScore;

  /// No description provided for @statLastCompleted.
  ///
  /// In en, this message translates to:
  /// **'Last completed'**
  String get statLastCompleted;

  /// No description provided for @leaderboardTitle.
  ///
  /// In en, this message translates to:
  /// **'Leaderboard'**
  String get leaderboardTitle;

  /// No description provided for @loadingLeaderboard.
  ///
  /// In en, this message translates to:
  /// **'Loading leaderboard...'**
  String get loadingLeaderboard;

  /// No description provided for @couldNotLoadLeaderboard.
  ///
  /// In en, this message translates to:
  /// **'Could not load leaderboard'**
  String get couldNotLoadLeaderboard;

  /// No description provided for @noScoresTitle.
  ///
  /// In en, this message translates to:
  /// **'No scores yet'**
  String get noScoresTitle;

  /// No description provided for @leaderboardAllTime.
  ///
  /// In en, this message translates to:
  /// **'All-time'**
  String get leaderboardAllTime;

  /// No description provided for @leaderboardDaily.
  ///
  /// In en, this message translates to:
  /// **'Daily'**
  String get leaderboardDaily;

  /// No description provided for @leaderboardWeekly.
  ///
  /// In en, this message translates to:
  /// **'Weekly'**
  String get leaderboardWeekly;

  /// No description provided for @leaderboardAllTimeDesc.
  ///
  /// In en, this message translates to:
  /// **'All-time total score across players.'**
  String get leaderboardAllTimeDesc;

  /// No description provided for @leaderboardDailyDesc.
  ///
  /// In en, this message translates to:
  /// **'Total score today (UTC).'**
  String get leaderboardDailyDesc;

  /// No description provided for @leaderboardWeeklyDesc.
  ///
  /// In en, this message translates to:
  /// **'Total score this week (UTC, Monday start).'**
  String get leaderboardWeeklyDesc;

  /// No description provided for @leaderboardAllTimeEmpty.
  ///
  /// In en, this message translates to:
  /// **'Complete a game to appear on the leaderboard.'**
  String get leaderboardAllTimeEmpty;

  /// No description provided for @leaderboardDailyEmpty.
  ///
  /// In en, this message translates to:
  /// **'No scores recorded today yet.'**
  String get leaderboardDailyEmpty;

  /// No description provided for @leaderboardWeeklyEmpty.
  ///
  /// In en, this message translates to:
  /// **'No scores recorded this week yet.'**
  String get leaderboardWeeklyEmpty;

  /// No description provided for @settingsSoundSection.
  ///
  /// In en, this message translates to:
  /// **'Sound'**
  String get settingsSoundSection;

  /// No description provided for @settingsMusic.
  ///
  /// In en, this message translates to:
  /// **'Music'**
  String get settingsMusic;

  /// No description provided for @settingsMusicSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Background music while you play'**
  String get settingsMusicSubtitle;

  /// No description provided for @settingsMusicVolume.
  ///
  /// In en, this message translates to:
  /// **'Music volume'**
  String get settingsMusicVolume;

  /// No description provided for @settingsSfx.
  ///
  /// In en, this message translates to:
  /// **'Sound effects'**
  String get settingsSfx;

  /// No description provided for @settingsSfxSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Taps, moves, wins and rewards'**
  String get settingsSfxSubtitle;

  /// No description provided for @settingsSfxVolume.
  ///
  /// In en, this message translates to:
  /// **'Sound effects volume'**
  String get settingsSfxVolume;

  /// No description provided for @commonSave.
  ///
  /// In en, this message translates to:
  /// **'Save'**
  String get commonSave;

  /// No description provided for @commonCreate.
  ///
  /// In en, this message translates to:
  /// **'Create'**
  String get commonCreate;

  /// No description provided for @commonClose.
  ///
  /// In en, this message translates to:
  /// **'Close'**
  String get commonClose;

  /// No description provided for @commonClear.
  ///
  /// In en, this message translates to:
  /// **'Clear'**
  String get commonClear;

  /// No description provided for @commonEdit.
  ///
  /// In en, this message translates to:
  /// **'Edit'**
  String get commonEdit;

  /// No description provided for @commonDeactivate.
  ///
  /// In en, this message translates to:
  /// **'Deactivate'**
  String get commonDeactivate;

  /// No description provided for @commonReset.
  ///
  /// In en, this message translates to:
  /// **'Reset'**
  String get commonReset;

  /// No description provided for @commonLoad.
  ///
  /// In en, this message translates to:
  /// **'Load'**
  String get commonLoad;

  /// No description provided for @commonRequired.
  ///
  /// In en, this message translates to:
  /// **'Required'**
  String get commonRequired;

  /// No description provided for @commonMustBeNumber.
  ///
  /// In en, this message translates to:
  /// **'Must be a number'**
  String get commonMustBeNumber;

  /// No description provided for @commonEnterNumber.
  ///
  /// In en, this message translates to:
  /// **'Enter a number'**
  String get commonEnterNumber;

  /// No description provided for @commonGreaterThanZero.
  ///
  /// In en, this message translates to:
  /// **'Must be greater than 0'**
  String get commonGreaterThanZero;

  /// No description provided for @commonNonNegative.
  ///
  /// In en, this message translates to:
  /// **'Must be >= 0'**
  String get commonNonNegative;

  /// No description provided for @commonNoDash.
  ///
  /// In en, this message translates to:
  /// **'—'**
  String get commonNoDash;

  /// No description provided for @adminLabel.
  ///
  /// In en, this message translates to:
  /// **'Admin'**
  String get adminLabel;

  /// No description provided for @adminConsole.
  ///
  /// In en, this message translates to:
  /// **'Admin console'**
  String get adminConsole;

  /// No description provided for @adminMobileTitle.
  ///
  /// In en, this message translates to:
  /// **'Admin · {title}'**
  String adminMobileTitle(Object title);

  /// No description provided for @adminSignOut.
  ///
  /// In en, this message translates to:
  /// **'Sign out'**
  String get adminSignOut;

  /// No description provided for @adminNavQuests.
  ///
  /// In en, this message translates to:
  /// **'Quests'**
  String get adminNavQuests;

  /// No description provided for @adminNavPlayers.
  ///
  /// In en, this message translates to:
  /// **'Players'**
  String get adminNavPlayers;

  /// No description provided for @adminNavEnergy.
  ///
  /// In en, this message translates to:
  /// **'Energy'**
  String get adminNavEnergy;

  /// No description provided for @adminNavHint.
  ///
  /// In en, this message translates to:
  /// **'Hint'**
  String get adminNavHint;

  /// No description provided for @adminNavUndo.
  ///
  /// In en, this message translates to:
  /// **'Undo'**
  String get adminNavUndo;

  /// No description provided for @adminNavReset.
  ///
  /// In en, this message translates to:
  /// **'Reset'**
  String get adminNavReset;

  /// No description provided for @adminNavDiamond.
  ///
  /// In en, this message translates to:
  /// **'Diamond'**
  String get adminNavDiamond;

  /// No description provided for @adminNavMarket.
  ///
  /// In en, this message translates to:
  /// **'Market'**
  String get adminNavMarket;

  /// No description provided for @adminNavContent.
  ///
  /// In en, this message translates to:
  /// **'Content'**
  String get adminNavContent;

  /// No description provided for @adminNavAudit.
  ///
  /// In en, this message translates to:
  /// **'Audit'**
  String get adminNavAudit;

  /// No description provided for @adminSignInTitle.
  ///
  /// In en, this message translates to:
  /// **'Admin sign-in'**
  String get adminSignInTitle;

  /// No description provided for @adminSignInHelp.
  ///
  /// In en, this message translates to:
  /// **'Development verifier: enter your admin email and the literal \"dev:admin:<email>\" token. Production SSO arrives later.'**
  String get adminSignInHelp;

  /// No description provided for @adminEmailLabel.
  ///
  /// In en, this message translates to:
  /// **'Email'**
  String get adminEmailLabel;

  /// No description provided for @adminExternalTokenLabel.
  ///
  /// In en, this message translates to:
  /// **'External token'**
  String get adminExternalTokenLabel;

  /// No description provided for @adminSigningIn.
  ///
  /// In en, this message translates to:
  /// **'Signing in...'**
  String get adminSigningIn;

  /// No description provided for @adminSignIn.
  ///
  /// In en, this message translates to:
  /// **'Sign in'**
  String get adminSignIn;

  /// No description provided for @adminSignInFailed.
  ///
  /// In en, this message translates to:
  /// **'Sign-in failed.'**
  String get adminSignInFailed;

  /// No description provided for @adminLookUp.
  ///
  /// In en, this message translates to:
  /// **'Look up'**
  String get adminLookUp;

  /// No description provided for @adminPlayerGuid.
  ///
  /// In en, this message translates to:
  /// **'Player GUID'**
  String get adminPlayerGuid;

  /// No description provided for @adminPlayerHandle.
  ///
  /// In en, this message translates to:
  /// **'Player handle'**
  String get adminPlayerHandle;

  /// No description provided for @adminPlayerId.
  ///
  /// In en, this message translates to:
  /// **'Player id'**
  String get adminPlayerId;

  /// No description provided for @adminLookupFailed.
  ///
  /// In en, this message translates to:
  /// **'Lookup failed.'**
  String get adminLookupFailed;

  /// No description provided for @adminNoPlayerFound.
  ///
  /// In en, this message translates to:
  /// **'No player found.'**
  String get adminNoPlayerFound;

  /// No description provided for @adminPlayerConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Player console'**
  String get adminPlayerConsoleTitle;

  /// No description provided for @adminPlayerConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Look up players by handle in DisplayName#1234 format.'**
  String get adminPlayerConsoleHelp;

  /// No description provided for @adminId.
  ///
  /// In en, this message translates to:
  /// **'Id'**
  String get adminId;

  /// No description provided for @adminLocale.
  ///
  /// In en, this message translates to:
  /// **'Locale'**
  String get adminLocale;

  /// No description provided for @adminAuthProvidersLinked.
  ///
  /// In en, this message translates to:
  /// **'Auth providers linked'**
  String get adminAuthProvidersLinked;

  /// No description provided for @adminCreated.
  ///
  /// In en, this message translates to:
  /// **'Created'**
  String get adminCreated;

  /// No description provided for @adminBannedAt.
  ///
  /// In en, this message translates to:
  /// **'Banned at'**
  String get adminBannedAt;

  /// No description provided for @adminReason.
  ///
  /// In en, this message translates to:
  /// **'Reason'**
  String get adminReason;

  /// No description provided for @adminBan.
  ///
  /// In en, this message translates to:
  /// **'Ban'**
  String get adminBan;

  /// No description provided for @adminUnban.
  ///
  /// In en, this message translates to:
  /// **'Unban'**
  String get adminUnban;

  /// No description provided for @adminBanned.
  ///
  /// In en, this message translates to:
  /// **'Banned'**
  String get adminBanned;

  /// No description provided for @adminGuest.
  ///
  /// In en, this message translates to:
  /// **'Guest'**
  String get adminGuest;

  /// No description provided for @adminBanPlayerTitle.
  ///
  /// In en, this message translates to:
  /// **'Ban player'**
  String get adminBanPlayerTitle;

  /// No description provided for @adminUnbanPlayerTitle.
  ///
  /// In en, this message translates to:
  /// **'Unban player?'**
  String get adminUnbanPlayerTitle;

  /// No description provided for @adminUnbanPlayerMessage.
  ///
  /// In en, this message translates to:
  /// **'{handle} will be able to sign in again. The ban reason history is preserved on the audit log.'**
  String adminUnbanPlayerMessage(Object handle);

  /// No description provided for @adminQuestsTitle.
  ///
  /// In en, this message translates to:
  /// **'Quest definitions'**
  String get adminQuestsTitle;

  /// No description provided for @adminNewQuest.
  ///
  /// In en, this message translates to:
  /// **'New quest'**
  String get adminNewQuest;

  /// No description provided for @adminQuestLoadError.
  ///
  /// In en, this message translates to:
  /// **'Quest definitions could not be loaded.'**
  String get adminQuestLoadError;

  /// No description provided for @adminNoQuestDefinitions.
  ///
  /// In en, this message translates to:
  /// **'No quest definitions yet. Start with \"New quest\".'**
  String get adminNoQuestDefinitions;

  /// No description provided for @adminQuestPrerequisite.
  ///
  /// In en, this message translates to:
  /// **'Prerequisite: {name}'**
  String adminQuestPrerequisite(Object name);

  /// No description provided for @adminQuestEditTooltip.
  ///
  /// In en, this message translates to:
  /// **'Edit'**
  String get adminQuestEditTooltip;

  /// No description provided for @adminQuestDeactivateTooltip.
  ///
  /// In en, this message translates to:
  /// **'Deactivate'**
  String get adminQuestDeactivateTooltip;

  /// No description provided for @adminQuestReactivateTooltip.
  ///
  /// In en, this message translates to:
  /// **'Reactivate'**
  String get adminQuestReactivateTooltip;

  /// No description provided for @adminInactive.
  ///
  /// In en, this message translates to:
  /// **'Inactive'**
  String get adminInactive;

  /// No description provided for @adminQuestDeactivateTitle.
  ///
  /// In en, this message translates to:
  /// **'Deactivate quest definition?'**
  String get adminQuestDeactivateTitle;

  /// No description provided for @adminQuestDeactivateMessage.
  ///
  /// In en, this message translates to:
  /// **'\"{name}\" will no longer be issued to players. Existing player progress is not affected.'**
  String adminQuestDeactivateMessage(Object name);

  /// No description provided for @adminQuestFormEditTitle.
  ///
  /// In en, this message translates to:
  /// **'Edit quest definition'**
  String get adminQuestFormEditTitle;

  /// No description provided for @adminQuestFormCreateTitle.
  ///
  /// In en, this message translates to:
  /// **'New quest definition'**
  String get adminQuestFormCreateTitle;

  /// No description provided for @adminName.
  ///
  /// In en, this message translates to:
  /// **'Name'**
  String get adminName;

  /// No description provided for @adminImmutableAfterCreate.
  ///
  /// In en, this message translates to:
  /// **'Cannot be changed after creation.'**
  String get adminImmutableAfterCreate;

  /// No description provided for @adminNameRequired.
  ///
  /// In en, this message translates to:
  /// **'Name is required'**
  String get adminNameRequired;

  /// No description provided for @adminMax64.
  ///
  /// In en, this message translates to:
  /// **'Maximum 64 characters'**
  String get adminMax64;

  /// No description provided for @adminDescription.
  ///
  /// In en, this message translates to:
  /// **'Description'**
  String get adminDescription;

  /// No description provided for @adminMax256.
  ///
  /// In en, this message translates to:
  /// **'Maximum 256 characters'**
  String get adminMax256;

  /// No description provided for @adminTrigger.
  ///
  /// In en, this message translates to:
  /// **'Trigger'**
  String get adminTrigger;

  /// No description provided for @adminTriggerRequired.
  ///
  /// In en, this message translates to:
  /// **'Trigger is required'**
  String get adminTriggerRequired;

  /// No description provided for @adminThreshold.
  ///
  /// In en, this message translates to:
  /// **'Threshold'**
  String get adminThreshold;

  /// No description provided for @adminEnergyReward.
  ///
  /// In en, this message translates to:
  /// **'Energy reward'**
  String get adminEnergyReward;

  /// No description provided for @adminHintReward.
  ///
  /// In en, this message translates to:
  /// **'Hint reward'**
  String get adminHintReward;

  /// No description provided for @adminUndoReward.
  ///
  /// In en, this message translates to:
  /// **'Undo reward'**
  String get adminUndoReward;

  /// No description provided for @adminResetReward.
  ///
  /// In en, this message translates to:
  /// **'Reset reward'**
  String get adminResetReward;

  /// No description provided for @adminDiamondReward.
  ///
  /// In en, this message translates to:
  /// **'Diamond reward'**
  String get adminDiamondReward;

  /// No description provided for @adminProgressBaseline.
  ///
  /// In en, this message translates to:
  /// **'Progress baseline'**
  String get adminProgressBaseline;

  /// No description provided for @adminProgressBaselineHelp.
  ///
  /// In en, this message translates to:
  /// **'Only meaningful for \"Total games\".'**
  String get adminProgressBaselineHelp;

  /// No description provided for @adminPrerequisiteOptional.
  ///
  /// In en, this message translates to:
  /// **'Prerequisite (optional)'**
  String get adminPrerequisiteOptional;

  /// No description provided for @adminNoPrerequisite.
  ///
  /// In en, this message translates to:
  /// **'— none —'**
  String get adminNoPrerequisite;

  /// No description provided for @adminRewardPositiveRequired.
  ///
  /// In en, this message translates to:
  /// **'At least one reward (energy, hint, undo, reset, diamond) must be greater than 0.'**
  String get adminRewardPositiveRequired;

  /// No description provided for @adminPositiveRequired.
  ///
  /// In en, this message translates to:
  /// **'Must be greater than 0'**
  String get adminPositiveRequired;

  /// No description provided for @adminNotNegative.
  ///
  /// In en, this message translates to:
  /// **'Cannot be less than 0'**
  String get adminNotNegative;

  /// No description provided for @adminEnergyConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Energy console'**
  String get adminEnergyConsoleTitle;

  /// No description provided for @adminEnergyConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Lookup by player GUID, then snap / grant / reset. Grant is capped at the player\'s maximum energy.'**
  String get adminEnergyConsoleHelp;

  /// No description provided for @adminNoEnergyAggregate.
  ///
  /// In en, this message translates to:
  /// **'No energy aggregate.'**
  String get adminNoEnergyAggregate;

  /// No description provided for @adminOverMax.
  ///
  /// In en, this message translates to:
  /// **'Over max'**
  String get adminOverMax;

  /// No description provided for @adminFull.
  ///
  /// In en, this message translates to:
  /// **'Full'**
  String get adminFull;

  /// No description provided for @adminRechargeInterval.
  ///
  /// In en, this message translates to:
  /// **'Recharge interval'**
  String get adminRechargeInterval;

  /// No description provided for @adminLastRefilled.
  ///
  /// In en, this message translates to:
  /// **'Last refilled'**
  String get adminLastRefilled;

  /// No description provided for @adminNextRefillIn.
  ///
  /// In en, this message translates to:
  /// **'Next refill in'**
  String get adminNextRefillIn;

  /// No description provided for @adminFullyRefilledAt.
  ///
  /// In en, this message translates to:
  /// **'Fully refilled at'**
  String get adminFullyRefilledAt;

  /// No description provided for @adminSetAmount.
  ///
  /// In en, this message translates to:
  /// **'Set amount'**
  String get adminSetAmount;

  /// No description provided for @adminGrantBonus.
  ///
  /// In en, this message translates to:
  /// **'Grant bonus'**
  String get adminGrantBonus;

  /// No description provided for @adminResetToFull.
  ///
  /// In en, this message translates to:
  /// **'Reset to full'**
  String get adminResetToFull;

  /// No description provided for @adminSetEnergyAmountTitle.
  ///
  /// In en, this message translates to:
  /// **'Set energy amount'**
  String get adminSetEnergyAmountTitle;

  /// No description provided for @adminNewCurrentAmount.
  ///
  /// In en, this message translates to:
  /// **'New current amount'**
  String get adminNewCurrentAmount;

  /// No description provided for @adminSetEnergyHelper.
  ///
  /// In en, this message translates to:
  /// **'Snaps the player\'s current energy to this value (>= 0).'**
  String get adminSetEnergyHelper;

  /// No description provided for @adminGrantBonusEnergyTitle.
  ///
  /// In en, this message translates to:
  /// **'Grant bonus energy'**
  String get adminGrantBonusEnergyTitle;

  /// No description provided for @adminBonusAmount.
  ///
  /// In en, this message translates to:
  /// **'Bonus amount'**
  String get adminBonusAmount;

  /// No description provided for @adminGrantEnergyHelper.
  ///
  /// In en, this message translates to:
  /// **'Adds to current energy, capped at the player\'s maximum.'**
  String get adminGrantEnergyHelper;

  /// No description provided for @adminResetEnergyTitle.
  ///
  /// In en, this message translates to:
  /// **'Reset energy?'**
  String get adminResetEnergyTitle;

  /// No description provided for @adminResetEnergyMessage.
  ///
  /// In en, this message translates to:
  /// **'Resets the player to maximum energy.'**
  String get adminResetEnergyMessage;

  /// No description provided for @adminHintConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Hint console'**
  String get adminHintConsoleTitle;

  /// No description provided for @adminHintConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Lookup by player GUID, then snap / grant / reset. Hint inventory has no max cap.'**
  String get adminHintConsoleHelp;

  /// No description provided for @adminNoHintInventory.
  ///
  /// In en, this message translates to:
  /// **'No hint inventory.'**
  String get adminNoHintInventory;

  /// No description provided for @adminSetBalance.
  ///
  /// In en, this message translates to:
  /// **'Set balance'**
  String get adminSetBalance;

  /// No description provided for @adminGrantHints.
  ///
  /// In en, this message translates to:
  /// **'Grant hints'**
  String get adminGrantHints;

  /// No description provided for @adminResetToZero.
  ///
  /// In en, this message translates to:
  /// **'Reset to zero'**
  String get adminResetToZero;

  /// No description provided for @adminSetHintBalanceTitle.
  ///
  /// In en, this message translates to:
  /// **'Set hint balance'**
  String get adminSetHintBalanceTitle;

  /// No description provided for @adminNewBalance.
  ///
  /// In en, this message translates to:
  /// **'New balance'**
  String get adminNewBalance;

  /// No description provided for @adminSetHintHelper.
  ///
  /// In en, this message translates to:
  /// **'Snaps the player\'s hint balance to this value (>= 0).'**
  String get adminSetHintHelper;

  /// No description provided for @adminHintAmount.
  ///
  /// In en, this message translates to:
  /// **'Hint amount'**
  String get adminHintAmount;

  /// No description provided for @adminGrantHintHelper.
  ///
  /// In en, this message translates to:
  /// **'Adds to the existing balance - no max cap.'**
  String get adminGrantHintHelper;

  /// No description provided for @adminResetHintTitle.
  ///
  /// In en, this message translates to:
  /// **'Reset hint balance?'**
  String get adminResetHintTitle;

  /// No description provided for @adminResetHintMessage.
  ///
  /// In en, this message translates to:
  /// **'Sets the player\'s hint balance to zero.'**
  String get adminResetHintMessage;

  /// No description provided for @adminUndoConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Undo console'**
  String get adminUndoConsoleTitle;

  /// No description provided for @adminUndoConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Lookup by player GUID, then snap / grant / reset. Undo inventory has no max cap.'**
  String get adminUndoConsoleHelp;

  /// No description provided for @adminNoUndoInventory.
  ///
  /// In en, this message translates to:
  /// **'No undo inventory.'**
  String get adminNoUndoInventory;

  /// No description provided for @adminGrantUndos.
  ///
  /// In en, this message translates to:
  /// **'Grant undos'**
  String get adminGrantUndos;

  /// No description provided for @adminSetUndoBalanceTitle.
  ///
  /// In en, this message translates to:
  /// **'Set undo balance'**
  String get adminSetUndoBalanceTitle;

  /// No description provided for @adminSetUndoHelper.
  ///
  /// In en, this message translates to:
  /// **'Snaps the player\'s undo balance to this value (>= 0).'**
  String get adminSetUndoHelper;

  /// No description provided for @adminUndoAmount.
  ///
  /// In en, this message translates to:
  /// **'Undo amount'**
  String get adminUndoAmount;

  /// No description provided for @adminGrantUndoHelper.
  ///
  /// In en, this message translates to:
  /// **'Adds to the existing balance - no max cap.'**
  String get adminGrantUndoHelper;

  /// No description provided for @adminResetUndoTitle.
  ///
  /// In en, this message translates to:
  /// **'Reset undo balance?'**
  String get adminResetUndoTitle;

  /// No description provided for @adminResetUndoMessage.
  ///
  /// In en, this message translates to:
  /// **'Sets the player\'s undo balance to zero.'**
  String get adminResetUndoMessage;

  /// No description provided for @adminResetConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Reset console'**
  String get adminResetConsoleTitle;

  /// No description provided for @adminResetConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Lookup by player GUID, then snap / grant / reset. Reset inventory has no max cap.'**
  String get adminResetConsoleHelp;

  /// No description provided for @adminNoResetInventory.
  ///
  /// In en, this message translates to:
  /// **'No reset inventory.'**
  String get adminNoResetInventory;

  /// No description provided for @adminGrantResets.
  ///
  /// In en, this message translates to:
  /// **'Grant resets'**
  String get adminGrantResets;

  /// No description provided for @adminSetResetBalanceTitle.
  ///
  /// In en, this message translates to:
  /// **'Set reset balance'**
  String get adminSetResetBalanceTitle;

  /// No description provided for @adminSetResetHelper.
  ///
  /// In en, this message translates to:
  /// **'Snaps the player\'s reset balance to this value (>= 0).'**
  String get adminSetResetHelper;

  /// No description provided for @adminResetAmount.
  ///
  /// In en, this message translates to:
  /// **'Reset amount'**
  String get adminResetAmount;

  /// No description provided for @adminGrantResetHelper.
  ///
  /// In en, this message translates to:
  /// **'Adds to the existing balance - no max cap.'**
  String get adminGrantResetHelper;

  /// No description provided for @adminResetResetTitle.
  ///
  /// In en, this message translates to:
  /// **'Reset reset balance?'**
  String get adminResetResetTitle;

  /// No description provided for @adminResetResetMessage.
  ///
  /// In en, this message translates to:
  /// **'Sets the player\'s reset balance to zero.'**
  String get adminResetResetMessage;

  /// No description provided for @adminDiamondConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Diamond console'**
  String get adminDiamondConsoleTitle;

  /// No description provided for @adminDiamondConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Lookup by player GUID, then set / grant / reset. Diamond is uncapped currency.'**
  String get adminDiamondConsoleHelp;

  /// No description provided for @adminNoDiamondInventory.
  ///
  /// In en, this message translates to:
  /// **'No diamond inventory.'**
  String get adminNoDiamondInventory;

  /// No description provided for @adminGrantDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Grant diamonds'**
  String get adminGrantDiamonds;

  /// No description provided for @adminSetDiamondBalanceTitle.
  ///
  /// In en, this message translates to:
  /// **'Set diamond balance'**
  String get adminSetDiamondBalanceTitle;

  /// No description provided for @adminBalance.
  ///
  /// In en, this message translates to:
  /// **'Balance'**
  String get adminBalance;

  /// No description provided for @adminAmount.
  ///
  /// In en, this message translates to:
  /// **'Amount'**
  String get adminAmount;

  /// No description provided for @adminResetDiamondTitle.
  ///
  /// In en, this message translates to:
  /// **'Reset diamond balance?'**
  String get adminResetDiamondTitle;

  /// No description provided for @adminResetDiamondMessage.
  ///
  /// In en, this message translates to:
  /// **'This sets the Diamond balance to zero.'**
  String get adminResetDiamondMessage;

  /// No description provided for @adminMarketConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Market console'**
  String get adminMarketConsoleTitle;

  /// No description provided for @adminMarketConsoleHelp.
  ///
  /// In en, this message translates to:
  /// **'Manage shop categories, diamond-priced items, and player purchase history.'**
  String get adminMarketConsoleHelp;

  /// No description provided for @adminMarketCategories.
  ///
  /// In en, this message translates to:
  /// **'Categories'**
  String get adminMarketCategories;

  /// No description provided for @adminMarketItems.
  ///
  /// In en, this message translates to:
  /// **'Items'**
  String get adminMarketItems;

  /// No description provided for @adminMarketOrders.
  ///
  /// In en, this message translates to:
  /// **'Orders'**
  String get adminMarketOrders;

  /// No description provided for @adminNewCategory.
  ///
  /// In en, this message translates to:
  /// **'New category'**
  String get adminNewCategory;

  /// No description provided for @adminEditCategory.
  ///
  /// In en, this message translates to:
  /// **'Edit category'**
  String get adminEditCategory;

  /// No description provided for @adminSortStatus.
  ///
  /// In en, this message translates to:
  /// **'Sort {sortOrder} - {status}'**
  String adminSortStatus(Object sortOrder, Object status);

  /// No description provided for @adminActive.
  ///
  /// In en, this message translates to:
  /// **'Active'**
  String get adminActive;

  /// No description provided for @adminNoMarketCategories.
  ///
  /// In en, this message translates to:
  /// **'No market categories yet.'**
  String get adminNoMarketCategories;

  /// No description provided for @adminNewItem.
  ///
  /// In en, this message translates to:
  /// **'New item'**
  String get adminNewItem;

  /// No description provided for @adminEditItem.
  ///
  /// In en, this message translates to:
  /// **'Edit item'**
  String get adminEditItem;

  /// No description provided for @adminMarketItemSubtitle.
  ///
  /// In en, this message translates to:
  /// **'{category} - {price} diamonds - stock {stock}'**
  String adminMarketItemSubtitle(Object category, Object price, Object stock);

  /// No description provided for @adminStock.
  ///
  /// In en, this message translates to:
  /// **'stock'**
  String get adminStock;

  /// No description provided for @adminNoMarketItems.
  ///
  /// In en, this message translates to:
  /// **'No market items yet.'**
  String get adminNoMarketItems;

  /// No description provided for @adminNoMarketOrders.
  ///
  /// In en, this message translates to:
  /// **'No market orders for this player.'**
  String get adminNoMarketOrders;

  /// No description provided for @adminSortOrder.
  ///
  /// In en, this message translates to:
  /// **'Sort order'**
  String get adminSortOrder;

  /// No description provided for @adminIcon.
  ///
  /// In en, this message translates to:
  /// **'Icon'**
  String get adminIcon;

  /// No description provided for @adminVisibilityStarts.
  ///
  /// In en, this message translates to:
  /// **'Visibility starts at (ISO, optional)'**
  String get adminVisibilityStarts;

  /// No description provided for @adminVisibilityEnds.
  ///
  /// In en, this message translates to:
  /// **'Visibility ends at (ISO, optional)'**
  String get adminVisibilityEnds;

  /// No description provided for @adminNormal.
  ///
  /// In en, this message translates to:
  /// **'Normal'**
  String get adminNormal;

  /// No description provided for @adminPromotion.
  ///
  /// In en, this message translates to:
  /// **'Promotion'**
  String get adminPromotion;

  /// No description provided for @adminCategory.
  ///
  /// In en, this message translates to:
  /// **'Category'**
  String get adminCategory;

  /// No description provided for @adminItemType.
  ///
  /// In en, this message translates to:
  /// **'Item type'**
  String get adminItemType;

  /// No description provided for @adminQuantity.
  ///
  /// In en, this message translates to:
  /// **'Quantity'**
  String get adminQuantity;

  /// No description provided for @adminPriceDiamonds.
  ///
  /// In en, this message translates to:
  /// **'Price diamonds'**
  String get adminPriceDiamonds;

  /// No description provided for @adminPromoPrice.
  ///
  /// In en, this message translates to:
  /// **'Promo price'**
  String get adminPromoPrice;

  /// No description provided for @adminPromotionStarts.
  ///
  /// In en, this message translates to:
  /// **'Promotion starts'**
  String get adminPromotionStarts;

  /// No description provided for @adminPromotionEnds.
  ///
  /// In en, this message translates to:
  /// **'Promotion ends'**
  String get adminPromotionEnds;

  /// No description provided for @adminMaxStock.
  ///
  /// In en, this message translates to:
  /// **'Max stock'**
  String get adminMaxStock;

  /// No description provided for @adminPerPlayerLimit.
  ///
  /// In en, this message translates to:
  /// **'Per-player limit'**
  String get adminPerPlayerLimit;

  /// No description provided for @adminLimitWindow.
  ///
  /// In en, this message translates to:
  /// **'Limit window'**
  String get adminLimitWindow;

  /// No description provided for @adminMustBeLowerThanPrice.
  ///
  /// In en, this message translates to:
  /// **'Must be lower than price'**
  String get adminMustBeLowerThanPrice;

  /// No description provided for @adminMustBeAfterStart.
  ///
  /// In en, this message translates to:
  /// **'Must be after start'**
  String get adminMustBeAfterStart;

  /// No description provided for @adminContentConsoleTitle.
  ///
  /// In en, this message translates to:
  /// **'Content console'**
  String get adminContentConsoleTitle;

  /// No description provided for @adminContentLanguageFilter.
  ///
  /// In en, this message translates to:
  /// **'Language filter'**
  String get adminContentLanguageFilter;

  /// No description provided for @adminContentAllLanguages.
  ///
  /// In en, this message translates to:
  /// **'All languages'**
  String get adminContentAllLanguages;

  /// No description provided for @adminContentNewCategory.
  ///
  /// In en, this message translates to:
  /// **'New content category'**
  String get adminContentNewCategory;

  /// No description provided for @adminContentEditCategory.
  ///
  /// In en, this message translates to:
  /// **'Edit content category'**
  String get adminContentEditCategory;

  /// No description provided for @adminContentLanguage.
  ///
  /// In en, this message translates to:
  /// **'Content language'**
  String get adminContentLanguage;

  /// No description provided for @adminContentNoCategories.
  ///
  /// In en, this message translates to:
  /// **'No content categories yet.'**
  String get adminContentNoCategories;

  /// No description provided for @adminContentLinkCount.
  ///
  /// In en, this message translates to:
  /// **'{count} links'**
  String adminContentLinkCount(Object count);

  /// No description provided for @adminAuditLogTitle.
  ///
  /// In en, this message translates to:
  /// **'Audit log'**
  String get adminAuditLogTitle;

  /// No description provided for @adminAuditHelp.
  ///
  /// In en, this message translates to:
  /// **'Newest first. Filters are optional; page size 50.'**
  String get adminAuditHelp;

  /// No description provided for @adminAdminUserId.
  ///
  /// In en, this message translates to:
  /// **'Admin user id (GUID)'**
  String get adminAdminUserId;

  /// No description provided for @adminTargetType.
  ///
  /// In en, this message translates to:
  /// **'Target type (e.g. Games.Category)'**
  String get adminTargetType;

  /// No description provided for @adminTargetId.
  ///
  /// In en, this message translates to:
  /// **'Target id'**
  String get adminTargetId;

  /// No description provided for @adminApplyFilters.
  ///
  /// In en, this message translates to:
  /// **'Apply filters'**
  String get adminApplyFilters;

  /// No description provided for @adminFailedLoadAudit.
  ///
  /// In en, this message translates to:
  /// **'Failed to load audit log.'**
  String get adminFailedLoadAudit;

  /// No description provided for @adminNoAuditEntries.
  ///
  /// In en, this message translates to:
  /// **'No audit entries match the current filters.'**
  String get adminNoAuditEntries;

  /// No description provided for @adminOffset.
  ///
  /// In en, this message translates to:
  /// **'Offset {offset}'**
  String adminOffset(Object offset);

  /// No description provided for @adminPrev.
  ///
  /// In en, this message translates to:
  /// **'Prev'**
  String get adminPrev;

  /// No description provided for @adminNext.
  ///
  /// In en, this message translates to:
  /// **'Next'**
  String get adminNext;

  /// No description provided for @adminAuditAdmin.
  ///
  /// In en, this message translates to:
  /// **'admin: {id}'**
  String adminAuditAdmin(Object id);

  /// No description provided for @adminViewPayload.
  ///
  /// In en, this message translates to:
  /// **'View payload'**
  String get adminViewPayload;

  /// No description provided for @commonLoading.
  ///
  /// In en, this message translates to:
  /// **'Loading...'**
  String get commonLoading;

  /// No description provided for @adminQuestTriggerTotal.
  ///
  /// In en, this message translates to:
  /// **'Total games'**
  String get adminQuestTriggerTotal;

  /// No description provided for @adminQuestTriggerDaily.
  ///
  /// In en, this message translates to:
  /// **'Daily games'**
  String get adminQuestTriggerDaily;

  /// No description provided for @adminQuestTriggerAuthProvider.
  ///
  /// In en, this message translates to:
  /// **'Account linked'**
  String get adminQuestTriggerAuthProvider;

  /// No description provided for @adminProgressFromSnapshot.
  ///
  /// In en, this message translates to:
  /// **'From this point'**
  String get adminProgressFromSnapshot;

  /// No description provided for @adminProgressFromExistingTotal.
  ///
  /// In en, this message translates to:
  /// **'All time'**
  String get adminProgressFromExistingTotal;

  /// No description provided for @adminMarketTypeEnergy.
  ///
  /// In en, this message translates to:
  /// **'Energy'**
  String get adminMarketTypeEnergy;

  /// No description provided for @adminMarketTypeHint.
  ///
  /// In en, this message translates to:
  /// **'Hint'**
  String get adminMarketTypeHint;

  /// No description provided for @adminMarketTypeUndo.
  ///
  /// In en, this message translates to:
  /// **'Undo'**
  String get adminMarketTypeUndo;

  /// No description provided for @adminMarketTypeReset.
  ///
  /// In en, this message translates to:
  /// **'Reset'**
  String get adminMarketTypeReset;

  /// No description provided for @adminMarketTypeDiamond.
  ///
  /// In en, this message translates to:
  /// **'Diamond'**
  String get adminMarketTypeDiamond;

  /// No description provided for @adminLimitLifetime.
  ///
  /// In en, this message translates to:
  /// **'Lifetime'**
  String get adminLimitLifetime;

  /// No description provided for @adminLimitDaily.
  ///
  /// In en, this message translates to:
  /// **'Daily'**
  String get adminLimitDaily;

  /// No description provided for @adminLimitPerPromo.
  ///
  /// In en, this message translates to:
  /// **'Per promotion'**
  String get adminLimitPerPromo;

  /// No description provided for @adminMarketOrderSubtitle.
  ///
  /// In en, this message translates to:
  /// **'{price} diamonds - {purchasedAt}'**
  String adminMarketOrderSubtitle(Object price, Object purchasedAt);
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['de', 'en', 'es', 'fr', 'tr'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'de':
      return AppLocalizationsDe();
    case 'en':
      return AppLocalizationsEn();
    case 'es':
      return AppLocalizationsEs();
    case 'fr':
      return AppLocalizationsFr();
    case 'tr':
      return AppLocalizationsTr();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
