import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/features/diamond/application/diamond_cubit.dart';

class DiamondBadge extends StatelessWidget {
  const DiamondBadge({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<DiamondCubit, DiamondState>(
      builder: (context, state) {
        if (state.status == DiamondStatus.success && state.diamond != null) {
          return _Pill(
            child: _DiamondContent(balance: state.diamond!.balance),
          );
        }

        if (state.status == DiamondStatus.failure) {
          return _Pill(
            child: Text(
              'Diamond unavailable',
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
                'Loading diamonds...',
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ],
          ),
        );
      },
    );
  }
}

class _DiamondContent extends StatelessWidget {
  const _DiamondContent({required this.balance});

  final int balance;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.diamond_outlined, size: 16, color: colorScheme.primary),
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
