import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';

void main() {
  test('iOS release config enables production interstitial ads only', () {
    final defines = _readDartDefines('ios/Flutter/Release.xcconfig');

    expect(defines, contains('LEXILINK_ENABLE_ADS=true'));
    expect(defines, contains('LEXILINK_ENABLE_REWARDED_ADS=false'));
    expect(
      defines,
      contains(
        'ADMOB_INTERSTITIAL_AD_UNIT_ID=ca-app-pub-2115638398802394/4516380950',
      ),
    );
    expect(
      defines,
      isNot(
        contains(
          'ADMOB_REWARDED_AD_UNIT_ID=ca-app-pub-2115638398802394/3077352370',
        ),
      ),
    );
    expect(
      defines.where((define) => define.startsWith('ADMOB_')),
      everyElement(isNot(contains('3940256099942544'))),
    );
  });
}

Set<String> _readDartDefines(String path) {
  final config = File(path);
  final line = config.readAsLinesSync().singleWhere(
    (line) => line.startsWith('DART_DEFINES='),
  );
  final encodedDefines = line.substring('DART_DEFINES='.length);

  return encodedDefines
      .split(',')
      .map((value) => utf8.decode(base64.decode(value)))
      .toSet();
}
