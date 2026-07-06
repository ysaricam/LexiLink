import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/app/theme/app_theme.dart';
import 'package:lexilink_app/features/game/presentation/word_definition_popover.dart';

void main() {
  testWidgets('shows and dismisses the word definition on tap', (
    tester,
  ) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light(),
        home: const Scaffold(
          body: Center(
            child: WordDefinitionPopover(
              word: 'Spor',
              definition: 'Bedeni gelistiren etkinlik.',
              textStyle: TextStyle(fontSize: 24),
            ),
          ),
        ),
      ),
    );

    expect(find.text('Bedeni gelistiren etkinlik.'), findsNothing);

    await tester.tap(find.text('Spor'));
    await tester.pump();

    expect(find.text('Bedeni gelistiren etkinlik.'), findsOneWidget);

    await tester.tapAt(const Offset(4, 4));
    await tester.pump();

    expect(find.text('Bedeni gelistiren etkinlik.'), findsNothing);
  });

  testWidgets('does not open a bubble for blank definitions', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light(),
        home: const Scaffold(
          body: Center(
            child: WordDefinitionPopover(
              word: 'Spor',
              definition: '   ',
              textStyle: TextStyle(fontSize: 24),
            ),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Spor'));
    await tester.pump();

    expect(find.byType(Overlay), findsOneWidget);
    expect(find.text('   '), findsNothing);
  });

  testWidgets('closes the bubble when the word changes', (tester) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light(),
        home: const Scaffold(
          body: Center(
            child: WordDefinitionPopover(
              word: 'Spor',
              definition: 'Bedeni gelistiren etkinlik.',
              textStyle: TextStyle(fontSize: 24),
            ),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Spor'));
    await tester.pump();
    expect(find.text('Bedeni gelistiren etkinlik.'), findsOneWidget);

    await tester.pumpWidget(
      MaterialApp(
        theme: AppTheme.light(),
        home: const Scaffold(
          body: Center(
            child: WordDefinitionPopover(
              word: 'Hedef',
              definition: 'Ulasilmak istenen kelime.',
              textStyle: TextStyle(fontSize: 24),
            ),
          ),
        ),
      ),
    );
    await tester.pump();

    expect(find.text('Bedeni gelistiren etkinlik.'), findsNothing);
    expect(find.text('Ulasilmak istenen kelime.'), findsNothing);
  });
}
