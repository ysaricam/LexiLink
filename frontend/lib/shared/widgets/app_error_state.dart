import 'package:flutter/material.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';

class AppErrorState extends StatelessWidget {
  const AppErrorState({
    required this.title,
    required this.message,
    this.retryLabel,
    this.onRetry,
    super.key,
  });

  final String title;
  final String message;
  final String? retryLabel;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(color: colorScheme.error.withValues(alpha: 0.34)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.error_outline,
              color: colorScheme.error,
              size: 28,
            ),
            const SizedBox(height: 12),
            Text(
              title,
              style: textTheme.titleMedium,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 6),
            Text(
              message,
              style: textTheme.bodyMedium?.copyWith(
                color: colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
            if (onRetry != null) ...[
              const SizedBox(height: 16),
              AppPrimaryButton(
                label: retryLabel ?? context.l10n.commonTryAgain,
                onPressed: onRetry,
              ),
            ],
          ],
        ),
      ),
    );
  }
}
