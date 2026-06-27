import 'package:shared_preferences/shared_preferences.dart';

abstract interface class GameTutorialStore {
  Future<bool> isCompleted();

  Future<void> markCompleted();
}

class SharedPreferencesGameTutorialStore implements GameTutorialStore {
  SharedPreferencesGameTutorialStore({
    SharedPreferencesAsync? preferences,
  }) : _preferences = preferences ?? SharedPreferencesAsync();

  static const completedKey = 'lexilink.gameTutorial.completed.v1';

  final SharedPreferencesAsync _preferences;

  @override
  Future<bool> isCompleted() async {
    return await _preferences.getBool(completedKey) ?? false;
  }

  @override
  Future<void> markCompleted() {
    return _preferences.setBool(completedKey, true);
  }
}

class InMemoryGameTutorialStore implements GameTutorialStore {
  InMemoryGameTutorialStore({bool completed = false}) : _completed = completed;

  bool _completed;

  @override
  Future<bool> isCompleted() async => _completed;

  @override
  Future<void> markCompleted() async {
    _completed = true;
  }
}
