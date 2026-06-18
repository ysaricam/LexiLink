import 'dart:math' as math;

import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/auth/data/guest_device_id_store.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:shared_preferences_platform_interface/in_memory_shared_preferences_async.dart';
import 'package:shared_preferences_platform_interface/shared_preferences_async_platform_interface.dart';

void main() {
  const deviceIdKey = 'lexilink.guestDeviceId';

  setUp(() {
    SharedPreferencesAsyncPlatform.instance =
        InMemorySharedPreferencesAsync.empty();
    FlutterSecureStorage.setMockInitialValues({});
  });

  test('returns secure storage id when one exists', () async {
    FlutterSecureStorage.setMockInitialValues({deviceIdKey: 'secure-id'});

    final id = await GuestDeviceIdStore().readOrCreate(
      preferLegacyDeviceId: true,
    );

    expect(id, 'secure-id');
  });

  test('migrates shared preferences id into secure storage', () async {
    final preferences = SharedPreferencesAsync();
    await preferences.setString(deviceIdKey, 'prefs-id');

    final id = await GuestDeviceIdStore().readOrCreate();

    expect(id, 'prefs-id');
    expect(
      await const FlutterSecureStorage().read(key: deviceIdKey),
      'prefs-id',
    );
  });

  test(
    'uses legacy preview id only for an existing session migration',
    () async {
      final id = await GuestDeviceIdStore().readOrCreate(
        preferLegacyDeviceId: true,
      );

      expect(id, 'frontend-preview-device');
      expect(
        await const FlutterSecureStorage().read(key: deviceIdKey),
        'frontend-preview-device',
      );
      expect(await SharedPreferencesAsync().getString(deviceIdKey), id);
    },
  );

  test('creates a new random id for fresh installs', () async {
    final id = await GuestDeviceIdStore(random: _ZeroRandom()).readOrCreate();

    expect(id, isNot('frontend-preview-device'));
    expect(id, 'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA');
    expect(await const FlutterSecureStorage().read(key: deviceIdKey), id);
    expect(await SharedPreferencesAsync().getString(deviceIdKey), id);
  });
}

class _ZeroRandom implements math.Random {
  @override
  bool nextBool() => false;

  @override
  double nextDouble() => 0;

  @override
  int nextInt(int max) => 0;
}
