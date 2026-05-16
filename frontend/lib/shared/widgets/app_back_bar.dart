import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class AppBackBar extends StatelessWidget {
  const AppBackBar({
    required this.title,
    this.fallbackRoute = '/home',
    super.key,
  });

  final String title;
  final String fallbackRoute;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      children: [
        Material(
          color: theme.colorScheme.surface,
          shape: const CircleBorder(),
          elevation: 1,
          child: InkWell(
            customBorder: const CircleBorder(),
            onTap: () => context.go(fallbackRoute),
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
          child: Text(title, style: theme.textTheme.titleLarge),
        ),
      ],
    );
  }
}
