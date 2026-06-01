import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Admin console shell. NavigationRail on >= 600 viewports, modal
/// drawer below. Wraps admin module pages. Sign-out clears the admin token
/// store and goes back to /admin/login; the player session is untouched.
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
      labelKey: _AdminLabelKey.quests,
      icon: Icons.assignment_outlined,
      selectedIcon: Icons.assignment,
      route: '/admin/quests',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.players,
      icon: Icons.person_outline,
      selectedIcon: Icons.person,
      route: '/admin/players',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.energy,
      icon: Icons.bolt_outlined,
      selectedIcon: Icons.bolt,
      route: '/admin/energy',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.hint,
      icon: Icons.lightbulb_outline,
      selectedIcon: Icons.lightbulb,
      route: '/admin/hint',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.undo,
      icon: Icons.undo,
      selectedIcon: Icons.undo,
      route: '/admin/undo',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.reset,
      icon: Icons.restart_alt,
      selectedIcon: Icons.restart_alt,
      route: '/admin/reset',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.diamond,
      icon: Icons.diamond_outlined,
      selectedIcon: Icons.diamond,
      route: '/admin/diamond',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.market,
      icon: Icons.storefront_outlined,
      selectedIcon: Icons.storefront,
      route: '/admin/market',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.content,
      icon: Icons.category_outlined,
      selectedIcon: Icons.category,
      route: '/admin/content',
    ),
    _AdminDestination(
      labelKey: _AdminLabelKey.audit,
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
    final store =
        await (widget.tokenStoreFactory?.call() ??
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
    final title = _destinations[_selectedIndex].label(context);
    return AppBar(
      title: Text(context.l10n.adminMobileTitle(title)),
      actions: [
        IconButton(
          tooltip: context.l10n.adminSignOut,
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
        Padding(
          padding: const EdgeInsets.fromLTRB(28, 24, 16, 8),
          child: Text(
            context.l10n.adminConsole,
            style: const TextStyle(fontWeight: FontWeight.w600),
          ),
        ),
        for (final d in _destinations)
          NavigationDrawerDestination(
            icon: Icon(d.icon),
            selectedIcon: Icon(d.selectedIcon),
            label: Text(d.label(context)),
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
                  Text(
                    context.l10n.adminLabel,
                    style: const TextStyle(fontSize: 12),
                  ),
                ],
              ),
            ),
            trailing: Expanded(
              child: Align(
                alignment: Alignment.bottomCenter,
                child: Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: IconButton(
                    tooltip: context.l10n.adminSignOut,
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
                  label: Text(d.label(context)),
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
    required this.labelKey,
    required this.icon,
    required this.selectedIcon,
    required this.route,
  });

  final _AdminLabelKey labelKey;
  final IconData icon;
  final IconData selectedIcon;
  final String route;

  String label(BuildContext context) => switch (labelKey) {
    _AdminLabelKey.quests => context.l10n.adminNavQuests,
    _AdminLabelKey.players => context.l10n.adminNavPlayers,
    _AdminLabelKey.energy => context.l10n.adminNavEnergy,
    _AdminLabelKey.hint => context.l10n.adminNavHint,
    _AdminLabelKey.undo => context.l10n.adminNavUndo,
    _AdminLabelKey.reset => context.l10n.adminNavReset,
    _AdminLabelKey.diamond => context.l10n.adminNavDiamond,
    _AdminLabelKey.market => context.l10n.adminNavMarket,
    _AdminLabelKey.content => context.l10n.adminNavContent,
    _AdminLabelKey.audit => context.l10n.adminNavAudit,
  };
}

enum _AdminLabelKey {
  quests,
  players,
  energy,
  hint,
  undo,
  reset,
  diamond,
  market,
  content,
  audit,
}
