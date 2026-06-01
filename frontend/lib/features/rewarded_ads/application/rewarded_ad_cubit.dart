import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/rewarded_ads/data/rewarded_ad_repository.dart';
import 'package:lexilink_app/features/rewarded_ads/data/rewarded_ad_status.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum RewardedAdStatusState {
  initial,
  loading,
  ready,
  watching,
  unavailable,
  failure,
}

class RewardedAdCubit extends Cubit<RewardedAdState> {
  RewardedAdCubit({
    required RewardedAdRepository repository,
    required AdsService adsService,
    required String? userId,
    required bool isSupported,
  }) : _repository = repository,
       _adsService = adsService,
       _userId = userId,
       _isSupported = isSupported,
       super(const RewardedAdState.initial());

  final RewardedAdRepository _repository;
  final AdsService _adsService;
  final String? _userId;
  final bool _isSupported;

  /// Loads the player's rewarded-ad standing. Marks the feature unavailable
  /// on web/desktop or when the player id is unknown.
  Future<void> load() async {
    if (!_isSupported || _userId == null || _userId.isEmpty) {
      emit(
        const RewardedAdState.unavailable(
          message: 'Rewarded ads are only available on the mobile app.',
        ),
      );
      return;
    }

    emit(const RewardedAdState.loading());
    try {
      final status = await _repository.getStatus();
      emit(RewardedAdState.ready(status: status));
    } on ApiException catch (error) {
      emit(RewardedAdState.failure(message: error.message));
    } on Exception {
      emit(
        const RewardedAdState.failure(
          message: 'We could not load rewarded ads. Try again.',
        ),
      );
    }
  }

  /// Shows a rewarded ad if the player still has grants left today. The Diamond
  /// grant is backend-owned via SSV, so after the ad closes we re-fetch the
  /// status (the grant may land slightly later) and flag `rewardJustWatched`
  /// so the screen can nudge the Diamond badge to refresh.
  Future<void> watch() async {
    final status = state.status;
    final data = state.data;
    if (status != RewardedAdStatusState.ready || data == null) return;
    if (data.isCapped) {
      emit(
        RewardedAdState.ready(
          status: data,
          message: 'Daily reward limit reached. Come back tomorrow.',
        ),
      );
      return;
    }

    emit(RewardedAdState.watching(status: data));
    await _adsService.showRewarded(userId: _userId!, onClosed: _onAdClosed);
  }

  Future<void> _onAdClosed() async {
    try {
      final refreshed = await _repository.getStatus();
      emit(RewardedAdState.ready(status: refreshed, rewardJustWatched: true));
    } on Exception {
      // Keep the last known status; the reward (if any) lands via SSV and the
      // next load reflects it. Still flag the watch so the badge refreshes.
      final last = state.data;
      if (last != null) {
        emit(RewardedAdState.ready(status: last, rewardJustWatched: true));
      }
    }
  }
}

class RewardedAdState extends Equatable {
  const RewardedAdState({
    required this.status,
    this.data,
    this.message,
    this.rewardJustWatched = false,
  });

  const RewardedAdState.initial()
    : this(status: RewardedAdStatusState.initial);

  const RewardedAdState.loading()
    : this(status: RewardedAdStatusState.loading);

  const RewardedAdState.ready({
    required RewardedAdStatus status,
    String? message,
    bool rewardJustWatched = false,
  }) : this(
         status: RewardedAdStatusState.ready,
         data: status,
         message: message,
         rewardJustWatched: rewardJustWatched,
       );

  const RewardedAdState.watching({required RewardedAdStatus status})
    : this(status: RewardedAdStatusState.watching, data: status);

  const RewardedAdState.unavailable({required String message})
    : this(status: RewardedAdStatusState.unavailable, message: message);

  const RewardedAdState.failure({required String message})
    : this(status: RewardedAdStatusState.failure, message: message);

  final RewardedAdStatusState status;
  final RewardedAdStatus? data;
  final String? message;

  /// One-shot signal that a rewarded ad just closed — the screen reacts by
  /// refreshing the Diamond badge.
  final bool rewardJustWatched;

  bool get isWatching => status == RewardedAdStatusState.watching;

  @override
  List<Object?> get props => [status, data, message, rewardJustWatched];
}
