import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

/// Placeholder admin landing screen rendered after a successful
/// /auth/admin/token exchange. Slice F2 replaces this with
/// `AppAdminShell` (side nav + Quest/Player/Energy/Audit pages);
/// for now it just confirms the session and offers sign-out.
class AdminHomeScreen extends StatefulWidget {
  const AdminHomeScreen({super.key, this.tokenStoreFactory});

  final Future<TokenStore> Function()? tokenStoreFactory;

  @override
  State<AdminHomeScreen> createState() => _AdminHomeScreenState();
}

class _AdminHomeScreenState extends State<AdminHomeScreen> {
  TokenStore? _tokenStore;
  bool _checking = true;
  bool _isAuthenticated = false;

  @override
  void initState() {
    super.initState();
    _resolve();
  }

  Future<void> _resolve() async {
    final store = await (widget.tokenStoreFactory?.call() ??
        SharedPreferencesAdminTokenStore.create());
    final token = await store.readAccessToken();
    if (!mounted) return;
    setState(() {
      _tokenStore = store;
      _isAuthenticated = token != null && token.isNotEmpty;
      _checking = false;
    });

    if (!_isAuthenticated && mounted) {
      // Async resolve already guarded by `mounted`; the synchronous-context
      // lint trips here because the await sits across the `mounted` check.
      // ignore: use_build_context_synchronously
      context.go('/admin/login');
    }
  }

  Future<void> _signOut() async {
    await _tokenStore?.clear();
    if (!mounted) return;
    context.go('/admin/login');
  }

  @override
  Widget build(BuildContext context) {
    if (_checking) {
      return const AppScreen(child: Center(child: CircularProgressIndicator()));
    }
    if (!_isAuthenticated) {
      return const AppScreen(child: SizedBox.shrink());
    }

    return AppScreen(
      size: AppScreenSize.compact,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: 32),
          Text(
            'Admin console',
            style: Theme.of(context).textTheme.headlineMedium,
          ),
          const SizedBox(height: 8),
          const Text(
            'Signed in. F2-F6 populate this shell with quest catalog, player '
            'search, energy admin, and audit views.',
          ),
          const SizedBox(height: 24),
          AppDangerButton(label: 'Sign out', onPressed: _signOut),
        ],
      ),
    );
  }
}
