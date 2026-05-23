import 'package:flutter/material.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_definition.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';

/// Result of [QuestDefinitionFormDialog]. For Create [questType] and
/// [cadence] are required; for Edit they're carried through unchanged
/// but the dialog disables those fields (server doesn't allow them to
/// change once a definition exists).
class QuestDefinitionFormResult {
  const QuestDefinitionFormResult({
    required this.questType,
    required this.cadence,
    required this.goal,
    required this.rewardAmount,
    this.prerequisiteQuestType,
  });

  final AdminQuestType? questType;
  final AdminQuestCadence? cadence;
  final int goal;
  final int rewardAmount;
  final AdminQuestType? prerequisiteQuestType;
}

class QuestDefinitionFormDialog extends StatefulWidget {
  const QuestDefinitionFormDialog({
    super.key,
    this.initial,
    this.takenTypes = const {},
  });

  /// When non-null the dialog is in Edit mode — type/cadence fields
  /// are read-only.
  final QuestDefinition? initial;

  /// Quest types that already have a definition in the catalog. In
  /// create mode these are removed from the type dropdown so the
  /// admin can't trigger the server's "already exists" 400.
  final Set<AdminQuestType> takenTypes;

  @override
  State<QuestDefinitionFormDialog> createState() =>
      _QuestDefinitionFormDialogState();
}

class _QuestDefinitionFormDialogState extends State<QuestDefinitionFormDialog> {
  late final TextEditingController _goalController;
  late final TextEditingController _rewardController;
  final _formKey = GlobalKey<FormState>();

  AdminQuestType? _questType;
  AdminQuestCadence? _cadence;
  AdminQuestType? _prerequisite;

  bool get _isEdit => widget.initial != null;

  @override
  void initState() {
    super.initState();
    final initial = widget.initial;
    _goalController =
        TextEditingController(text: initial?.goal.toString() ?? '');
    _rewardController =
        TextEditingController(text: initial?.rewardAmount.toString() ?? '');
    _questType = initial?.questType;
    _cadence = initial?.cadence;
    _prerequisite = initial?.prerequisiteQuestType;
  }

  @override
  void dispose() {
    _goalController.dispose();
    _rewardController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(_isEdit ? 'Edit quest definition' : 'New quest definition'),
      content: Form(
        key: _formKey,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              DropdownButtonFormField<AdminQuestType>(
                initialValue: _questType,
                decoration: const InputDecoration(
                  labelText: 'Quest type',
                  border: OutlineInputBorder(),
                ),
                items: [
                  for (final t in AdminQuestType.values)
                    DropdownMenuItem(
                      value: t,
                      enabled:
                          _isEdit || !widget.takenTypes.contains(t),
                      child: Text(
                        widget.takenTypes.contains(t) && !_isEdit
                            ? '${t.wire}  (exists)'
                            : t.wire,
                      ),
                    ),
                ],
                onChanged: _isEdit
                    ? null
                    : (v) => setState(() => _questType = v),
                validator: (v) =>
                    v == null ? 'Quest type is required' : null,
              ),
              if (!_isEdit &&
                  widget.takenTypes.length ==
                      AdminQuestType.values.length) ...[
                const SizedBox(height: 8),
                Text(
                  'All quest types already have a definition. Edit or '
                  'deactivate an existing one instead.',
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: Theme.of(context).colorScheme.error,
                      ),
                ),
              ],
              const SizedBox(height: 12),
              DropdownButtonFormField<AdminQuestCadence>(
                initialValue: _cadence,
                decoration: const InputDecoration(
                  labelText: 'Cadence',
                  border: OutlineInputBorder(),
                ),
                items: [
                  for (final c in AdminQuestCadence.values)
                    DropdownMenuItem(value: c, child: Text(c.wire)),
                ],
                onChanged: _isEdit
                    ? null
                    : (v) => setState(() => _cadence = v),
                validator: (v) =>
                    v == null ? 'Cadence is required' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _goalController,
                decoration: const InputDecoration(
                  labelText: 'Goal',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
                validator: _validatePositive,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _rewardController,
                decoration: const InputDecoration(
                  labelText: 'Reward amount',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
                validator: _validatePositive,
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<AdminQuestType?>(
                initialValue: _prerequisite,
                decoration: const InputDecoration(
                  labelText: 'Prerequisite (optional)',
                  border: OutlineInputBorder(),
                ),
                items: [
                  const DropdownMenuItem<AdminQuestType?>(
                    child: Text('— none —'),
                  ),
                  for (final t in AdminQuestType.values)
                    DropdownMenuItem<AdminQuestType?>(
                      value: t,
                      child: Text(t.wire),
                    ),
                ],
                onChanged: (v) => setState(() => _prerequisite = v),
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          onPressed: _submit,
          child: Text(_isEdit ? 'Save' : 'Create'),
        ),
      ],
    );
  }

  String? _validatePositive(String? v) {
    if (v == null || v.trim().isEmpty) return 'Required';
    final parsed = int.tryParse(v.trim());
    if (parsed == null) return 'Must be a number';
    if (parsed <= 0) return 'Must be greater than 0';
    return null;
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) return;
    Navigator.of(context).pop(
      QuestDefinitionFormResult(
        questType: _questType,
        cadence: _cadence,
        goal: int.parse(_goalController.text.trim()),
        rewardAmount: int.parse(_rewardController.text.trim()),
        prerequisiteQuestType: _prerequisite,
      ),
    );
  }
}
