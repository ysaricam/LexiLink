import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/game/presentation/game_option_tile_state.dart';

void main() {
  group('isGameOptionTileDisabled', () {
    test('disables every option while the screen is busy or finished', () {
      expect(
        isGameOptionTileDisabled(
          screenDisabled: true,
          optionIsActive: true,
          optionIsPrevious: true,
        ),
        isTrue,
      );
    });

    test('keeps active normal options enabled', () {
      expect(
        isGameOptionTileDisabled(
          screenDisabled: false,
          optionIsActive: true,
          optionIsPrevious: false,
        ),
        isFalse,
      );
    });

    test('disables inactive normal options', () {
      expect(
        isGameOptionTileDisabled(
          screenDisabled: false,
          optionIsActive: false,
          optionIsPrevious: false,
        ),
        isTrue,
      );
    });

    test('keeps inactive previous options enabled for undo', () {
      expect(
        isGameOptionTileDisabled(
          screenDisabled: false,
          optionIsActive: false,
          optionIsPrevious: true,
        ),
        isFalse,
      );
    });
  });
}
