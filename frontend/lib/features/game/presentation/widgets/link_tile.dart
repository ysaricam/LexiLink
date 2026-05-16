import 'package:flutter/material.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';

enum LinkTileTone {
  normal,
  current,
  target,
  disabled,
}

class LinkTile extends StatelessWidget {
  const LinkTile({
    required this.label,
    this.tone = LinkTileTone.normal,
    this.onPressed,
    super.key,
  });

  final String label;
  final LinkTileTone tone;
  final VoidCallback? onPressed;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;
    final colors = _colors(colorScheme);
    final isDisabled = tone == LinkTileTone.disabled || onPressed == null;

    return Material(
      color: colors.background,
      borderRadius: BorderRadius.circular(8),
      child: InkWell(
        borderRadius: BorderRadius.circular(8),
        onTap: isDisabled ? null : onPressed,
        child: DecoratedBox(
          decoration: BoxDecoration(
            border: Border.all(color: colors.border, width: colors.borderWidth),
            borderRadius: BorderRadius.circular(8),
          ),
          child: ConstrainedBox(
            constraints: const BoxConstraints(minHeight: 48, minWidth: 96),
            child: Center(
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 14,
                  vertical: 10,
                ),
                child: Text(
                  label,
                  style: textTheme.titleMedium?.copyWith(
                    color: colors.foreground,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  _LinkTileColors _colors(ColorScheme colorScheme) {
    return switch (tone) {
      LinkTileTone.current => _LinkTileColors(
        background: colorScheme.primaryContainer,
        foreground: colorScheme.onPrimaryContainer,
        border: colorScheme.primary,
        borderWidth: 1.5,
      ),
      LinkTileTone.target => _LinkTileColors(
        background: colorScheme.secondaryContainer,
        foreground: colorScheme.onSecondaryContainer,
        border: colorScheme.secondary,
        borderWidth: 1.5,
      ),
      LinkTileTone.disabled => _LinkTileColors(
        background: colorScheme.surfaceContainerHighest,
        foreground: colorScheme.onSurfaceVariant.withValues(alpha: 0.62),
        border: colorScheme.outline.withValues(alpha: 0.28),
        borderWidth: 1,
      ),
      LinkTileTone.normal => _LinkTileColors(
        background: colorScheme.surface,
        foreground: colorScheme.onSurface,
        border: AppPalette.primary.withValues(alpha: 0.24),
        borderWidth: 1,
      ),
    };
  }
}

class _LinkTileColors {
  const _LinkTileColors({
    required this.background,
    required this.foreground,
    required this.border,
    required this.borderWidth,
  });

  final Color background;
  final Color foreground;
  final Color border;
  final double borderWidth;
}
