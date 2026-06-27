import 'package:flutter/material.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';

class AppLoadingState extends StatelessWidget {
  const AppLoadingState({
    this.message,
    this.compact = false,
    super.key,
  });

  final String? message;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    final indicator = SizedBox.square(
      dimension: compact ? 18 : 24,
      child: CircularProgressIndicator(
        strokeWidth: compact ? 2 : 2.5,
      ),
    );

    if (compact) {
      return Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          indicator,
          const SizedBox(width: 10),
          Text(
            message ?? context.l10n.commonLoading,
            style: textTheme.bodyMedium,
          ),
        ],
      );
    }

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: colorScheme.outline.withValues(alpha: 0.28)),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: colorScheme.primary.withValues(alpha: 0.08),
            blurRadius: 16,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            indicator,
            const SizedBox(height: 12),
            Text(
              message ?? context.l10n.commonLoading,
              style: textTheme.bodyMedium?.copyWith(
                color: colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}
