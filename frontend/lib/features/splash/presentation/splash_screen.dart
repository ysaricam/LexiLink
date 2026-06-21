import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with SingleTickerProviderStateMixin {
  static const _logoText = 'LexiLink';
  static const _letterStagger = Duration(milliseconds: 220);
  static const _letterDuration = Duration(milliseconds: 780);
  static const _holdAfterComplete = Duration(milliseconds: 480);
  static const _nextRoute = '/home';

  late final AnimationController _controller;
  late final Duration _totalDuration;

  @override
  void initState() {
    super.initState();
    _totalDuration = _letterStagger * (_logoText.length - 1) + _letterDuration;
    _controller = AnimationController(vsync: this, duration: _totalDuration)
      ..addStatusListener(_onStatus)
      ..forward();
  }

  Future<void> _onStatus(AnimationStatus status) async {
    if (status != AnimationStatus.completed) return;
    await Future<void>.delayed(_holdAfterComplete);
    if (!mounted) return;
    context.go(_nextRoute);
  }

  @override
  void dispose() {
    _controller
      ..removeStatusListener(_onStatus)
      ..dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final letterStyle = (theme.textTheme.displaySmall ?? const TextStyle())
        .copyWith(
          fontSize: 56,
          fontWeight: FontWeight.w700,
          letterSpacing: 1.4,
          color: colorScheme.secondary,
        );
    final grainColor = colorScheme.secondary;

    return Scaffold(
      backgroundColor: theme.scaffoldBackgroundColor,
      body: Center(
        child: AnimatedBuilder(
          animation: _controller,
          builder: (context, _) {
            return _SandLogo(
              text: _logoText,
              progress: _controller.value,
              totalMs: _totalDuration.inMilliseconds,
              staggerMs: _letterStagger.inMilliseconds,
              letterMs: _letterDuration.inMilliseconds,
              style: letterStyle,
              grainColor: grainColor,
            );
          },
        ),
      ),
    );
  }
}

class _SandLogo extends StatelessWidget {
  const _SandLogo({
    required this.text,
    required this.progress,
    required this.totalMs,
    required this.staggerMs,
    required this.letterMs,
    required this.style,
    required this.grainColor,
  });

  final String text;
  final double progress;
  final int totalMs;
  final int staggerMs;
  final int letterMs;
  final TextStyle style;
  final Color grainColor;

  double _letterProgress(int index) {
    final currentMs = progress * totalMs;
    final startMs = index * staggerMs;
    final endMs = startMs + letterMs;
    if (currentMs <= startMs) return 0;
    if (currentMs >= endMs) return 1;
    return (currentMs - startMs) / letterMs;
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        for (var i = 0; i < text.length; i++)
          _SandLetter(
            character: text[i],
            progress: _letterProgress(i),
            style: style,
            grainColor: grainColor,
            seed: i,
          ),
      ],
    );
  }
}

class _SandLetter extends StatelessWidget {
  const _SandLetter({
    required this.character,
    required this.progress,
    required this.style,
    required this.grainColor,
    required this.seed,
  });

  final String character;
  final double progress;
  final TextStyle style;
  final Color grainColor;
  final int seed;

  static const _fallDistance = 70.0;
  static const _trailHeight = 110.0;

  @override
  Widget build(BuildContext context) {
    final eased = Curves.easeOutCubic.transform(progress);
    final dy = (1 - eased) * -_fallDistance;
    final opacity = math.min(progress * 3, 1).clamp(0, 1).toDouble();
    final hasStarted = progress > 0 && progress < 1;

    return SizedBox(
      height: style.fontSize! * 1.6 + _trailHeight,
      child: Stack(
        alignment: Alignment.bottomCenter,
        clipBehavior: Clip.none,
        children: [
          if (hasStarted)
            Positioned.fill(
              bottom: style.fontSize! * 0.55,
              child: IgnorePointer(
                child: CustomPaint(
                  painter: _SandGrainPainter(
                    progress: progress,
                    color: grainColor,
                    seed: seed,
                  ),
                ),
              ),
            ),
          Transform.translate(
            offset: Offset(0, dy),
            child: Opacity(
              opacity: opacity,
              child: Text(character, style: style),
            ),
          ),
        ],
      ),
    );
  }
}

class _SandGrainPainter extends CustomPainter {
  _SandGrainPainter({
    required this.progress,
    required this.color,
    required this.seed,
  });

  final double progress;
  final Color color;
  final int seed;

  static const _grainCount = 7;

  @override
  void paint(Canvas canvas, Size size) {
    if (progress <= 0 || progress >= 1) return;

    final rng = math.Random(seed * 1009 + 7);
    final paint = Paint()..style = PaintingStyle.fill;
    final columnWidth = size.width * 0.62;
    final columnLeft = (size.width - columnWidth) / 2;
    final bell = math.sin(progress * math.pi);

    for (var g = 0; g < _grainCount; g++) {
      final phaseOffset = rng.nextDouble();
      final phase = (progress * 2.2 + phaseOffset) % 1.0;
      final dx = columnLeft + rng.nextDouble() * columnWidth;
      final dy = phase * size.height;
      final radius = 1.0 + rng.nextDouble() * 1.4;
      final grainOpacity = (1 - phase) * bell;
      if (grainOpacity <= 0) continue;
      paint.color = color.withValues(alpha: grainOpacity.clamp(0.0, 1.0));
      canvas.drawCircle(Offset(dx, dy), radius, paint);
    }
  }

  @override
  bool shouldRepaint(_SandGrainPainter old) =>
      old.progress != progress || old.color != color || old.seed != seed;
}
