// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'LexiLink';

  @override
  String get settingsTitle => 'Settings';

  @override
  String get languageLabel => 'Language';

  @override
  String get commonCancel => 'Cancel';

  @override
  String get commonApply => 'Apply';

  @override
  String get commonRetry => 'Retry';

  @override
  String get commonTryAgain => 'Try again.';

  @override
  String get commonStart => 'Start';

  @override
  String get commonStarting => 'Starting...';

  @override
  String get commonRefresh => 'Refresh';

  @override
  String get sessionStorageFailedTitle => 'Session storage failed';

  @override
  String get sessionStorageFailedMessage => 'Restart the app and try again.';

  @override
  String get preparingSession => 'Preparing session...';

  @override
  String get navProfile => 'Profile';

  @override
  String get navQuests => 'Quests';

  @override
  String get navMarket => 'Market';

  @override
  String get navDiamonds => 'Diamonds';

  @override
  String get navEarnDiamonds => 'Earn Diamonds';

  @override
  String get navSettings => 'Settings';

  @override
  String get loadingCategories => 'Loading categories...';

  @override
  String get couldNotLoadCategories => 'Could not load categories';

  @override
  String get preparingCategories => 'Preparing categories...';

  @override
  String get couldNotStartGame => 'Could not start game';

  @override
  String get chooseCategory => 'Choose category';

  @override
  String get chooseCategorySubtitle =>
      'Pick a word field for your next link path.';

  @override
  String get noCategoriesTitle => 'No categories yet';

  @override
  String get noCategoriesMessage =>
      'Add category content before starting a game.';

  @override
  String get startEasyGame => 'Start easy game';

  @override
  String get preparingGame => 'Preparing game...';

  @override
  String get loadingGame => 'Loading game...';

  @override
  String get couldNotLoadGame => 'Could not load game';

  @override
  String get gameTitle => 'Game';

  @override
  String get pickNextWord => 'Pick the next word';

  @override
  String get noMovesTitle => 'No moves available';

  @override
  String get noMovesMessage => 'This link has no outgoing choices.';

  @override
  String get actionFailed => 'Action failed';

  @override
  String get backToHome => 'Back to home';

  @override
  String get quitGameTitle => 'Quit game?';

  @override
  String get quitGameMessage =>
      'This will abandon your current game and you will not earn any score.';

  @override
  String get keepPlaying => 'Keep playing';

  @override
  String get quit => 'Quit';

  @override
  String get anchorTarget => 'Target';

  @override
  String get currentLabel => 'Current';

  @override
  String get moreActions => 'More actions';

  @override
  String get resultScore => 'Score';

  @override
  String get resultSteps => 'Steps';

  @override
  String get resultHintsUsed => 'Hints used';

  @override
  String get resultPath => 'Path';

  @override
  String hudSteps(int taken, int max) {
    return 'Steps $taken/$max';
  }

  @override
  String hudHints(int count) {
    return 'Hints $count';
  }

  @override
  String hudScore(int score) {
    return 'Score $score';
  }

  @override
  String hintAction(int balance) {
    return 'Hint ($balance)';
  }

  @override
  String undoAction(int balance) {
    return 'Undo ($balance)';
  }

  @override
  String resetProgress(int balance) {
    return 'Reset progress ($balance)';
  }

  @override
  String get actionMakingStep => 'Making step...';

  @override
  String get actionFindingHint => 'Finding hint...';

  @override
  String get actionUndoing => 'Undoing...';

  @override
  String get actionResetting => 'Resetting...';

  @override
  String get actionAbandoning => 'Abandoning...';

  @override
  String get actionWorking => 'Working...';

  @override
  String get outcomeCompletedTitle => 'Completed';

  @override
  String outcomeCompletedSubtitle(String target) {
    return 'You reached $target.';
  }

  @override
  String get outcomeFailedTitle => 'No steps left';

  @override
  String outcomeFailedSubtitle(String target) {
    return 'You ran out of steps before reaching $target.';
  }

  @override
  String get outcomeAbandonedTitle => 'Abandoned';

  @override
  String get outcomeAbandonedSubtitle => 'This game was abandoned.';

  @override
  String get outcomeEndedSubtitle => 'Game ended.';

  @override
  String get commonBuy => 'Buy';

  @override
  String get commonUnavailable => 'Unavailable';

  @override
  String get commonProcessing => 'Processing...';

  @override
  String get commonCheckBackLater => 'Check back later.';

  @override
  String get commonUnlimited => 'unlimited';

  @override
  String get marketTitle => 'Market';

  @override
  String get openingMarket => 'Opening market...';

  @override
  String get fetchingOffers => 'Fetching offers...';

  @override
  String get marketUnavailable => 'Market unavailable';

  @override
  String get noOffersTitle => 'No offers yet';

  @override
  String promoPrice(Object price) {
    return 'Promo $price ◆';
  }

  @override
  String price(Object price) {
    return '$price ◆';
  }

  @override
  String stockLabel(Object stock) {
    return 'Stock: $stock';
  }

  @override
  String yourRemaining(Object remaining) {
    return 'Your remaining: $remaining';
  }

  @override
  String buyConfirmTitle(Object quantity, Object type) {
    return 'Buy $quantity $type?';
  }

  @override
  String buyConfirmMessage(Object price) {
    return 'This will spend $price diamonds.';
  }

  @override
  String get diamondUnavailable => 'Diamond unavailable';

  @override
  String get energyTitle => 'Energy';

  @override
  String get loadingDiamonds => 'Loading diamonds...';

  @override
  String get openingDiamonds => 'Opening diamonds...';

  @override
  String get diamondsTitle => 'Diamonds';

  @override
  String get fetchingBundles => 'Fetching diamond bundles...';

  @override
  String get purchasesUnavailable => 'Purchases unavailable';

  @override
  String get purchasesUnavailableMessage =>
      'Diamonds purchase is not available here.';

  @override
  String get couldNotLoadDiamonds => 'Could not load diamonds';

  @override
  String get noBundlesTitle => 'No bundles yet';

  @override
  String get openingRewards => 'Opening rewards...';

  @override
  String get rewardWatchedSnack =>
      'Thanks for watching! Diamonds arrive once verified.';

  @override
  String get loadingRewards => 'Loading rewards...';

  @override
  String get rewardedUnavailableTitle => 'Rewarded ads unavailable';

  @override
  String get rewardedUnavailableMessage =>
      'Open the mobile app to watch and earn.';

  @override
  String get couldNotLoadRewards => 'Could not load rewards';

  @override
  String get rewardLoadingAd => 'Loading ad...';

  @override
  String get rewardDailyLimitReached => 'Daily limit reached';

  @override
  String rewardWatchEarn(Object amount) {
    return 'Watch & earn $amount 💎';
  }

  @override
  String rewardCardTitle(Object amount) {
    return 'Watch a short ad, earn $amount diamonds';
  }

  @override
  String rewardToday(Object grants, Object limit, Object remaining) {
    return 'Today: $grants / $limit watched • $remaining left';
  }

  @override
  String get rewardFooter =>
      'Diamonds are credited after the reward is verified by the ad network, so they may take a moment to appear.';

  @override
  String get commonUnknown => 'unknown';

  @override
  String get questsSubtitle => 'Complete quests, earn bonus energy.';

  @override
  String get questsLoading => 'Loading quests...';

  @override
  String get questsLoadError => 'Could not load quests';

  @override
  String get noQuestsTitle => 'No quests yet';

  @override
  String get noQuestsMessage =>
      'Complete a game and quests will start appearing here.';

  @override
  String get questClaiming => 'Claiming...';

  @override
  String get questClaimReward => 'Claim reward';

  @override
  String get questStateReady => 'Ready';

  @override
  String get questStateActive => 'Active';

  @override
  String get questStateClaimed => 'Claimed';

  @override
  String get loadingProfile => 'Loading profile...';

  @override
  String get couldNotLoadProfile => 'Could not load profile';

  @override
  String get noProfileTitle => 'No profile yet';

  @override
  String get noProfileMessage => 'Start a guest session to see your profile.';

  @override
  String get viewLeaderboard => 'View leaderboard';

  @override
  String get guestPlayer => 'Guest player';

  @override
  String get guestSession => 'Guest session';

  @override
  String providersLinked(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count providers linked',
      one: '1 provider linked',
    );
    return '$_temp0';
  }

  @override
  String get statGamesCompleted => 'Games completed';

  @override
  String get statBestScore => 'Best score';

  @override
  String get statTotalScore => 'Total score';

  @override
  String get statLastCompleted => 'Last completed';

  @override
  String get leaderboardTitle => 'Leaderboard';

  @override
  String get loadingLeaderboard => 'Loading leaderboard...';

  @override
  String get couldNotLoadLeaderboard => 'Could not load leaderboard';

  @override
  String get noScoresTitle => 'No scores yet';

  @override
  String get leaderboardAllTime => 'All-time';

  @override
  String get leaderboardDaily => 'Daily';

  @override
  String get leaderboardWeekly => 'Weekly';

  @override
  String get leaderboardAllTimeDesc => 'All-time best score across players.';

  @override
  String get leaderboardDailyDesc => 'Best score today (UTC).';

  @override
  String get leaderboardWeeklyDesc =>
      'Best score this week (UTC, Monday start).';

  @override
  String get leaderboardAllTimeEmpty =>
      'Complete a game to appear on the leaderboard.';

  @override
  String get leaderboardDailyEmpty => 'No scores recorded today yet.';

  @override
  String get leaderboardWeeklyEmpty => 'No scores recorded this week yet.';

  @override
  String get settingsSoundSection => 'Sound';

  @override
  String get settingsMusic => 'Music';

  @override
  String get settingsMusicSubtitle => 'Background music while you play';

  @override
  String get settingsMusicVolume => 'Music volume';

  @override
  String get settingsSfx => 'Sound effects';

  @override
  String get settingsSfxSubtitle => 'Taps, moves, wins and rewards';

  @override
  String get settingsSfxVolume => 'Sound effects volume';

  @override
  String get commonSave => 'Save';

  @override
  String get commonCreate => 'Create';

  @override
  String get commonClose => 'Close';

  @override
  String get commonClear => 'Clear';

  @override
  String get commonEdit => 'Edit';

  @override
  String get commonDeactivate => 'Deactivate';

  @override
  String get commonReset => 'Reset';

  @override
  String get commonLoad => 'Load';

  @override
  String get commonRequired => 'Required';

  @override
  String get commonMustBeNumber => 'Must be a number';

  @override
  String get commonEnterNumber => 'Enter a number';

  @override
  String get commonGreaterThanZero => 'Must be greater than 0';

  @override
  String get commonNonNegative => 'Must be >= 0';

  @override
  String get commonNoDash => '—';

  @override
  String get adminLabel => 'Admin';

  @override
  String get adminConsole => 'Admin console';

  @override
  String adminMobileTitle(Object title) {
    return 'Admin · $title';
  }

  @override
  String get adminSignOut => 'Sign out';

  @override
  String get adminNavQuests => 'Quests';

  @override
  String get adminNavPlayers => 'Players';

  @override
  String get adminNavEnergy => 'Energy';

  @override
  String get adminNavHint => 'Hint';

  @override
  String get adminNavUndo => 'Undo';

  @override
  String get adminNavReset => 'Reset';

  @override
  String get adminNavDiamond => 'Diamond';

  @override
  String get adminNavMarket => 'Market';

  @override
  String get adminNavContent => 'Content';

  @override
  String get adminNavAudit => 'Audit';

  @override
  String get adminSignInTitle => 'Admin sign-in';

  @override
  String get adminSignInHelp =>
      'Development verifier: enter your admin email and the literal \"dev:admin:<email>\" token. Production SSO arrives later.';

  @override
  String get adminEmailLabel => 'Email';

  @override
  String get adminExternalTokenLabel => 'External token';

  @override
  String get adminSigningIn => 'Signing in...';

  @override
  String get adminSignIn => 'Sign in';

  @override
  String get adminSignInFailed => 'Sign-in failed.';

  @override
  String get adminLookUp => 'Look up';

  @override
  String get adminPlayerGuid => 'Player GUID';

  @override
  String get adminPlayerHandle => 'Player handle';

  @override
  String get adminPlayerId => 'Player id';

  @override
  String get adminLookupFailed => 'Lookup failed.';

  @override
  String get adminNoPlayerFound => 'No player found.';

  @override
  String get adminPlayerConsoleTitle => 'Player console';

  @override
  String get adminPlayerConsoleHelp =>
      'Look up players by handle in DisplayName#1234 format.';

  @override
  String get adminId => 'Id';

  @override
  String get adminLocale => 'Locale';

  @override
  String get adminAuthProvidersLinked => 'Auth providers linked';

  @override
  String get adminCreated => 'Created';

  @override
  String get adminBannedAt => 'Banned at';

  @override
  String get adminReason => 'Reason';

  @override
  String get adminBan => 'Ban';

  @override
  String get adminUnban => 'Unban';

  @override
  String get adminBanned => 'Banned';

  @override
  String get adminGuest => 'Guest';

  @override
  String get adminBanPlayerTitle => 'Ban player';

  @override
  String get adminUnbanPlayerTitle => 'Unban player?';

  @override
  String adminUnbanPlayerMessage(Object handle) {
    return '$handle will be able to sign in again. The ban reason history is preserved on the audit log.';
  }

  @override
  String get adminQuestsTitle => 'Quest definitions';

  @override
  String get adminNewQuest => 'New quest';

  @override
  String get adminQuestLoadError => 'Quest definitions could not be loaded.';

  @override
  String get adminNoQuestDefinitions =>
      'No quest definitions yet. Start with \"New quest\".';

  @override
  String adminQuestPrerequisite(Object name) {
    return 'Prerequisite: $name';
  }

  @override
  String get adminQuestEditTooltip => 'Edit';

  @override
  String get adminQuestDeactivateTooltip => 'Deactivate';

  @override
  String get adminQuestReactivateTooltip => 'Reactivate';

  @override
  String get adminInactive => 'Inactive';

  @override
  String get adminQuestDeactivateTitle => 'Deactivate quest definition?';

  @override
  String adminQuestDeactivateMessage(Object name) {
    return '\"$name\" will no longer be issued to players. Existing player progress is not affected.';
  }

  @override
  String get adminQuestFormEditTitle => 'Edit quest definition';

  @override
  String get adminQuestFormCreateTitle => 'New quest definition';

  @override
  String get adminName => 'Name';

  @override
  String get adminImmutableAfterCreate => 'Cannot be changed after creation.';

  @override
  String get adminNameRequired => 'Name is required';

  @override
  String get adminMax64 => 'Maximum 64 characters';

  @override
  String get adminDescription => 'Description';

  @override
  String get adminMax256 => 'Maximum 256 characters';

  @override
  String get adminTrigger => 'Trigger';

  @override
  String get adminTriggerRequired => 'Trigger is required';

  @override
  String get adminThreshold => 'Threshold';

  @override
  String get adminEnergyReward => 'Energy reward';

  @override
  String get adminHintReward => 'Hint reward';

  @override
  String get adminUndoReward => 'Undo reward';

  @override
  String get adminResetReward => 'Reset reward';

  @override
  String get adminDiamondReward => 'Diamond reward';

  @override
  String get adminProgressBaseline => 'Progress baseline';

  @override
  String get adminProgressBaselineHelp =>
      'Only meaningful for \"Total games\".';

  @override
  String get adminPrerequisiteOptional => 'Prerequisite (optional)';

  @override
  String get adminNoPrerequisite => '— none —';

  @override
  String get adminRewardPositiveRequired =>
      'At least one reward (energy, hint, undo, reset, diamond) must be greater than 0.';

  @override
  String get adminPositiveRequired => 'Must be greater than 0';

  @override
  String get adminNotNegative => 'Cannot be less than 0';

  @override
  String get adminEnergyConsoleTitle => 'Energy console';

  @override
  String get adminEnergyConsoleHelp =>
      'Lookup by player GUID, then snap / grant / reset. Grant intentionally allows over-max balance.';

  @override
  String get adminNoEnergyAggregate => 'No energy aggregate.';

  @override
  String get adminOverMax => 'Over max';

  @override
  String get adminFull => 'Full';

  @override
  String get adminRechargeInterval => 'Recharge interval';

  @override
  String get adminLastRefilled => 'Last refilled';

  @override
  String get adminNextRefillIn => 'Next refill in';

  @override
  String get adminFullyRefilledAt => 'Fully refilled at';

  @override
  String get adminSetAmount => 'Set amount';

  @override
  String get adminGrantBonus => 'Grant bonus';

  @override
  String get adminResetToFull => 'Reset to full';

  @override
  String get adminSetEnergyAmountTitle => 'Set energy amount';

  @override
  String get adminNewCurrentAmount => 'New current amount';

  @override
  String get adminSetEnergyHelper =>
      'Snaps the player\'s current energy to this value (>= 0).';

  @override
  String get adminGrantBonusEnergyTitle => 'Grant bonus energy';

  @override
  String get adminBonusAmount => 'Bonus amount';

  @override
  String get adminGrantEnergyHelper =>
      'Added on top - may push current above maximum.';

  @override
  String get adminResetEnergyTitle => 'Reset energy?';

  @override
  String get adminResetEnergyMessage => 'Resets the player to maximum energy.';

  @override
  String get adminHintConsoleTitle => 'Hint console';

  @override
  String get adminHintConsoleHelp =>
      'Lookup by player GUID, then snap / grant / reset. Hint inventory has no max cap.';

  @override
  String get adminNoHintInventory => 'No hint inventory.';

  @override
  String get adminSetBalance => 'Set balance';

  @override
  String get adminGrantHints => 'Grant hints';

  @override
  String get adminResetToZero => 'Reset to zero';

  @override
  String get adminSetHintBalanceTitle => 'Set hint balance';

  @override
  String get adminNewBalance => 'New balance';

  @override
  String get adminSetHintHelper =>
      'Snaps the player\'s hint balance to this value (>= 0).';

  @override
  String get adminHintAmount => 'Hint amount';

  @override
  String get adminGrantHintHelper =>
      'Adds to the existing balance - no max cap.';

  @override
  String get adminResetHintTitle => 'Reset hint balance?';

  @override
  String get adminResetHintMessage =>
      'Sets the player\'s hint balance to zero.';

  @override
  String get adminUndoConsoleTitle => 'Undo console';

  @override
  String get adminUndoConsoleHelp =>
      'Lookup by player GUID, then snap / grant / reset. Undo inventory has no max cap.';

  @override
  String get adminNoUndoInventory => 'No undo inventory.';

  @override
  String get adminGrantUndos => 'Grant undos';

  @override
  String get adminSetUndoBalanceTitle => 'Set undo balance';

  @override
  String get adminSetUndoHelper =>
      'Snaps the player\'s undo balance to this value (>= 0).';

  @override
  String get adminUndoAmount => 'Undo amount';

  @override
  String get adminGrantUndoHelper =>
      'Adds to the existing balance - no max cap.';

  @override
  String get adminResetUndoTitle => 'Reset undo balance?';

  @override
  String get adminResetUndoMessage =>
      'Sets the player\'s undo balance to zero.';

  @override
  String get adminResetConsoleTitle => 'Reset console';

  @override
  String get adminResetConsoleHelp =>
      'Lookup by player GUID, then snap / grant / reset. Reset inventory has no max cap.';

  @override
  String get adminNoResetInventory => 'No reset inventory.';

  @override
  String get adminGrantResets => 'Grant resets';

  @override
  String get adminSetResetBalanceTitle => 'Set reset balance';

  @override
  String get adminSetResetHelper =>
      'Snaps the player\'s reset balance to this value (>= 0).';

  @override
  String get adminResetAmount => 'Reset amount';

  @override
  String get adminGrantResetHelper =>
      'Adds to the existing balance - no max cap.';

  @override
  String get adminResetResetTitle => 'Reset reset balance?';

  @override
  String get adminResetResetMessage =>
      'Sets the player\'s reset balance to zero.';

  @override
  String get adminDiamondConsoleTitle => 'Diamond console';

  @override
  String get adminDiamondConsoleHelp =>
      'Lookup by player GUID, then set / grant / reset. Diamond is uncapped currency.';

  @override
  String get adminNoDiamondInventory => 'No diamond inventory.';

  @override
  String get adminGrantDiamonds => 'Grant diamonds';

  @override
  String get adminSetDiamondBalanceTitle => 'Set diamond balance';

  @override
  String get adminBalance => 'Balance';

  @override
  String get adminAmount => 'Amount';

  @override
  String get adminResetDiamondTitle => 'Reset diamond balance?';

  @override
  String get adminResetDiamondMessage =>
      'This sets the Diamond balance to zero.';

  @override
  String get adminMarketConsoleTitle => 'Market console';

  @override
  String get adminMarketConsoleHelp =>
      'Manage shop categories, diamond-priced items, and player purchase history.';

  @override
  String get adminMarketCategories => 'Categories';

  @override
  String get adminMarketItems => 'Items';

  @override
  String get adminMarketOrders => 'Orders';

  @override
  String get adminNewCategory => 'New category';

  @override
  String get adminEditCategory => 'Edit category';

  @override
  String adminSortStatus(Object sortOrder, Object status) {
    return 'Sort $sortOrder - $status';
  }

  @override
  String get adminActive => 'Active';

  @override
  String get adminNoMarketCategories => 'No market categories yet.';

  @override
  String get adminNewItem => 'New item';

  @override
  String get adminEditItem => 'Edit item';

  @override
  String adminMarketItemSubtitle(Object category, Object price, Object stock) {
    return '$category - $price diamonds - stock $stock';
  }

  @override
  String get adminStock => 'stock';

  @override
  String get adminNoMarketItems => 'No market items yet.';

  @override
  String get adminNoMarketOrders => 'No market orders for this player.';

  @override
  String get adminSortOrder => 'Sort order';

  @override
  String get adminIcon => 'Icon';

  @override
  String get adminVisibilityStarts => 'Visibility starts at (ISO, optional)';

  @override
  String get adminVisibilityEnds => 'Visibility ends at (ISO, optional)';

  @override
  String get adminNormal => 'Normal';

  @override
  String get adminPromotion => 'Promotion';

  @override
  String get adminCategory => 'Category';

  @override
  String get adminItemType => 'Item type';

  @override
  String get adminQuantity => 'Quantity';

  @override
  String get adminPriceDiamonds => 'Price diamonds';

  @override
  String get adminPromoPrice => 'Promo price';

  @override
  String get adminPromotionStarts => 'Promotion starts';

  @override
  String get adminPromotionEnds => 'Promotion ends';

  @override
  String get adminMaxStock => 'Max stock';

  @override
  String get adminPerPlayerLimit => 'Per-player limit';

  @override
  String get adminLimitWindow => 'Limit window';

  @override
  String get adminMustBeLowerThanPrice => 'Must be lower than price';

  @override
  String get adminMustBeAfterStart => 'Must be after start';

  @override
  String get adminContentConsoleTitle => 'Content console';

  @override
  String get adminContentLanguageFilter => 'Language filter';

  @override
  String get adminContentAllLanguages => 'All languages';

  @override
  String get adminContentNewCategory => 'New content category';

  @override
  String get adminContentEditCategory => 'Edit content category';

  @override
  String get adminContentLanguage => 'Content language';

  @override
  String get adminContentNoCategories => 'No content categories yet.';

  @override
  String adminContentLinkCount(Object count) {
    return '$count links';
  }

  @override
  String get adminAuditLogTitle => 'Audit log';

  @override
  String get adminAuditHelp =>
      'Newest first. Filters are optional; page size 50.';

  @override
  String get adminAdminUserId => 'Admin user id (GUID)';

  @override
  String get adminTargetType => 'Target type (e.g. Games.Category)';

  @override
  String get adminTargetId => 'Target id';

  @override
  String get adminApplyFilters => 'Apply filters';

  @override
  String get adminFailedLoadAudit => 'Failed to load audit log.';

  @override
  String get adminNoAuditEntries =>
      'No audit entries match the current filters.';

  @override
  String adminOffset(Object offset) {
    return 'Offset $offset';
  }

  @override
  String get adminPrev => 'Prev';

  @override
  String get adminNext => 'Next';

  @override
  String adminAuditAdmin(Object id) {
    return 'admin: $id';
  }

  @override
  String get adminViewPayload => 'View payload';

  @override
  String get commonLoading => 'Loading...';

  @override
  String get adminQuestTriggerTotal => 'Total games';

  @override
  String get adminQuestTriggerDaily => 'Daily games';

  @override
  String get adminQuestTriggerAuthProvider => 'Account linked';

  @override
  String get adminProgressFromSnapshot => 'From this point';

  @override
  String get adminProgressFromExistingTotal => 'All time';

  @override
  String get adminMarketTypeEnergy => 'Energy';

  @override
  String get adminMarketTypeHint => 'Hint';

  @override
  String get adminMarketTypeUndo => 'Undo';

  @override
  String get adminMarketTypeReset => 'Reset';

  @override
  String get adminMarketTypeDiamond => 'Diamond';

  @override
  String get adminLimitLifetime => 'Lifetime';

  @override
  String get adminLimitDaily => 'Daily';

  @override
  String get adminLimitPerPromo => 'Per promotion';

  @override
  String adminMarketOrderSubtitle(Object price, Object purchasedAt) {
    return '$price diamonds - $purchasedAt';
  }
}
