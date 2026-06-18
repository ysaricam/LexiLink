import 'dart:convert';
import 'dart:math' as math;

import 'package:shared_preferences/shared_preferences.dart';

class SharedPreferencesGuestDeviceIdStore {
  SharedPreferencesGuestDeviceIdStore({
    SharedPreferencesAsync? preferences,
    math.Random? random,
  }) : _preferences = preferences ?? SharedPreferencesAsync(),
       _random = random ?? math.Random.secure();

  static const _deviceIdKey = 'lexilink.guestDeviceId';

  final SharedPreferencesAsync _preferences;
  final math.Random _random;

  Future<String> readOrCreate() async {
    final existing = await _preferences.getString(_deviceIdKey);
    if (existing != null && existing.isNotEmpty) {
      return existing;
    }

    final bytes = List<int>.generate(32, (_) => _random.nextInt(256));
    final id = base64UrlEncode(bytes).replaceAll('=', '');
    await _preferences.setString(_deviceIdKey, id);
    return id;
  }
}
