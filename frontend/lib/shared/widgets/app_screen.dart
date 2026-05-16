import 'package:flutter/material.dart';
import 'package:lexilink_app/app/theme/app_layout.dart';

class AppScreen extends StatelessWidget {
  const AppScreen({
    required this.child,
    this.size = AppScreenSize.standard,
    this.padding,
    this.scrollable = true,
    super.key,
  });

  final Widget child;
  final AppScreenSize size;
  final EdgeInsetsGeometry? padding;
  final bool scrollable;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: LayoutBuilder(
          builder: (context, constraints) {
            final content = Center(
              child: ConstrainedBox(
                constraints: BoxConstraints(
                  maxWidth: AppLayout.maxWidthForSize(size),
                ),
                child: Padding(
                  padding:
                      padding ??
                      AppSpacing.screenPaddingForWidth(constraints.maxWidth),
                  child: child,
                ),
              ),
            );

            return scrollable ? SingleChildScrollView(child: content) : content;
          },
        ),
      ),
    );
  }
}
