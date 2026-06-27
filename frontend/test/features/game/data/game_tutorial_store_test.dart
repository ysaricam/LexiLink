import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/game/data/game_tutorial_store.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:shared_preferences_platform_interface/in_memory_shared_preferences_async.dart';
import 'package:shared_preferences_platform_interface/shared_preferences_async_platform_interface.dart';

void main() {
  setUp(() {
    SharedPreferencesAsyncPlatform.instance =
        InMemorySharedPreferencesAsync.empty();
  });

  test('is incomplete by default', () async {
    final store = SharedPreferencesGameTutorialStore();

    expect(await store.isCompleted(), isFalse);
  });

  test('persists completion', () async {
    final store = SharedPreferencesGameTutorialStore();

    await store.markCompleted();

    expect(await store.isCompleted(), isTrue);
    expect(
      await SharedPreferencesAsync().getBool(
        SharedPreferencesGameTutorialStore.completedKey,
      ),
      isTrue,
    );
  });
}
