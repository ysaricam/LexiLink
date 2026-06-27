import 'package:flutter/material.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';

class GameTutorialSheet extends StatefulWidget {
  const GameTutorialSheet({super.key});

  @override
  State<GameTutorialSheet> createState() => _GameTutorialSheetState();
}

class _GameTutorialSheetState extends State<GameTutorialSheet> {
  final PageController _controller = PageController();
  int _index = 0;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final steps = _steps(context);
    final isLast = _index == steps.length - 1;
    final theme = Theme.of(context);

    return SafeArea(
      child: Container(
        decoration: BoxDecoration(
          color: theme.colorScheme.surface,
          borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.08),
              blurRadius: 18,
              offset: const Offset(0, -4),
            ),
          ],
        ),
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Center(
              child: Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: theme.colorScheme.outline.withValues(alpha: 0.22),
                  borderRadius: BorderRadius.circular(999),
                ),
              ),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: Text(
                    context.l10n.gameTutorialTitle,
                    style: theme.textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                ),
                IconButton(
                  tooltip: context.l10n.gameTutorialSkip,
                  onPressed: () => Navigator.of(context).pop(true),
                  icon: const Icon(Icons.close_rounded),
                ),
              ],
            ),
            const SizedBox(height: 8),
            SizedBox(
              height: 220,
              child: PageView.builder(
                controller: _controller,
                onPageChanged: (value) => setState(() => _index = value),
                itemCount: steps.length,
                itemBuilder: (context, index) => _TutorialStepView(
                  step: steps[index],
                ),
              ),
            ),
            const SizedBox(height: 12),
            _StepDots(count: steps.length, index: _index),
            const SizedBox(height: 18),
            Row(
              children: [
                if (_index > 0)
                  TextButton.icon(
                    onPressed: _previous,
                    icon: const Icon(Icons.arrow_back_rounded),
                    label: Text(context.l10n.gameTutorialBack),
                  )
                else
                  TextButton(
                    onPressed: () => Navigator.of(context).pop(true),
                    child: Text(context.l10n.gameTutorialSkip),
                  ),
                const Spacer(),
                FilledButton.icon(
                  onPressed: isLast
                      ? () => Navigator.of(context).pop(true)
                      : _next,
                  icon: Icon(
                    isLast ? Icons.check_rounded : Icons.arrow_forward_rounded,
                  ),
                  label: Text(
                    isLast
                        ? context.l10n.gameTutorialDone
                        : context.l10n.gameTutorialNext,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  List<_TutorialStep> _steps(BuildContext context) {
    final l10n = context.l10n;
    return [
      _TutorialStep(
        icon: Icons.flag_rounded,
        color: AppPalette.primary,
        title: l10n.gameTutorialGoalTitle,
        body: l10n.gameTutorialGoalBody,
      ),
      _TutorialStep(
        icon: Icons.touch_app_rounded,
        color: AppPalette.focus,
        title: l10n.gameTutorialMoveTitle,
        body: l10n.gameTutorialMoveBody,
      ),
      _TutorialStep(
        icon: Icons.route_rounded,
        color: AppPalette.success,
        title: l10n.gameTutorialStepsTitle,
        body: l10n.gameTutorialStepsBody,
      ),
      _TutorialStep(
        icon: Icons.lightbulb_rounded,
        color: AppPalette.danger,
        title: l10n.gameTutorialPowerTitle,
        body: l10n.gameTutorialPowerBody,
      ),
    ];
  }

  Future<void> _next() async {
    await _controller.nextPage(
      duration: const Duration(milliseconds: 220),
      curve: Curves.easeOut,
    );
  }

  Future<void> _previous() async {
    await _controller.previousPage(
      duration: const Duration(milliseconds: 220),
      curve: Curves.easeOut,
    );
  }
}

class _TutorialStep {
  const _TutorialStep({
    required this.icon,
    required this.color,
    required this.title,
    required this.body,
  });

  final IconData icon;
  final Color color;
  final String title;
  final String body;
}

class _TutorialStepView extends StatelessWidget {
  const _TutorialStepView({required this.step});

  final _TutorialStep step;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Container(
          width: 68,
          height: 68,
          decoration: BoxDecoration(
            color: step.color.withValues(alpha: 0.12),
            shape: BoxShape.circle,
          ),
          child: Icon(step.icon, color: step.color, size: 34),
        ),
        const SizedBox(height: 18),
        Text(
          step.title,
          textAlign: TextAlign.center,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.w900,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          step.body,
          textAlign: TextAlign.center,
          style: theme.textTheme.bodyMedium?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
            height: 1.35,
          ),
        ),
      ],
    );
  }
}

class _StepDots extends StatelessWidget {
  const _StepDots({required this.count, required this.index});

  final int count;
  final int index;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        for (var i = 0; i < count; i++) ...[
          AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            width: i == index ? 18 : 7,
            height: 7,
            decoration: BoxDecoration(
              color: i == index
                  ? AppPalette.primary
                  : Theme.of(context).colorScheme.outline.withValues(
                      alpha: 0.28,
                    ),
              borderRadius: BorderRadius.circular(999),
            ),
          ),
          if (i != count - 1) const SizedBox(width: 6),
        ],
      ],
    );
  }
}
