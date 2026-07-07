import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';

class WordDefinitionPopover extends StatefulWidget {
  const WordDefinitionPopover({
    required this.word,
    required this.definition,
    required this.textStyle,
    this.textAlign,
    this.maxLines = 1,
    super.key,
  });

  final String word;
  final String? definition;
  final TextStyle? textStyle;
  final TextAlign? textAlign;
  final int maxLines;

  @override
  State<WordDefinitionPopover> createState() => _WordDefinitionPopoverState();
}

class _WordDefinitionPopoverState extends State<WordDefinitionPopover> {
  OverlayEntry? _entry;

  bool get _hasDefinition => widget.definition?.trim().isNotEmpty ?? false;

  @override
  void didUpdateWidget(covariant WordDefinitionPopover oldWidget) {
    super.didUpdateWidget(oldWidget);

    if (oldWidget.word != widget.word ||
        oldWidget.definition != widget.definition) {
      _hideDefinition();
    }
  }

  @override
  void dispose() {
    _hideDefinition();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final text = Text(
      widget.word,
      maxLines: widget.maxLines,
      overflow: TextOverflow.ellipsis,
      textAlign: widget.textAlign,
      style: widget.textStyle,
    );

    if (!_hasDefinition) return text;

    return Semantics(
      button: true,
      label: widget.word,
      child: MouseRegion(
        cursor: SystemMouseCursors.click,
        child: GestureDetector(
          behavior: HitTestBehavior.opaque,
          onTap: _toggleDefinition,
          child: text,
        ),
      ),
    );
  }

  void _toggleDefinition() {
    if (_entry == null) {
      _showDefinition();
    } else {
      _hideDefinition();
    }
  }

  void _showDefinition() {
    if (!_hasDefinition) return;

    final renderObject = context.findRenderObject();
    if (renderObject is! RenderBox || !renderObject.hasSize) return;

    final overlay = Overlay.of(context);
    final overlayRenderObject = overlay.context.findRenderObject();
    if (overlayRenderObject is! RenderBox) return;

    final anchorOffset = renderObject.localToGlobal(
      Offset.zero,
      ancestor: overlayRenderObject,
    );
    final anchorRect = anchorOffset & renderObject.size;
    final viewportSize = overlayRenderObject.size;
    const edgeInset = 16.0;
    const gap = 8.0;
    const estimatedBubbleHeight = 148.0;
    final availableWidth = viewportSize.width - edgeInset * 2;
    if (availableWidth <= 0) return;

    final double bubbleWidth = math.min(320, availableWidth);
    final left = (anchorRect.center.dx - bubbleWidth / 2).clamp(
      edgeInset,
      viewportSize.width - bubbleWidth - edgeInset,
    );
    final belowTop = anchorRect.bottom + gap;
    final belowFits =
        belowTop + estimatedBubbleHeight <= viewportSize.height - edgeInset;
    final showBelow = belowFits || anchorRect.top < viewportSize.height / 2;
    final top = showBelow ? belowTop : null;
    final bottom = showBelow
        ? null
        : viewportSize.height - anchorRect.top + gap;

    _entry = OverlayEntry(
      builder: (context) => Stack(
        children: [
          Positioned.fill(
            child: GestureDetector(
              behavior: HitTestBehavior.translucent,
              onTap: _hideDefinition,
              child: const SizedBox.expand(),
            ),
          ),
          Positioned(
            left: left,
            top: top,
            bottom: bottom,
            width: bubbleWidth,
            child: _DefinitionBubble(
              word: widget.word,
              definition: widget.definition!.trim(),
            ),
          ),
        ],
      ),
    );

    overlay.insert(_entry!);
  }

  void _hideDefinition() {
    _entry?.remove();
    _entry = null;
  }
}

class _DefinitionBubble extends StatelessWidget {
  const _DefinitionBubble({
    required this.word,
    required this.definition,
  });

  final String word;
  final String definition;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Material(
      color: Colors.transparent,
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: colorScheme.surface,
          border: Border.all(
            color: colorScheme.outline.withValues(alpha: 0.36),
          ),
          borderRadius: BorderRadius.circular(14),
          boxShadow: [
            BoxShadow(
              color: AppPalette.lightText.withValues(alpha: 0.14),
              blurRadius: 24,
              offset: const Offset(0, 12),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                word,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: theme.textTheme.labelLarge?.copyWith(
                  color: colorScheme.primary,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                definition,
                maxLines: 6,
                overflow: TextOverflow.ellipsis,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: colorScheme.onSurface,
                  fontWeight: FontWeight.w600,
                  height: 1.35,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
