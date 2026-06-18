import 'dart:convert';
import 'dart:math' as math;

import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

class GuestDeviceIdStore {
  GuestDeviceIdStore({
    SharedPreferencesAsync? preferences,
    FlutterSecureStorage? secureStorage,
    math.Random? random,
  }) : _preferences = preferences ?? SharedPreferencesAsync(),
       _secureStorage = secureStorage ?? const FlutterSecureStorage(),
       _random = random ?? math.Random.secure();

  static const _deviceIdKey = 'lexilink.guestDeviceId';

  final SharedPreferencesAsync _preferences;
  final FlutterSecureStorage _secureStorage;
  final math.Random _random;

  Future<String> readOrCreate({bool preferLegacyDeviceId = false}) async {
    final existing = await _readSecureDeviceId();
    if (existing != null && existing.isNotEmpty) {
      return existing;
    }

    final migrated = await _preferences.getString(_deviceIdKey);
    if (migrated != null && migrated.isNotEmpty) {
      await _writeSecureDeviceId(migrated);
      return migrated;
    }

    if (preferLegacyDeviceId) {
      await _persistDeviceId(_legacyPreviewDeviceId);
      return _legacyPreviewDeviceId;
    }

    final bytes = List<int>.generate(32, (_) => _random.nextInt(256));
    final id = base64UrlEncode(bytes).replaceAll('=', '');
    await _persistDeviceId(id);
    return id;
  }

  Future<void> _persistDeviceId(String id) async {
    await _writeSecureDeviceId(id);
    await _preferences.setString(_deviceIdKey, id);
  }

  Future<String?> _readSecureDeviceId() async {
    try {
      return await _secureStorage.read(key: _deviceIdKey);
    } on Object {
      return null;
    }
  }

  Future<void> _writeSecureDeviceId(String id) async {
    try {
      await _secureStorage.write(key: _deviceIdKey, value: id);
    } on Object {
      // SharedPreferences remains as the fallback for platforms/environments
      // where secure storage is unavailable.
    }
  }
}

const _legacyPreviewDeviceId = 'frontend-preview-device';
