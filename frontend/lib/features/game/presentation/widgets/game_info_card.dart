import 'package:flutter/material.dart';

class GameInfoCard extends StatelessWidget {
  const GameInfoCard({
    required this.label,
    required this.value,
    this.accented = false,
    super.key,
  });

  final String label;
  final String value;
  final bool accented;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: accented ? colorScheme.primaryContainer : colorScheme.surface,
        border: Border.all(color: colorScheme.outline.withValues(alpha: 0.55)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              label,
              style: textTheme.labelSmall?.copyWith(
                color: colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 6),
            Text(
              value,
              style: textTheme.titleMedium,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ),
      ),
    );
  }
}
