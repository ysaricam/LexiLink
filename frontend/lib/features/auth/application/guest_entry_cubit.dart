import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/session/application/session_cubit.dart';
import 'package:lexilink_app/shared/api/api_error.dart';

enum GuestEntryStatus {
  idle,
  submitting,
  success,
  failure,
}

class GuestEntryCubit extends Cubit<GuestEntryState> {
  GuestEntryCubit({
    required GuestPlayerRepository guestPlayerRepository,
    required SessionCubit sessionCubit,
  }) : _guestPlayerRepository = guestPlayerRepository,
       _sessionCubit = sessionCubit,
       super(const GuestEntryState.idle());

  final GuestPlayerRepository _guestPlayerRepository;
  final SessionCubit _sessionCubit;

  void reset() {
    emit(const GuestEntryState.idle());
  }

  Future<void> continueAsGuest({
    required String deviceId,
    required String displayName,
    required String locale,
  }) async {
    emit(const GuestEntryState.submitting());

    try {
      final playerId = await _guestPlayerRepository.registerGuest(
        deviceId: deviceId,
        displayName: displayName,
        locale: locale,
      );
      await _sessionCubit.setAuthenticated(playerId);
      emit(GuestEntryState.success(playerId: playerId));
    } on ApiException catch (error) {
      emit(GuestEntryState.failure(message: error.message));
    } on Exception {
      emit(
        const GuestEntryState.failure(
          message: 'We could not create a guest player. Try again.',
        ),
      );
    }
  }
}

class GuestEntryState extends Equatable {
  const GuestEntryState({
    required this.status,
    this.playerId,
    this.message,
  });

  const GuestEntryState.idle() : this(status: GuestEntryStatus.idle);

  const GuestEntryState.submitting()
    : this(status: GuestEntryStatus.submitting);

  const GuestEntryState.success({required String playerId})
    : this(
        status: GuestEntryStatus.success,
        playerId: playerId,
      );

  const GuestEntryState.failure({required String message})
    : this(
        status: GuestEntryStatus.failure,
        message: message,
      );

  final GuestEntryStatus status;
  final String? playerId;
  final String? message;

  bool get isSubmitting => status == GuestEntryStatus.submitting;

  @override
  List<Object?> get props => [status, playerId, message];
}
