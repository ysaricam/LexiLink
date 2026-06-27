import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class AppBackBar extends StatelessWidget {
  const AppBackBar({
    required this.title,
    this.fallbackRoute = '/home',
    this.onBack,
    this.trailing,
    super.key,
  });

  final String title;
  final String fallbackRoute;
  final VoidCallback? onBack;
  final Widget? trailing;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      children: [
        Material(
          color: theme.colorScheme.surface,
          shape: const CircleBorder(),
          elevation: 1,
          shadowColor: theme.colorScheme.primary.withValues(alpha: 0.18),
          child: InkWell(
            customBorder: const CircleBorder(),
            onTap: onBack ?? () => context.go(fallbackRoute),
            child: Padding(
              padding: const EdgeInsets.all(8),
              child: Icon(
                Icons.arrow_back,
                color: theme.colorScheme.primary,
              ),
            ),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Text(
            title,
            style: theme.textTheme.titleLarge?.copyWith(
              color: theme.colorScheme.onSurface,
              fontWeight: FontWeight.w800,
            ),
          ),
        ),
        if (trailing != null) trailing!,
      ],
    );
  }
}
