import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Admin console shell. NavigationRail on >= 600 viewports, modal
/// drawer below. Wraps the four admin pages (Quests / Players /
/// Energy / Audit). Sign-out clears the admin token store and goes
/// back to /admin/login; the player session is untouched.
class AppAdminShell extends StatefulWidget {
  const AppAdminShell({
    required this.child,
    required this.location,
    super.key,
    this.tokenStoreFactory,
  });

  final Widget child;
  final String location;

  /// Test hook. Production resolves [SharedPreferencesAdminTokenStore].
  final Future<TokenStore> Function()? tokenStoreFactory;

  @override
  State<AppAdminShell> createState() => _AppAdminShellState();
}

class _AppAdminShellState extends State<AppAdminShell> {
  TokenStore? _tokenStore;

  static const _destinations = <_AdminDestination>[
    _AdminDestination(
      label: 'Quests',
      icon: Icons.assignment_outlined,
      selectedIcon: Icons.assignment,
      route: '/admin/quests',
    ),
    _AdminDestination(
      label: 'Players',
      icon: Icons.person_outline,
      selectedIcon: Icons.person,
      route: '/admin/players',
    ),
    _AdminDestination(
      label: 'Energy',
      icon: Icons.bolt_outlined,
      selectedIcon: Icons.bolt,
      route: '/admin/energy',
    ),
    _AdminDestination(
      label: 'Audit',
      icon: Icons.fact_check_outlined,
      selectedIcon: Icons.fact_check,
      route: '/admin/audit',
    ),
  ];

  @override
  void initState() {
    super.initState();
    _resolveStore();
  }

  Future<void> _resolveStore() async {
    final store = await (widget.tokenStoreFactory?.call() ??
        SharedPreferencesAdminTokenStore.create());
    if (!mounted) return;
    setState(() => _tokenStore = store);
  }

  int get _selectedIndex {
    final i = _destinations.indexWhere(
      (d) => widget.location.startsWith(d.route),
    );
    return i < 0 ? 0 : i;
  }

  void _navigateTo(int index) {
    context.go(_destinations[index].route);
  }

  Future<void> _signOut() async {
    await _tokenStore?.clear();
    if (!mounted) return;
    context.go('/admin/login');
  }

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.sizeOf(context).width;
    final useRail = !AppBreakpoints.isMobile(width);

    return Scaffold(
      appBar: useRail ? null : _buildMobileAppBar(context),
      drawer: useRail ? null : _buildMobileDrawer(context),
      body: useRail ? _buildWideLayout(context) : SafeArea(child: widget.child),
    );
  }

  PreferredSizeWidget _buildMobileAppBar(BuildContext context) {
    final title = _destinations[_selectedIndex].label;
    return AppBar(
      title: Text('Admin · $title'),
      actions: [
        IconButton(
          tooltip: 'Sign out',
          icon: const Icon(Icons.logout),
          onPressed: _signOut,
        ),
      ],
    );
  }

  Widget _buildMobileDrawer(BuildContext context) {
    return NavigationDrawer(
      selectedIndex: _selectedIndex,
      onDestinationSelected: (index) {
        Navigator.of(context).pop();
        _navigateTo(index);
      },
      children: [
        const Padding(
          padding: EdgeInsets.fromLTRB(28, 24, 16, 8),
          child: Text(
            'Admin console',
            style: TextStyle(fontWeight: FontWeight.w600),
          ),
        ),
        for (final d in _destinations)
          NavigationDrawerDestination(
            icon: Icon(d.icon),
            selectedIcon: Icon(d.selectedIcon),
            label: Text(d.label),
          ),
      ],
    );
  }

  Widget _buildWideLayout(BuildContext context) {
    return SafeArea(
      child: Row(
        children: [
          NavigationRail(
            selectedIndex: _selectedIndex,
            onDestinationSelected: _navigateTo,
            labelType: NavigationRailLabelType.all,
            leading: Padding(
              padding: const EdgeInsets.symmetric(vertical: 16),
              child: Column(
                children: [
                  Icon(
                    Icons.shield_moon,
                    color: Theme.of(context).colorScheme.primary,
                  ),
                  const SizedBox(height: 4),
                  const Text('Admin', style: TextStyle(fontSize: 12)),
                ],
              ),
            ),
            trailing: Expanded(
              child: Align(
                alignment: Alignment.bottomCenter,
                child: Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: IconButton(
                    tooltip: 'Sign out',
                    icon: const Icon(Icons.logout),
                    onPressed: _signOut,
                  ),
                ),
              ),
            ),
            destinations: [
              for (final d in _destinations)
                NavigationRailDestination(
                  icon: Icon(d.icon),
                  selectedIcon: Icon(d.selectedIcon),
                  label: Text(d.label),
                ),
            ],
          ),
          const VerticalDivider(width: 1, thickness: 1),
          Expanded(child: widget.child),
        ],
      ),
    );
  }
}

class _AdminDestination {
  const _AdminDestination({
    required this.label,
    required this.icon,
    required this.selectedIcon,
    required this.route,
  });

  final String label;
  final IconData icon;
  final IconData selectedIcon;
  final String route;
}
