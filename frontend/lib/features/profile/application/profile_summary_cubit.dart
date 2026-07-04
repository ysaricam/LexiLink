import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/profile/data/player_stats.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum ProfileSummaryStatus {
  initial,
  loading,
  success,
  failure,
}

class ProfileSummaryCubit extends Cubit<ProfileSummaryState> {
  ProfileSummaryCubit({
    required PlayerStatsRepository playerStatsRepository,
    required TokenStore tokenStore,
  }) : _playerStatsRepository = playerStatsRepository,
       _tokenStore = tokenStore,
       super(const ProfileSummaryState.initial());

  final PlayerStatsRepository _playerStatsRepository;
  final TokenStore _tokenStore;

  Future<void> loadSummary() async {
    emit(const ProfileSummaryState.loading());

    try {
      final playerId = await _tokenStore.readPlayerId();
      if (playerId == null || playerId.isEmpty) {
        emit(
          const ProfileSummaryState.failure(
            message: 'Guest session is missing.',
          ),
        );
        return;
      }

      final stats = await _playerStatsRepository.getPlayerStats(playerId);
      final sessionMode = await _tokenStore.readSessionMode();
      emit(ProfileSummaryState.success(stats: stats, sessionMode: sessionMode));
    } on ApiException catch (error) {
      emit(ProfileSummaryState.failure(message: error.message));
    } on Exception {
      emit(
        const ProfileSummaryState.failure(
          message: 'We could not load profile stats. Try again.',
        ),
      );
    }
  }

  Future<String?> updateHandle({
    required String displayName,
    required int discriminator,
  }) async {
    final currentStats = state.stats;
    if (currentStats == null) {
      return 'Profile is not loaded.';
    }

    try {
      final playerId = await _tokenStore.readPlayerId();
      if (playerId == null || playerId.isEmpty) {
        return 'Guest session is missing.';
      }

      await _playerStatsRepository.updatePlayerProfile(
        playerId: playerId,
        avatarUrl: currentStats.avatarUrl,
        locale: currentStats.locale ?? 'en-US',
        displayName: displayName,
        discriminator: discriminator,
      );

      emit(
        ProfileSummaryState.success(
          stats: currentStats.copyWith(
            displayName: displayName,
            discriminator: discriminator,
            handle: '$displayName#${discriminator.toString().padLeft(4, '0')}',
            updatedAt: DateTime.now().toUtc(),
          ),
          sessionMode: state.sessionMode ?? await _tokenStore.readSessionMode(),
        ),
      );
      return null;
    } on ApiException catch (error) {
      return error.message;
    } on Exception {
      return 'We could not update your username. Try again.';
    }
  }
}

class ProfileSummaryState extends Equatable {
  const ProfileSummaryState({
    required this.status,
    this.stats,
    this.sessionMode,
    this.message,
  });

  const ProfileSummaryState.initial()
    : this(status: ProfileSummaryStatus.initial);

  const ProfileSummaryState.loading()
    : this(status: ProfileSummaryStatus.loading);

  const ProfileSummaryState.success({
    required PlayerStats stats,
    AuthSessionMode? sessionMode,
  }) : this(
         status: ProfileSummaryStatus.success,
         stats: stats,
         sessionMode: sessionMode,
       );

  const ProfileSummaryState.failure({required String message})
    : this(status: ProfileSummaryStatus.failure, message: message);

  final ProfileSummaryStatus status;
  final PlayerStats? stats;
  final AuthSessionMode? sessionMode;
  final String? message;

  bool get isLoading => status == ProfileSummaryStatus.loading;

  @override
  List<Object?> get props => [status, stats, sessionMode, message];
}
