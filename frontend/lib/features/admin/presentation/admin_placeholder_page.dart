import 'package:flutter/material.dart';

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
  Widget build(BuildContext context) {
    return const AdminPlaceholderPage(
      title: 'Quests',
      slice: 'F3',
      bulletPoints: [
        'List quest definitions (active + deactivated).',
        'Create a new quest definition.',
        'Edit goal / reward / prerequisite.',
        'Deactivate quest definition.',
      ],
    );
  }
}

class AdminPlayersPage extends StatelessWidget {
  const AdminPlayersPage({super.key});

  @override
  Widget build(BuildContext context) {
    return const AdminPlaceholderPage(
      title: 'Players',
      slice: 'F4',
      bulletPoints: [
        'Search players by handle or email.',
        'View player admin detail (handle, providers, ban state).',
        'Ban / unban with reason capture.',
      ],
    );
  }
}

class AdminEnergyPage extends StatelessWidget {
  const AdminEnergyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return const AdminPlaceholderPage(
      title: 'Energy',
      slice: 'F5',
      bulletPoints: [
        'Snap a player energy to a specific value.',
        'Grant bonus energy (over-max allowed).',
        'Reset to full.',
      ],
    );
  }
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
