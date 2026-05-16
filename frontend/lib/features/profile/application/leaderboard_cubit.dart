import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_entry.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_query.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum LeaderboardStatus {
  initial,
  loading,
  success,
  failure,
}

class LeaderboardCubit extends Cubit<LeaderboardState> {
  LeaderboardCubit({
    required PlayerStatsRepository playerStatsRepository,
  }) : _playerStatsRepository = playerStatsRepository,
       super(const LeaderboardState.initial());

  final PlayerStatsRepository _playerStatsRepository;

  Future<void> loadLeaderboard({
    LeaderboardQuery query = const LeaderboardQuery(),
  }) async {
    emit(LeaderboardState.loading(query: query));

    try {
      final entries = await _playerStatsRepository.getLeaderboard(
        query: query,
      );
      emit(LeaderboardState.success(entries: entries, query: query));
    } on ApiException catch (error) {
      emit(LeaderboardState.failure(message: error.message, query: query));
    } on Exception {
      emit(
        LeaderboardState.failure(
          message: 'We could not load the leaderboard. Try again.',
          query: query,
        ),
      );
    }
  }

  Future<void> changePeriod(LeaderboardPeriod period) async {
    if (state.status == LeaderboardStatus.success &&
        state.query.period == period) {
      return;
    }

    await loadLeaderboard(query: state.query.copyWith(period: period));
  }
}

class LeaderboardState extends Equatable {
  const LeaderboardState({
    required this.status,
    this.entries = const [],
    this.query = const LeaderboardQuery(),
    this.message,
  });

  const LeaderboardState.initial() : this(status: LeaderboardStatus.initial);

  const LeaderboardState.loading({
    LeaderboardQuery query = const LeaderboardQuery(),
  }) : this(status: LeaderboardStatus.loading, query: query);

  const LeaderboardState.success({
    required List<LeaderboardEntry> entries,
    required LeaderboardQuery query,
  }) : this(
         status: LeaderboardStatus.success,
         entries: entries,
         query: query,
       );

  const LeaderboardState.failure({
    required String message,
    LeaderboardQuery query = const LeaderboardQuery(),
  }) : this(
         status: LeaderboardStatus.failure,
         message: message,
         query: query,
       );

  final LeaderboardStatus status;
  final List<LeaderboardEntry> entries;
  final LeaderboardQuery query;
  final String? message;

  bool get isLoading => status == LeaderboardStatus.loading;

  @override
  List<Object?> get props => [status, entries, query, message];
}
