import 'package:flutter/material.dart';
import 'package:lexilink_app/features/admin_audit/presentation/admin_audit_screen.dart';
import 'package:lexilink_app/features/admin_energy/presentation/admin_energy_screen.dart';
import 'package:lexilink_app/features/admin_players/presentation/admin_players_screen.dart';
import 'package:lexilink_app/features/admin_quests/presentation/admin_quests_screen.dart';

/// Thin route-target wrappers consumed by the admin ShellRoute. Each
/// returns the feature's real screen; this layer exists so the router
/// only imports a single file regardless of how many feature folders
/// the destinations live in.
class AdminQuestsPage extends StatelessWidget {
  const AdminQuestsPage({super.key});

  @override
  Widget build(BuildContext context) => const AdminQuestsScreen();
}

class AdminPlayersPage extends StatelessWidget {
  const AdminPlayersPage({super.key});

  @override
  Widget build(BuildContext context) => const AdminPlayersScreen();
}

class AdminEnergyPage extends StatelessWidget {
  const AdminEnergyPage({super.key});

  @override
  Widget build(BuildContext context) => const AdminEnergyScreen();
}

class AdminAuditPage extends StatelessWidget {
  const AdminAuditPage({super.key});

  @override
  Widget build(BuildContext context) => const AdminAuditScreen();
}
