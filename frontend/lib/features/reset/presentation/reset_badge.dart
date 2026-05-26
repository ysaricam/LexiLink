import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/features/reset/application/reset_cubit.dart';

class ResetBadge extends StatelessWidget {
  const ResetBadge({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ResetCubit, ResetState>(
      builder: (context, state) {
        if (state.status == ResetStatus.success && state.reset != null) {
          return _Pill(child: _ResetContent(balance: state.reset!.balance));
        }

        if (state.status == ResetStatus.failure) {
          return _Pill(
            child: Text(
              'Reset unavailable',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          );
        }

        return _Pill(
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              const SizedBox.square(
                dimension: 12,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
              const SizedBox(width: 8),
              Text(
                'Loading resets...',
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ],
          ),
        );
      },
    );
  }
}

class _ResetContent extends StatelessWidget {
  const _ResetContent({required this.balance});

  final int balance;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    // Reset uses errorContainer for a softer warning tone than .error;
    // visually distinguishes from the other three inventory badges
    // (energy/primary, hint/tertiary, undo/secondary).
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.restart_alt, size: 16, color: colorScheme.error),
        const SizedBox(width: 6),
        Text('$balance', style: textTheme.titleSmall),
      ],
    );
  }
}

class _Pill extends StatelessWidget {
  const _Pill({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: colorScheme.outline.withValues(alpha: 0.42)),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        child: child,
      ),
    );
  }
}
