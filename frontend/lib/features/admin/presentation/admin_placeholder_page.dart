import 'package:flutter/material.dart';
import 'package:lexilink_app/features/admin_energy/presentation/admin_energy_screen.dart';
import 'package:lexilink_app/features/admin_players/presentation/admin_players_screen.dart';
import 'package:lexilink_app/features/admin_quests/presentation/admin_quests_screen.dart';

/// Shared placeholder rendered by each admin destination until its
/// real implementation lands in F3-F6.
class AdminPlaceholderPage extends StatelessWidget {
  const AdminPlaceholderPage({
    required this.title,
    required this.slice,
    required this.bulletPoints,
    super.key,
  });

  final String title;
  final String slice;
  final List<String> bulletPoints;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title, style: Theme.of(context).textTheme.headlineMedium),
          const SizedBox(height: 4),
          Text(
            'Coming in $slice',
            style: Theme.of(context).textTheme.titleSmall,
          ),
          const SizedBox(height: 16),
          for (final point in bulletPoints) ...[
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('• '),
                Expanded(child: Text(point)),
              ],
            ),
            const SizedBox(height: 4),
          ],
        ],
      ),
    );
  }
}

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
  Widget build(BuildContext context) {
    return const AdminPlaceholderPage(
      title: 'Audit',
      slice: 'F6',
      bulletPoints: [
        'Paged audit log, newest first.',
        'Filter by admin, target type, target id.',
        'Inspect raw payload JSON.',
      ],
    );
  }
}
