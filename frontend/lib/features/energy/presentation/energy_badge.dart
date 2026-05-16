import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/energy/data/player_energy.dart';

class EnergyBadge extends StatelessWidget {
  const EnergyBadge({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<EnergyCubit, EnergyState>(
      builder: (context, state) {
        if (state.status == EnergyStatus.success && state.energy != null) {
          return _Pill(child: _EnergyContent(energy: state.energy!));
        }

        if (state.status == EnergyStatus.failure) {
          return _Pill(
            child: Text(
              'Energy unavailable',
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
                'Loading energy...',
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ],
          ),
        );
      },
    );
  }
}

class _EnergyContent extends StatelessWidget {
  const _EnergyContent({required this.energy});

  final PlayerEnergy energy;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;

    final countdown = energy.isFull
        ? 'Full'
        : 'Next in ${_formatSeconds(energy.secondsUntilNextRefill ?? 0)}';

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.bolt, size: 16, color: colorScheme.primary),
        const SizedBox(width: 6),
        Text(
          '${energy.currentAmount}/${energy.maximumAmount}',
          style: textTheme.titleSmall,
        ),
        const SizedBox(width: 10),
        Text(
          countdown,
          style: textTheme.bodySmall?.copyWith(
            color: colorScheme.onSurfaceVariant,
          ),
        ),
      ],
    );
  }

  static String _formatSeconds(int totalSeconds) {
    if (totalSeconds <= 0) {
      return '0s';
    }
    final minutes = totalSeconds ~/ 60;
    final seconds = totalSeconds % 60;
    if (minutes <= 0) {
      return '${seconds}s';
    }
    return seconds == 0 ? '${minutes}m' : '${minutes}m ${seconds}s';
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
