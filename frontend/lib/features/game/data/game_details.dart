import 'package:equatable/equatable.dart';

class GameDetails extends Equatable {
  const GameDetails({
    required this.id,
    required this.playerId,
    required this.categoryId,
    required this.difficulty,
    required this.startLinkId,
    required this.startWord,
    required this.targetLinkId,
    required this.targetWord,
    required this.targetDescription,
    required this.currentLinkId,
    required this.currentWord,
    required this.currentDescription,
    required this.state,
    required this.score,
    required this.maxSteps,
    required this.stepsTaken,
    required this.hintsTotal,
    required this.hintsUsed,
    required this.undosTotal,
    required this.undosUsed,
    required this.resetsTotal,
    required this.resetsUsed,
    required this.history,
  });

  factory GameDetails.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final playerId = json['playerId'];
    final categoryId = json['categoryId'];
    final difficulty = json['difficulty'];
    final startLinkId = json['startLinkId'];
    final startWord = json['startWord'];
    final targetLinkId = json['targetLinkId'];
    final targetWord = json['targetWord'];
    final targetDescription = _optionalString(json['targetDescription']);
    final currentLinkId = json['currentLinkId'];
    final currentWord = json['currentWord'];
    final currentDescription = _optionalString(json['currentDescription']);
    final state = json['state'];
    final score = json['score'];
    final maxSteps = json['maxSteps'];
    final stepsTaken = json['stepsTaken'];
    final hintsTotal = json['hintsTotal'];
    final hintsUsed = json['HintsUsed'] ?? json['hintsUsed'];
    final undosTotal = json['undosTotal'];
    final undosUsed = json['undosUsed'];
    final resetsTotal = json['resetsTotal'];
    final resetsUsed = json['resetsUsed'];
    final history = json['history'];

    if (id is! String ||
        playerId is! String ||
        categoryId is! String ||
        difficulty is! String ||
        startLinkId is! String ||
        startWord is! String ||
        targetLinkId is! String ||
        targetWord is! String ||
        currentLinkId is! String ||
        currentWord is! String ||
        state is! String ||
        (score != null && score is! int) ||
        maxSteps is! int ||
        stepsTaken is! int ||
        hintsTotal is! int ||
        hintsUsed is! int ||
        undosTotal is! int ||
        undosUsed is! int ||
        resetsTotal is! int ||
        resetsUsed is! int ||
        history is! List<dynamic>) {
      throw StateError('Game response is missing required fields.');
    }

    return GameDetails(
      id: id,
      playerId: playerId,
      categoryId: categoryId,
      difficulty: difficulty,
      startLinkId: startLinkId,
      startWord: startWord,
      targetLinkId: targetLinkId,
      targetWord: targetWord,
      targetDescription: targetDescription,
      currentLinkId: currentLinkId,
      currentWord: currentWord,
      currentDescription: currentDescription,
      state: state,
      score: score as int?,
      maxSteps: maxSteps,
      stepsTaken: stepsTaken,
      hintsTotal: hintsTotal,
      hintsUsed: hintsUsed,
      undosTotal: undosTotal,
      undosUsed: undosUsed,
      resetsTotal: resetsTotal,
      resetsUsed: resetsUsed,
      history: history
          .map((item) {
            if (item is Map<String, dynamic>) {
              return GameHistoryStep.fromJson(item);
            }

            throw StateError('Game history response contains an invalid item.');
          })
          .toList(growable: false),
    );
  }

  final String id;
  final String playerId;
  final String categoryId;
  final String difficulty;
  final String startLinkId;
  final String startWord;
  final String targetLinkId;
  final String targetWord;
  final String? targetDescription;
  final String currentLinkId;
  final String currentWord;
  final String? currentDescription;
  final String state;
  final int? score;
  final int maxSteps;
  final int stepsTaken;
  final int hintsTotal;
  final int hintsUsed;
  final int undosTotal;
  final int undosUsed;
  final int resetsTotal;
  final int resetsUsed;
  final List<GameHistoryStep> history;

  int get stepsLeft => maxSteps - stepsTaken;
  int get hintsLeft => hintsTotal - hintsUsed;
  int get undosLeft => undosTotal - undosUsed;
  int get resetsLeft => resetsTotal - resetsUsed;
  String? get backtrackParentLinkId {
    final path = <String>[startLinkId];
    for (final step in history) {
      final existingIndex = path.indexOf(step.linkId);
      if (existingIndex >= 0) {
        path.removeRange(existingIndex + 1, path.length);
      } else {
        path.add(step.linkId);
      }
    }

    if (path.isEmpty || path.last != currentLinkId) {
      return null;
    }

    return path.length >= 2 ? path[path.length - 2] : null;
  }

  bool get isFinished =>
      state == 'Completed' || state == 'Failed' || state == 'Abandoned';

  GameDetails withWordDescriptions({
    String? currentDescription,
    String? targetDescription,
  }) {
    return GameDetails(
      id: id,
      playerId: playerId,
      categoryId: categoryId,
      difficulty: difficulty,
      startLinkId: startLinkId,
      startWord: startWord,
      targetLinkId: targetLinkId,
      targetWord: targetWord,
      targetDescription: targetDescription ?? this.targetDescription,
      currentLinkId: currentLinkId,
      currentWord: currentWord,
      currentDescription: currentDescription ?? this.currentDescription,
      state: state,
      score: score,
      maxSteps: maxSteps,
      stepsTaken: stepsTaken,
      hintsTotal: hintsTotal,
      hintsUsed: hintsUsed,
      undosTotal: undosTotal,
      undosUsed: undosUsed,
      resetsTotal: resetsTotal,
      resetsUsed: resetsUsed,
      history: history,
    );
  }

  @override
  List<Object?> get props => [
    id,
    playerId,
    categoryId,
    difficulty,
    startLinkId,
    startWord,
    targetLinkId,
    targetWord,
    targetDescription,
    currentLinkId,
    currentWord,
    currentDescription,
    state,
    score,
    maxSteps,
    stepsTaken,
    hintsTotal,
    hintsUsed,
    undosTotal,
    undosUsed,
    resetsTotal,
    resetsUsed,
    history,
  ];
}

String? _optionalString(Object? value) {
  if (value == null) return null;
  if (value is! String) {
    throw StateError('Game response contains an invalid optional string.');
  }

  final trimmed = value.trim();
  return trimmed.isEmpty ? null : trimmed;
}

class GameHistoryStep extends Equatable {
  const GameHistoryStep({
    required this.stepNumber,
    required this.linkId,
    required this.linkValue,
  });

  factory GameHistoryStep.fromJson(Map<String, dynamic> json) {
    final stepNumber = json['stepNumber'];
    final linkId = json['linkId'];
    final linkValue = json['linkValue'];

    if (stepNumber is! int ||
        linkId is! String ||
        linkId.isEmpty ||
        linkValue is! String ||
        linkValue.isEmpty) {
      throw StateError('Game history response is missing required fields.');
    }

    return GameHistoryStep(
      stepNumber: stepNumber,
      linkId: linkId,
      linkValue: linkValue,
    );
  }

  final int stepNumber;
  final String linkId;
  final String linkValue;

  @override
  List<Object> get props => [stepNumber, linkId, linkValue];
}
