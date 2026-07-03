import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/features/quests/data/quest_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum QuestsStatus {
  initial,
  loading,
  success,
  failure,
}

class QuestsCubit extends Cubit<QuestsState> {
  QuestsCubit({
    required QuestRepository questRepository,
  }) : _questRepository = questRepository,
       super(const QuestsState.initial());

  final QuestRepository _questRepository;

  Future<void> loadQuests() async {
    emit(state.toLoading());

    try {
      final quests = await _questRepository.getMe();
      emit(QuestsState.success(quests: quests));
    } on ApiException catch (error) {
      emit(QuestsState.failure(message: error.message));
    } on Exception {
      emit(
        const QuestsState.failure(
          message: 'We could not load quests. Try again.',
        ),
      );
    }
  }

  // Claim may leave an energy-only remainder on the server when the player is
  // near max energy. Other inventory rewards are queued asynchronously by the
  // backend outbox, so badges still refresh separately after a short delay.
  Future<bool> claim(String playerQuestId) async {
    if (state.claimingId != null) {
      return false;
    }

    emit(state.copyWith(claimingId: playerQuestId, claimMessage: null));

    try {
      await _questRepository.claim(playerQuestId);
      await _reloadAfterClaim();
      emit(
        state.copyWith(
          claimingId: null,
          claimMessage:
              'Reward queued — your inventories will update in a few seconds.',
        ),
      );
      return true;
    } on ApiException catch (error) {
      emit(state.copyWith(claimingId: null, claimMessage: error.message));
      return false;
    } on Exception {
      emit(
        state.copyWith(
          claimingId: null,
          claimMessage: 'We could not claim this quest. Try again.',
        ),
      );
      return false;
    }
  }

  void clearClaimMessage() {
    if (state.claimMessage == null) {
      return;
    }
    emit(state.copyWith(claimMessage: null));
  }

  Future<void> _reloadAfterClaim() async {
    try {
      final quests = await _questRepository.getMe();
      emit(QuestsState.success(quests: quests));
    } on Exception {
      // Swallow reload error: claim itself succeeded; UI keeps the previous
      // list. The user can pull/tap refresh to retry the reload.
    }
  }
}

class QuestsState extends Equatable {
  const QuestsState({
    required this.status,
    this.quests = const [],
    this.message,
    this.claimingId,
    this.claimMessage,
  });

  const QuestsState.initial() : this(status: QuestsStatus.initial);

  const QuestsState.loading({
    List<PlayerQuest> quests = const [],
  }) : this(status: QuestsStatus.loading, quests: quests);

  const QuestsState.success({required List<PlayerQuest> quests})
    : this(status: QuestsStatus.success, quests: quests);

  const QuestsState.failure({required String message})
    : this(status: QuestsStatus.failure, message: message);

  final QuestsStatus status;
  final List<PlayerQuest> quests;
  final String? message;
  final String? claimingId;
  final String? claimMessage;

  bool get isLoading => status == QuestsStatus.loading;

  QuestsState toLoading() =>
      QuestsState(status: QuestsStatus.loading, quests: quests);

  QuestsState copyWith({
    QuestsStatus? status,
    List<PlayerQuest>? quests,
    String? message,
    Object? claimingId = _sentinel,
    Object? claimMessage = _sentinel,
  }) {
    return QuestsState(
      status: status ?? this.status,
      quests: quests ?? this.quests,
      message: message ?? this.message,
      claimingId: identical(claimingId, _sentinel)
          ? this.claimingId
          : claimingId as String?,
      claimMessage: identical(claimMessage, _sentinel)
          ? this.claimMessage
          : claimMessage as String?,
    );
  }

  static const _sentinel = Object();

  @override
  List<Object?> get props => [
    status,
    quests,
    message,
    claimingId,
    claimMessage,
  ];
}
