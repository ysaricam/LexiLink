// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Spanish Castilian (`es`).
class AppLocalizationsEs extends AppLocalizations {
  AppLocalizationsEs([String locale = 'es']) : super(locale);

  @override
  String get appTitle => 'LexiLink';

  @override
  String get settingsTitle => 'Ajustes';

  @override
  String get languageLabel => 'Idioma';

  @override
  String get commonCancel => 'Cancelar';

  @override
  String get commonApply => 'Aplicar';

  @override
  String get commonRetry => 'Reintentar';

  @override
  String get commonTryAgain => 'Inténtalo de nuevo.';

  @override
  String get commonStart => 'Empezar';

  @override
  String get commonStarting => 'Empezando...';

  @override
  String get commonRefresh => 'Actualizar';

  @override
  String get sessionStorageFailedTitle => 'Error de almacenamiento de sesión';

  @override
  String get sessionStorageFailedMessage =>
      'Reinicia la app e inténtalo de nuevo.';

  @override
  String get preparingSession => 'Preparando la sesión...';

  @override
  String get navProfile => 'Perfil';

  @override
  String get navQuests => 'Misiones';

  @override
  String get navMarket => 'Mercado';

  @override
  String get navDiamonds => 'Diamantes';

  @override
  String get navEarnDiamonds => 'Ganar diamantes';

  @override
  String get navSettings => 'Ajustes';

  @override
  String get loadingCategories => 'Cargando categorías...';

  @override
  String get couldNotLoadCategories => 'No se pudieron cargar las categorías';

  @override
  String get preparingCategories => 'Preparando las categorías...';

  @override
  String get couldNotStartGame => 'No se pudo iniciar la partida';

  @override
  String get chooseCategory => 'Elegir categoría';

  @override
  String get chooseCategorySubtitle =>
      'Elige un campo de palabras para tu próximo camino.';

  @override
  String get noCategoriesTitle => 'Aún no hay categorías';

  @override
  String get noCategoriesMessage =>
      'Añade contenido de categorías antes de iniciar una partida.';

  @override
  String get startEasyGame => 'Iniciar partida fácil';

  @override
  String get preparingGame => 'Preparando la partida...';

  @override
  String get loadingGame => 'Cargando la partida...';

  @override
  String get couldNotLoadGame => 'No se pudo cargar la partida';

  @override
  String get gameTitle => 'Partida';

  @override
  String get pickNextWord => 'Elige la siguiente palabra';

  @override
  String get noMovesTitle => 'No hay movimientos disponibles';

  @override
  String get noMovesMessage => 'Este enlace no tiene opciones de salida.';

  @override
  String get actionFailed => 'Acción fallida';

  @override
  String get backToHome => 'Volver al inicio';

  @override
  String get quitGameTitle => '¿Salir de la partida?';

  @override
  String get quitGameMessage =>
      'Esto abandonará tu partida actual y no ganarás puntos.';

  @override
  String get keepPlaying => 'Seguir jugando';

  @override
  String get quit => 'Salir';

  @override
  String get anchorTarget => 'Objetivo';

  @override
  String get currentLabel => 'Actual';

  @override
  String get moreActions => 'Más acciones';

  @override
  String get resultScore => 'Puntos';

  @override
  String get resultSteps => 'Pasos';

  @override
  String get resultHintsUsed => 'Pistas usadas';

  @override
  String get resultPath => 'Camino';

  @override
  String hudSteps(int taken, int max) {
    return 'Pasos $taken/$max';
  }

  @override
  String hudHints(int count) {
    return 'Pistas $count';
  }

  @override
  String hudScore(int score) {
    return 'Puntos $score';
  }

  @override
  String hintAction(int balance) {
    return 'Pista ($balance)';
  }

  @override
  String undoAction(int balance) {
    return 'Deshacer ($balance)';
  }

  @override
  String resetProgress(int balance) {
    return 'Reiniciar progreso ($balance)';
  }

  @override
  String get actionMakingStep => 'Haciendo el movimiento...';

  @override
  String get actionFindingHint => 'Buscando una pista...';

  @override
  String get actionUndoing => 'Deshaciendo...';

  @override
  String get actionResetting => 'Reiniciando...';

  @override
  String get actionAbandoning => 'Abandonando...';

  @override
  String get actionWorking => 'Procesando...';

  @override
  String get outcomeCompletedTitle => 'Completado';

  @override
  String outcomeCompletedSubtitle(String target) {
    return 'Llegaste a $target.';
  }

  @override
  String get outcomeFailedTitle => 'Sin pasos';

  @override
  String outcomeFailedSubtitle(String target) {
    return 'Te quedaste sin pasos antes de llegar a $target.';
  }

  @override
  String get outcomeAbandonedTitle => 'Abandonado';

  @override
  String get outcomeAbandonedSubtitle => 'Esta partida fue abandonada.';

  @override
  String get outcomeEndedSubtitle => 'Partida terminada.';

  @override
  String get commonBuy => 'Comprar';

  @override
  String get commonUnavailable => 'No disponible';

  @override
  String get commonProcessing => 'Procesando...';

  @override
  String get commonCheckBackLater => 'Vuelve más tarde.';

  @override
  String get commonUnlimited => 'ilimitado';

  @override
  String get marketTitle => 'Mercado';

  @override
  String get openingMarket => 'Abriendo el mercado...';

  @override
  String get fetchingOffers => 'Cargando ofertas...';

  @override
  String get marketUnavailable => 'Mercado no disponible';

  @override
  String get noOffersTitle => 'Aún no hay ofertas';

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
    return 'Existencias: $stock';
  }

  @override
  String yourRemaining(Object remaining) {
    return 'Te quedan: $remaining';
  }

  @override
  String buyConfirmTitle(Object quantity, Object type) {
    return '¿Comprar $quantity $type?';
  }

  @override
  String buyConfirmMessage(Object price) {
    return 'Esto costará $price diamantes.';
  }

  @override
  String get diamondUnavailable => 'Diamantes no disponibles';

  @override
  String get loadingDiamonds => 'Cargando diamantes...';

  @override
  String get openingDiamonds => 'Abriendo diamantes...';

  @override
  String get diamondsTitle => 'Diamantes';

  @override
  String get fetchingBundles => 'Cargando paquetes de diamantes...';

  @override
  String get purchasesUnavailable => 'Compras no disponibles';

  @override
  String get purchasesUnavailableMessage =>
      'La compra de diamantes no está disponible aquí.';

  @override
  String get couldNotLoadDiamonds => 'No se pudieron cargar los diamantes';

  @override
  String get noBundlesTitle => 'Aún no hay paquetes';

  @override
  String get openingRewards => 'Abriendo recompensas...';

  @override
  String get rewardWatchedSnack =>
      '¡Gracias por ver! Los diamantes llegan tras la verificación.';

  @override
  String get loadingRewards => 'Cargando recompensas...';

  @override
  String get rewardedUnavailableTitle =>
      'Anuncios recompensados no disponibles';

  @override
  String get rewardedUnavailableMessage =>
      'Abre la app móvil para ver y ganar.';

  @override
  String get couldNotLoadRewards => 'No se pudieron cargar las recompensas';

  @override
  String get rewardLoadingAd => 'Cargando anuncio...';

  @override
  String get rewardDailyLimitReached => 'Límite diario alcanzado';

  @override
  String rewardWatchEarn(Object amount) {
    return 'Ver y ganar $amount 💎';
  }

  @override
  String rewardCardTitle(Object amount) {
    return 'Mira un anuncio corto y gana $amount diamantes';
  }

  @override
  String rewardToday(Object grants, Object limit, Object remaining) {
    return 'Hoy: $grants / $limit vistos • $remaining restantes';
  }

  @override
  String get rewardFooter =>
      'Los diamantes se acreditan después de que la red publicitaria verifique la recompensa; puede tardar un momento.';

  @override
  String get commonUnknown => 'desconocido';

  @override
  String get questsSubtitle => 'Completa misiones y gana energía extra.';

  @override
  String get questsLoading => 'Cargando misiones...';

  @override
  String get questsLoadError => 'No se pudieron cargar las misiones';

  @override
  String get noQuestsTitle => 'Aún no hay misiones';

  @override
  String get noQuestsMessage =>
      'Completa una partida y las misiones aparecerán aquí.';

  @override
  String get questClaiming => 'Reclamando...';

  @override
  String get questClaimReward => 'Reclamar recompensa';

  @override
  String get questStateReady => 'Lista';

  @override
  String get questStateActive => 'Activa';

  @override
  String get questStateClaimed => 'Reclamada';

  @override
  String get loadingProfile => 'Cargando perfil...';

  @override
  String get couldNotLoadProfile => 'No se pudo cargar el perfil';

  @override
  String get noProfileTitle => 'Aún no hay perfil';

  @override
  String get noProfileMessage =>
      'Inicia una sesión de invitado para ver tu perfil.';

  @override
  String get viewLeaderboard => 'Ver clasificación';

  @override
  String get guestPlayer => 'Jugador invitado';

  @override
  String get guestSession => 'Sesión de invitado';

  @override
  String providersLinked(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: '$count proveedores vinculados',
      one: '1 proveedor vinculado',
    );
    return '$_temp0';
  }

  @override
  String get statGamesCompleted => 'Partidas completadas';

  @override
  String get statBestScore => 'Mejor puntuación';

  @override
  String get statTotalScore => 'Puntuación total';

  @override
  String get statLastCompleted => 'Última completada';

  @override
  String get leaderboardTitle => 'Clasificación';

  @override
  String get loadingLeaderboard => 'Cargando clasificación...';

  @override
  String get couldNotLoadLeaderboard => 'No se pudo cargar la clasificación';

  @override
  String get noScoresTitle => 'Aún no hay puntuaciones';

  @override
  String get leaderboardAllTime => 'Histórico';

  @override
  String get leaderboardDaily => 'Diario';

  @override
  String get leaderboardWeekly => 'Semanal';

  @override
  String get leaderboardAllTimeDesc =>
      'Mejor puntuación histórica entre todos los jugadores.';

  @override
  String get leaderboardDailyDesc => 'Mejor puntuación de hoy (UTC).';

  @override
  String get leaderboardWeeklyDesc =>
      'Mejor puntuación de esta semana (UTC, inicio el lunes).';

  @override
  String get leaderboardAllTimeEmpty =>
      'Completa una partida para aparecer en la clasificación.';

  @override
  String get leaderboardDailyEmpty =>
      'Aún no hay puntuaciones registradas hoy.';

  @override
  String get leaderboardWeeklyEmpty =>
      'Aún no hay puntuaciones registradas esta semana.';

  @override
  String get settingsSoundSection => 'Sonido';

  @override
  String get settingsMusic => 'Música';

  @override
  String get settingsMusicSubtitle => 'Música de fondo mientras juegas';

  @override
  String get settingsMusicVolume => 'Volumen de música';

  @override
  String get settingsSfx => 'Efectos de sonido';

  @override
  String get settingsSfxSubtitle =>
      'Toques, movimientos, victorias y recompensas';

  @override
  String get settingsSfxVolume => 'Volumen de efectos';

  @override
  String get commonSave => 'Guardar';

  @override
  String get commonCreate => 'Crear';

  @override
  String get commonClose => 'Cerrar';

  @override
  String get commonClear => 'Limpiar';

  @override
  String get commonEdit => 'Editar';

  @override
  String get commonDeactivate => 'Desactivar';

  @override
  String get commonReset => 'Restablecer';

  @override
  String get commonLoad => 'Cargar';

  @override
  String get commonRequired => 'Obligatorio';

  @override
  String get commonMustBeNumber => 'Debe ser un número';

  @override
  String get commonEnterNumber => 'Introduce un número';

  @override
  String get commonGreaterThanZero => 'Debe ser mayor que 0';

  @override
  String get commonNonNegative => 'Debe ser >= 0';

  @override
  String get commonNoDash => '—';

  @override
  String get adminLabel => 'Admin';

  @override
  String get adminConsole => 'Consola de admin';

  @override
  String adminMobileTitle(Object title) {
    return 'Admin · $title';
  }

  @override
  String get adminSignOut => 'Cerrar sesión';

  @override
  String get adminNavQuests => 'Misiones';

  @override
  String get adminNavPlayers => 'Jugadores';

  @override
  String get adminNavEnergy => 'Energía';

  @override
  String get adminNavHint => 'Pista';

  @override
  String get adminNavUndo => 'Deshacer';

  @override
  String get adminNavReset => 'Restablecer';

  @override
  String get adminNavDiamond => 'Diamante';

  @override
  String get adminNavMarket => 'Mercado';

  @override
  String get adminNavContent => 'Contenido';

  @override
  String get adminNavAudit => 'Auditoría';

  @override
  String get adminSignInTitle => 'Inicio de sesión admin';

  @override
  String get adminSignInHelp =>
      'Verificador de desarrollo: introduce tu correo de admin y el token exacto \"dev:admin:<email>\". El SSO de producción llegará más adelante.';

  @override
  String get adminEmailLabel => 'Correo';

  @override
  String get adminExternalTokenLabel => 'Token externo';

  @override
  String get adminSigningIn => 'Iniciando sesión...';

  @override
  String get adminSignIn => 'Iniciar sesión';

  @override
  String get adminSignInFailed => 'Error al iniciar sesión.';

  @override
  String get adminLookUp => 'Buscar';

  @override
  String get adminPlayerGuid => 'GUID del jugador';

  @override
  String get adminPlayerHandle => 'Handle del jugador';

  @override
  String get adminPlayerId => 'ID de jugador';

  @override
  String get adminLookupFailed => 'Búsqueda fallida.';

  @override
  String get adminNoPlayerFound => 'No se encontró ningún jugador.';

  @override
  String get adminPlayerConsoleTitle => 'Consola de jugadores';

  @override
  String get adminPlayerConsoleHelp =>
      'Busca jugadores por handle con el formato DisplayName#1234.';

  @override
  String get adminId => 'ID';

  @override
  String get adminLocale => 'Idioma';

  @override
  String get adminAuthProvidersLinked => 'Proveedores auth vinculados';

  @override
  String get adminCreated => 'Creado';

  @override
  String get adminBannedAt => 'Bloqueado el';

  @override
  String get adminReason => 'Motivo';

  @override
  String get adminBan => 'Bloquear';

  @override
  String get adminUnban => 'Desbloquear';

  @override
  String get adminBanned => 'Bloqueado';

  @override
  String get adminGuest => 'Invitado';

  @override
  String get adminBanPlayerTitle => 'Bloquear jugador';

  @override
  String get adminUnbanPlayerTitle => '¿Desbloquear jugador?';

  @override
  String adminUnbanPlayerMessage(Object handle) {
    return '$handle podrá iniciar sesión de nuevo. El historial del motivo del bloqueo se conserva en el registro de auditoría.';
  }

  @override
  String get adminQuestsTitle => 'Definiciones de misiones';

  @override
  String get adminNewQuest => 'Nueva misión';

  @override
  String get adminQuestLoadError =>
      'No se pudieron cargar las definiciones de misiones.';

  @override
  String get adminNoQuestDefinitions =>
      'Aún no hay definiciones de misiones. Empieza con \"Nueva misión\".';

  @override
  String adminQuestPrerequisite(Object name) {
    return 'Requisito: $name';
  }

  @override
  String get adminQuestEditTooltip => 'Editar';

  @override
  String get adminQuestDeactivateTooltip => 'Desactivar';

  @override
  String get adminQuestReactivateTooltip => 'Reactivar';

  @override
  String get adminInactive => 'Inactivo';

  @override
  String get adminQuestDeactivateTitle => '¿Desactivar definición de misión?';

  @override
  String adminQuestDeactivateMessage(Object name) {
    return '\"$name\" ya no se asignará a jugadores. El progreso existente no se verá afectado.';
  }

  @override
  String get adminQuestFormEditTitle => 'Editar definición de misión';

  @override
  String get adminQuestFormCreateTitle => 'Nueva definición de misión';

  @override
  String get adminName => 'Nombre';

  @override
  String get adminImmutableAfterCreate =>
      'No se puede cambiar después de crear.';

  @override
  String get adminNameRequired => 'El nombre es obligatorio';

  @override
  String get adminMax64 => 'Máximo 64 caracteres';

  @override
  String get adminDescription => 'Descripción';

  @override
  String get adminMax256 => 'Máximo 256 caracteres';

  @override
  String get adminTrigger => 'Disparador';

  @override
  String get adminTriggerRequired => 'El disparador es obligatorio';

  @override
  String get adminThreshold => 'Umbral';

  @override
  String get adminEnergyReward => 'Recompensa de energía';

  @override
  String get adminHintReward => 'Recompensa de pista';

  @override
  String get adminUndoReward => 'Recompensa de deshacer';

  @override
  String get adminResetReward => 'Recompensa de restablecer';

  @override
  String get adminDiamondReward => 'Recompensa de diamantes';

  @override
  String get adminProgressBaseline => 'Base de progreso';

  @override
  String get adminProgressBaselineHelp =>
      'Solo tiene sentido para \"Juegos totales\".';

  @override
  String get adminPrerequisiteOptional => 'Requisito (opcional)';

  @override
  String get adminNoPrerequisite => '— ninguno —';

  @override
  String get adminRewardPositiveRequired =>
      'Al menos una recompensa (energía, pista, deshacer, restablecer, diamante) debe ser mayor que 0.';

  @override
  String get adminPositiveRequired => 'Debe ser mayor que 0';

  @override
  String get adminNotNegative => 'No puede ser menor que 0';

  @override
  String get adminEnergyConsoleTitle => 'Consola de energía';

  @override
  String get adminEnergyConsoleHelp =>
      'Busca por GUID del jugador y luego fija / concede / restablece. Conceder permite superar el máximo intencionalmente.';

  @override
  String get adminNoEnergyAggregate => 'No hay agregado de energía.';

  @override
  String get adminOverMax => 'Sobre el máximo';

  @override
  String get adminFull => 'Lleno';

  @override
  String get adminRechargeInterval => 'Intervalo de recarga';

  @override
  String get adminLastRefilled => 'Última recarga';

  @override
  String get adminNextRefillIn => 'Siguiente recarga en';

  @override
  String get adminFullyRefilledAt => 'Completamente recargado a';

  @override
  String get adminSetAmount => 'Fijar cantidad';

  @override
  String get adminGrantBonus => 'Conceder bonus';

  @override
  String get adminResetToFull => 'Restablecer a lleno';

  @override
  String get adminSetEnergyAmountTitle => 'Fijar cantidad de energía';

  @override
  String get adminNewCurrentAmount => 'Nueva cantidad actual';

  @override
  String get adminSetEnergyHelper =>
      'Fija la energía actual del jugador en este valor (>= 0).';

  @override
  String get adminGrantBonusEnergyTitle => 'Conceder energía bonus';

  @override
  String get adminBonusAmount => 'Cantidad bonus';

  @override
  String get adminGrantEnergyHelper =>
      'Se añade encima; puede dejar la cantidad actual por encima del máximo.';

  @override
  String get adminResetEnergyTitle => '¿Restablecer energía?';

  @override
  String get adminResetEnergyMessage =>
      'Restablece al jugador a la energía máxima.';

  @override
  String get adminHintConsoleTitle => 'Consola de pistas';

  @override
  String get adminHintConsoleHelp =>
      'Busca por GUID del jugador y luego fija / concede / restablece. El inventario de pistas no tiene máximo.';

  @override
  String get adminNoHintInventory => 'No hay inventario de pistas.';

  @override
  String get adminSetBalance => 'Fijar saldo';

  @override
  String get adminGrantHints => 'Conceder pistas';

  @override
  String get adminResetToZero => 'Restablecer a cero';

  @override
  String get adminSetHintBalanceTitle => 'Fijar saldo de pistas';

  @override
  String get adminNewBalance => 'Nuevo saldo';

  @override
  String get adminSetHintHelper =>
      'Fija el saldo de pistas del jugador en este valor (>= 0).';

  @override
  String get adminHintAmount => 'Cantidad de pistas';

  @override
  String get adminGrantHintHelper => 'Se añade al saldo existente; sin máximo.';

  @override
  String get adminResetHintTitle => '¿Restablecer saldo de pistas?';

  @override
  String get adminResetHintMessage =>
      'Pone el saldo de pistas del jugador a cero.';

  @override
  String get adminUndoConsoleTitle => 'Consola de deshacer';

  @override
  String get adminUndoConsoleHelp =>
      'Busca por GUID del jugador y luego fija / concede / restablece. El inventario de deshacer no tiene máximo.';

  @override
  String get adminNoUndoInventory => 'No hay inventario de deshacer.';

  @override
  String get adminGrantUndos => 'Conceder deshacer';

  @override
  String get adminSetUndoBalanceTitle => 'Fijar saldo de deshacer';

  @override
  String get adminSetUndoHelper =>
      'Fija el saldo de deshacer del jugador en este valor (>= 0).';

  @override
  String get adminUndoAmount => 'Cantidad de deshacer';

  @override
  String get adminGrantUndoHelper => 'Se añade al saldo existente; sin máximo.';

  @override
  String get adminResetUndoTitle => '¿Restablecer saldo de deshacer?';

  @override
  String get adminResetUndoMessage =>
      'Pone el saldo de deshacer del jugador a cero.';

  @override
  String get adminResetConsoleTitle => 'Consola de restablecer';

  @override
  String get adminResetConsoleHelp =>
      'Busca por GUID del jugador y luego fija / concede / restablece. El inventario de restablecer no tiene máximo.';

  @override
  String get adminNoResetInventory => 'No hay inventario de restablecer.';

  @override
  String get adminGrantResets => 'Conceder restablecer';

  @override
  String get adminSetResetBalanceTitle => 'Fijar saldo de restablecer';

  @override
  String get adminSetResetHelper =>
      'Fija el saldo de restablecer del jugador en este valor (>= 0).';

  @override
  String get adminResetAmount => 'Cantidad de restablecer';

  @override
  String get adminGrantResetHelper =>
      'Se añade al saldo existente; sin máximo.';

  @override
  String get adminResetResetTitle => '¿Restablecer saldo de restablecer?';

  @override
  String get adminResetResetMessage =>
      'Pone el saldo de restablecer del jugador a cero.';

  @override
  String get adminDiamondConsoleTitle => 'Consola de diamantes';

  @override
  String get adminDiamondConsoleHelp =>
      'Busca por GUID del jugador y luego fija / concede / restablece. Diamante es una moneda sin límite.';

  @override
  String get adminNoDiamondInventory => 'No hay inventario de diamantes.';

  @override
  String get adminGrantDiamonds => 'Conceder diamantes';

  @override
  String get adminSetDiamondBalanceTitle => 'Fijar saldo de diamantes';

  @override
  String get adminBalance => 'Saldo';

  @override
  String get adminAmount => 'Cantidad';

  @override
  String get adminResetDiamondTitle => '¿Restablecer saldo de diamantes?';

  @override
  String get adminResetDiamondMessage =>
      'Esto pone el saldo de diamantes a cero.';

  @override
  String get adminMarketConsoleTitle => 'Consola de mercado';

  @override
  String get adminMarketConsoleHelp =>
      'Gestiona categorías de tienda, artículos con precio en diamantes e historial de compras de jugadores.';

  @override
  String get adminMarketCategories => 'Categorías';

  @override
  String get adminMarketItems => 'Artículos';

  @override
  String get adminMarketOrders => 'Pedidos';

  @override
  String get adminNewCategory => 'Nueva categoría';

  @override
  String get adminEditCategory => 'Editar categoría';

  @override
  String adminSortStatus(Object sortOrder, Object status) {
    return 'Orden $sortOrder - $status';
  }

  @override
  String get adminActive => 'Activo';

  @override
  String get adminNoMarketCategories => 'Aún no hay categorías de mercado.';

  @override
  String get adminNewItem => 'Nuevo artículo';

  @override
  String get adminEditItem => 'Editar artículo';

  @override
  String adminMarketItemSubtitle(Object category, Object price, Object stock) {
    return '$category - $price diamantes - existencias $stock';
  }

  @override
  String get adminStock => 'existencias';

  @override
  String get adminNoMarketItems => 'Aún no hay artículos de mercado.';

  @override
  String get adminNoMarketOrders =>
      'No hay pedidos de mercado para este jugador.';

  @override
  String get adminSortOrder => 'Orden';

  @override
  String get adminIcon => 'Icono';

  @override
  String get adminVisibilityStarts => 'Visibilidad empieza en (ISO, opcional)';

  @override
  String get adminVisibilityEnds => 'Visibilidad termina en (ISO, opcional)';

  @override
  String get adminNormal => 'Normal';

  @override
  String get adminPromotion => 'Promoción';

  @override
  String get adminCategory => 'Categoría';

  @override
  String get adminItemType => 'Tipo de artículo';

  @override
  String get adminQuantity => 'Cantidad';

  @override
  String get adminPriceDiamonds => 'Precio en diamantes';

  @override
  String get adminPromoPrice => 'Precio promo';

  @override
  String get adminPromotionStarts => 'Inicio de promoción';

  @override
  String get adminPromotionEnds => 'Fin de promoción';

  @override
  String get adminMaxStock => 'Existencias máximas';

  @override
  String get adminPerPlayerLimit => 'Límite por jugador';

  @override
  String get adminLimitWindow => 'Ventana de límite';

  @override
  String get adminMustBeLowerThanPrice => 'Debe ser menor que el precio';

  @override
  String get adminMustBeAfterStart => 'Debe ser posterior al inicio';

  @override
  String get adminContentConsoleTitle => 'Consola de contenido';

  @override
  String get adminContentLanguageFilter => 'Filtro de idioma';

  @override
  String get adminContentAllLanguages => 'Todos los idiomas';

  @override
  String get adminContentNewCategory => 'Nueva categoría de contenido';

  @override
  String get adminContentEditCategory => 'Editar categoría de contenido';

  @override
  String get adminContentLanguage => 'Idioma del contenido';

  @override
  String get adminContentNoCategories => 'Aún no hay categorías de contenido.';

  @override
  String adminContentLinkCount(Object count) {
    return '$count enlaces';
  }

  @override
  String get adminAuditLogTitle => 'Registro de auditoría';

  @override
  String get adminAuditHelp =>
      'Más recientes primero. Los filtros son opcionales; tamaño de página 50.';

  @override
  String get adminAdminUserId => 'ID de usuario admin (GUID)';

  @override
  String get adminTargetType => 'Tipo de destino (p. ej. Games.Category)';

  @override
  String get adminTargetId => 'ID de destino';

  @override
  String get adminApplyFilters => 'Aplicar filtros';

  @override
  String get adminFailedLoadAudit =>
      'No se pudo cargar el registro de auditoría.';

  @override
  String get adminNoAuditEntries =>
      'No hay entradas de auditoría que coincidan con los filtros actuales.';

  @override
  String adminOffset(Object offset) {
    return 'Offset $offset';
  }

  @override
  String get adminPrev => 'Anterior';

  @override
  String get adminNext => 'Siguiente';

  @override
  String adminAuditAdmin(Object id) {
    return 'admin: $id';
  }

  @override
  String get adminViewPayload => 'Ver payload';

  @override
  String get commonLoading => 'Cargando...';

  @override
  String get adminQuestTriggerTotal => 'Juegos totales';

  @override
  String get adminQuestTriggerDaily => 'Juegos diarios';

  @override
  String get adminQuestTriggerAuthProvider => 'Cuenta vinculada';

  @override
  String get adminProgressFromSnapshot => 'Desde este punto';

  @override
  String get adminProgressFromExistingTotal => 'Todo el historial';

  @override
  String get adminMarketTypeEnergy => 'Energía';

  @override
  String get adminMarketTypeHint => 'Pista';

  @override
  String get adminMarketTypeUndo => 'Deshacer';

  @override
  String get adminMarketTypeReset => 'Restablecer';

  @override
  String get adminMarketTypeDiamond => 'Diamante';

  @override
  String get adminLimitLifetime => 'De por vida';

  @override
  String get adminLimitDaily => 'Diario';

  @override
  String get adminLimitPerPromo => 'Por promoción';

  @override
  String adminMarketOrderSubtitle(Object price, Object purchasedAt) {
    return '$price diamantes - $purchasedAt';
  }
}
