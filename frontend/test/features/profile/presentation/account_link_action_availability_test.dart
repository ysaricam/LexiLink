import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/profile/presentation/profile_summary_screen.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('accountLinkActionAvailability', () {
    test('keeps Apple link available after returning to guest mode', () {
      final availability = accountLinkActionAvailability(
        authProvidersLinked: 1,
        sessionMode: AuthSessionMode.guest,
      );

      expect(availability.canLinkApple, isTrue);
      expect(availability.canReturnToGuest, isFalse);
    });

    test('shows return to guest while Apple mode is active', () {
      final availability = accountLinkActionAvailability(
        authProvidersLinked: 1,
        sessionMode: AuthSessionMode.apple,
      );

      expect(availability.canLinkApple, isFalse);
      expect(availability.canReturnToGuest, isTrue);
    });

    test('falls back to provider count for older sessions without mode', () {
      final linkedAvailability = accountLinkActionAvailability(
        authProvidersLinked: 1,
        sessionMode: null,
      );
      final guestAvailability = accountLinkActionAvailability(
        authProvidersLinked: 0,
        sessionMode: null,
      );

      expect(linkedAvailability.canLinkApple, isFalse);
      expect(linkedAvailability.canReturnToGuest, isTrue);
      expect(guestAvailability.canLinkApple, isTrue);
      expect(guestAvailability.canReturnToGuest, isFalse);
    });
  });
}
