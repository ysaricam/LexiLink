import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/game/data/game_details.dart';

void main() {
  test('parses completed game details', () {
    final game = GameDetails.fromJson(const {
      'id': 'game-1',
      'playerId': 'player-1',
      'categoryId': 'category-1',
      'difficulty': 'Easy',
      'startLinkId': 'link-0',
      'startWord': 'boks',
      'targetLinkId': 'link-3',
      'targetWord': 'antrenman',
      'targetDescription': 'calisma hazirligi',
      'currentLinkId': 'link-3',
      'currentWord': 'antrenman',
      'currentDescription': 'calisma hazirligi',
      'state': 'Completed',
      'score': 300,
      'maxSteps': 8,
      'stepsTaken': 3,
      'hintsTotal': 3,
      'hintsUsed': 0,
      'undosTotal': 5,
      'undosUsed': 0,
      'resetsTotal': 2,
      'resetsUsed': 0,
      'history': [
        {
          'stepNumber': 1,
          'linkId': 'link-1',
          'linkValue': 'dovus sporu',
        },
      ],
    });

    expect(game.isFinished, isTrue);
    expect(game.score, 300);
    expect(game.targetDescription, 'calisma hazirligi');
    expect(game.currentDescription, 'calisma hazirligi');
    expect(game.history.single.linkValue, 'dovus sporu');
  });

  test('keeps word descriptions nullable for older game responses', () {
    final game = GameDetails.fromJson(const {
      'id': 'game-1',
      'playerId': 'player-1',
      'categoryId': 'category-1',
      'difficulty': 'Easy',
      'startLinkId': 'sport',
      'startWord': 'Spor',
      'targetLinkId': 'target',
      'targetWord': 'Hedef',
      'currentLinkId': 'sport',
      'currentWord': 'Spor',
      'state': 'InProgress',
      'score': null,
      'maxSteps': 8,
      'stepsTaken': 0,
      'hintsTotal': 3,
      'hintsUsed': 0,
      'undosTotal': 5,
      'undosUsed': 0,
      'resetsTotal': 2,
      'resetsUsed': 0,
      'history': <Map<String, Object?>>[],
    });

    expect(game.currentDescription, isNull);
    expect(game.targetDescription, isNull);
  });

  test('normalizes blank word descriptions to null', () {
    final game = GameDetails.fromJson(const {
      'id': 'game-1',
      'playerId': 'player-1',
      'categoryId': 'category-1',
      'difficulty': 'Easy',
      'startLinkId': 'sport',
      'startWord': 'Spor',
      'targetLinkId': 'target',
      'targetWord': 'Hedef',
      'targetDescription': '   ',
      'currentLinkId': 'sport',
      'currentWord': 'Spor',
      'currentDescription': '',
      'state': 'InProgress',
      'score': null,
      'maxSteps': 8,
      'stepsTaken': 0,
      'hintsTotal': 3,
      'hintsUsed': 0,
      'undosTotal': 5,
      'undosUsed': 0,
      'resetsTotal': 2,
      'resetsUsed': 0,
      'history': <Map<String, Object?>>[],
    });

    expect(game.currentDescription, isNull);
    expect(game.targetDescription, isNull);
  });

  test('backtrack parent uses simplified path after walking backwards', () {
    final game = GameDetails.fromJson(const {
      'id': 'game-1',
      'playerId': 'player-1',
      'categoryId': 'category-1',
      'difficulty': 'Easy',
      'startLinkId': 'sport',
      'startWord': 'Spor',
      'targetLinkId': 'target',
      'targetWord': 'Hedef',
      'currentLinkId': 'athletics',
      'currentWord': 'Atletizm',
      'state': 'InProgress',
      'score': null,
      'maxSteps': 8,
      'stepsTaken': 3,
      'hintsTotal': 3,
      'hintsUsed': 0,
      'undosTotal': 5,
      'undosUsed': 0,
      'resetsTotal': 2,
      'resetsUsed': 0,
      'history': [
        {
          'stepNumber': 1,
          'linkId': 'athletics',
          'linkValue': 'Atletizm',
        },
        {
          'stepNumber': 2,
          'linkId': 'jump',
          'linkValue': 'Atlama',
        },
        {
          'stepNumber': 3,
          'linkId': 'athletics',
          'linkValue': 'Atletizm',
        },
      ],
    });

    expect(game.backtrackParentLinkId, 'sport');
  });
}
