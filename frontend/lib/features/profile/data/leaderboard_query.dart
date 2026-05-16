import 'package:equatable/equatable.dart';

enum LeaderboardOrderBy {
  bestScore('bestScore'),
  totalScore('totalScore'),
  gamesCompleted('gamesCompleted');

  const LeaderboardOrderBy(this.wireName);

  final String wireName;
}

enum LeaderboardPeriod {
  allTime('allTime'),
  daily('daily'),
  weekly('weekly');

  const LeaderboardPeriod(this.wireName);

  final String wireName;
}

class LeaderboardQuery extends Equatable {
  const LeaderboardQuery({
    this.orderBy = LeaderboardOrderBy.bestScore,
    this.period = LeaderboardPeriod.allTime,
    this.periodStart,
    this.limit = 50,
  });

  final LeaderboardOrderBy orderBy;
  final LeaderboardPeriod period;
  final DateTime? periodStart;
  final int limit;

  Map<String, String> toQueryParameters() {
    return {
      'orderBy': orderBy.wireName,
      'period': period.wireName,
      'limit': limit.toString(),
      if (periodStart != null) 'periodStart': _formatDate(periodStart!),
    };
  }

  LeaderboardQuery copyWith({
    LeaderboardOrderBy? orderBy,
    LeaderboardPeriod? period,
    DateTime? periodStart,
    int? limit,
  }) {
    return LeaderboardQuery(
      orderBy: orderBy ?? this.orderBy,
      period: period ?? this.period,
      periodStart: periodStart ?? this.periodStart,
      limit: limit ?? this.limit,
    );
  }

  static String _formatDate(DateTime date) {
    final year = date.year.toString().padLeft(4, '0');
    final month = date.month.toString().padLeft(2, '0');
    final day = date.day.toString().padLeft(2, '0');

    return '$year-$month-$day';
  }

  @override
  List<Object?> get props => [orderBy, period, periodStart, limit];
}
