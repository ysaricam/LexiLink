import 'package:flutter/material.dart';
import 'package:lexilink_app/app/router/app_router.dart';
import 'package:lexilink_app/app/theme/app_theme.dart';

class LexiLinkApp extends StatelessWidget {
  const LexiLinkApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'LexiLink',
      theme: AppTheme.light,
      darkTheme: AppTheme.dark,
      routerConfig: appRouter,
    );
  }
}
