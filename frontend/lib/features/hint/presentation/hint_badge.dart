import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/features/hint/application/hint_cubit.dart';

class HintBadge extends StatelessWidget {
  const HintBadge({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<HintCubit, HintState>(
      builder: (context, state) {
        if (state.status == HintStatus.success && state.hint != null) {
          return _Pill(child: _HintContent(balance: state.hint!.balance));
        }

        if (state.status == HintStatus.failure) {
          return _Pill(
            child: Text(
              'Hint unavailable',
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
                'Loading hints...',
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ],
          ),
        );
      },
    );
  }
}

class _HintContent extends StatelessWidget {
  const _HintContent({required this.balance});

  final int balance;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.lightbulb_outline, size: 16, color: colorScheme.tertiary),
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
