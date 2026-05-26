import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/features/undo/application/undo_cubit.dart';

class UndoBadge extends StatelessWidget {
  const UndoBadge({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<UndoCubit, UndoState>(
      builder: (context, state) {
        if (state.status == UndoStatus.success && state.undo != null) {
          return _Pill(child: _UndoContent(balance: state.undo!.balance));
        }

        if (state.status == UndoStatus.failure) {
          return _Pill(
            child: Text(
              'Undo unavailable',
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
                'Loading undos...',
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ],
          ),
        );
      },
    );
  }
}

class _UndoContent extends StatelessWidget {
  const _UndoContent({required this.balance});

  final int balance;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.undo, size: 16, color: colorScheme.secondary),
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
