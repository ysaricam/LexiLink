import 'package:flutter/material.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_definition.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';

/// Result of [QuestDefinitionFormDialog]. For Create the dialog
/// supplies name + trigger; for Edit those fields are read-only
/// (server rejects changes to name/trigger after Create — see Q1.5).
/// Sprint H splits the reward into energy + hint; at least one must
/// be positive (the form enforces this client-side via a non-field
/// check before submit).
class QuestDefinitionFormResult {
  const QuestDefinitionFormResult({
    required this.name,
    required this.description,
    required this.trigger,
    required this.threshold,
    required this.energyReward,
    required this.hintReward,
    required this.undoReward,
    required this.resetReward,
    required this.progressBaseline,
    this.prerequisiteQuestDefinitionId,
  });

  final String name;
  final String description;
  final QuestTrigger trigger;
  final int threshold;
  final int energyReward;
  final int hintReward;
  final int undoReward;
  final int resetReward;
  final ProgressBaseline progressBaseline;
  final String? prerequisiteQuestDefinitionId;
}

class QuestDefinitionFormDialog extends StatefulWidget {
  const QuestDefinitionFormDialog({
    super.key,
    this.initial,
    this.availablePrerequisites = const [],
  });

  /// When non-null the dialog is in Edit mode — name + trigger are
  /// read-only.
  final QuestDefinition? initial;

  /// Other active definitions admin can pick as a prerequisite. In
  /// Edit mode the current definition is filtered out so the user
  /// can't pick it as its own prereq.
  final List<QuestDefinition> availablePrerequisites;

  @override
  State<QuestDefinitionFormDialog> createState() =>
      _QuestDefinitionFormDialogState();
}

class _QuestDefinitionFormDialogState extends State<QuestDefinitionFormDialog> {
  late final TextEditingController _nameController;
  late final TextEditingController _descriptionController;
  late final TextEditingController _thresholdController;
  late final TextEditingController _energyRewardController;
  late final TextEditingController _hintRewardController;
  late final TextEditingController _undoRewardController;
  late final TextEditingController _resetRewardController;
  final _formKey = GlobalKey<FormState>();

  QuestTrigger? _trigger;
  ProgressBaseline _progressBaseline = ProgressBaseline.fromSnapshot;
  String? _prerequisiteId;
  String? _rewardSumError;

  bool get _isEdit => widget.initial != null;

  @override
  void initState() {
    super.initState();
    final initial = widget.initial;
    _nameController = TextEditingController(text: initial?.name ?? '');
    _descriptionController =
        TextEditingController(text: initial?.description ?? '');
    _thresholdController =
        TextEditingController(text: initial?.threshold.toString() ?? '');
    _energyRewardController =
        TextEditingController(text: initial?.energyReward.toString() ?? '0');
    _hintRewardController =
        TextEditingController(text: initial?.hintReward.toString() ?? '0');
    _undoRewardController =
        TextEditingController(text: initial?.undoReward.toString() ?? '0');
    _resetRewardController =
        TextEditingController(text: initial?.resetReward.toString() ?? '0');
    _trigger = initial?.trigger;
    _progressBaseline = initial?.progressBaseline ?? ProgressBaseline.fromSnapshot;
    _prerequisiteId = initial?.prerequisiteQuestDefinitionId;
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    _thresholdController.dispose();
    _energyRewardController.dispose();
    _hintRewardController.dispose();
    _undoRewardController.dispose();
    _resetRewardController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final prereqs = _isEdit
        ? widget.availablePrerequisites
            .where((d) => d.id != widget.initial!.id)
            .toList(growable: false)
        : widget.availablePrerequisites;
    final showProgressBaseline = _trigger == QuestTrigger.gameCompletedTotal;

    return AlertDialog(
      title: Text(_isEdit ? 'Quest tanımını düzenle' : 'Yeni quest tanımı'),
      content: Form(
        key: _formKey,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextFormField(
                controller: _nameController,
                readOnly: _isEdit,
                maxLength: 64,
                decoration: InputDecoration(
                  labelText: 'İsim',
                  border: const OutlineInputBorder(),
                  helperText: _isEdit
                      ? 'Oluşturulduktan sonra değiştirilemez.'
                      : null,
                ),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) {
                    return 'İsim zorunlu';
                  }
                  if (v.length > 64) {
                    return 'En fazla 64 karakter';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _descriptionController,
                maxLength: 256,
                minLines: 2,
                maxLines: 4,
                decoration: const InputDecoration(
                  labelText: 'Açıklama',
                  border: OutlineInputBorder(),
                ),
                validator: (v) {
                  if (v != null && v.length > 256) {
                    return 'En fazla 256 karakter';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<QuestTrigger>(
                initialValue: _trigger,
                decoration: InputDecoration(
                  labelText: 'Tetikleyici',
                  border: const OutlineInputBorder(),
                  helperText: _isEdit
                      ? 'Oluşturulduktan sonra değiştirilemez.'
                      : null,
                ),
                items: [
                  for (final t in QuestTrigger.values)
                    DropdownMenuItem(value: t, child: Text(t.displayLabel)),
                ],
                onChanged: _isEdit
                    ? null
                    : (v) => setState(() => _trigger = v),
                validator: (v) =>
                    v == null ? 'Tetikleyici zorunlu' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _thresholdController,
                decoration: const InputDecoration(
                  labelText: 'Eşik (threshold)',
                  border: OutlineInputBorder(),
                ),
                keyboardType: TextInputType.number,
                validator: _validatePositive,
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      controller: _energyRewardController,
                      decoration: const InputDecoration(
                        labelText: 'Enerji ödülü ⚡',
                        border: OutlineInputBorder(),
                      ),
                      keyboardType: TextInputType.number,
                      validator: _validateNonNegative,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: TextFormField(
                      controller: _hintRewardController,
                      decoration: const InputDecoration(
                        labelText: 'İpucu ödülü 💡',
                        border: OutlineInputBorder(),
                      ),
                      keyboardType: TextInputType.number,
                      validator: _validateNonNegative,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      controller: _undoRewardController,
                      decoration: const InputDecoration(
                        labelText: 'Geri al ödülü ↶',
                        border: OutlineInputBorder(),
                      ),
                      keyboardType: TextInputType.number,
                      validator: _validateNonNegative,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: TextFormField(
                      controller: _resetRewardController,
                      decoration: const InputDecoration(
                        labelText: 'Sıfırla ödülü ↻',
                        border: OutlineInputBorder(),
                      ),
                      keyboardType: TextInputType.number,
                      validator: _validateNonNegative,
                    ),
                  ),
                ],
              ),
              if (_rewardSumError != null) ...[
                const SizedBox(height: 4),
                Text(
                  _rewardSumError!,
                  style: TextStyle(
                    color: Theme.of(context).colorScheme.error,
                    fontSize: 12,
                  ),
                ),
              ],
              if (showProgressBaseline) ...[
                const SizedBox(height: 12),
                DropdownButtonFormField<ProgressBaseline>(
                  initialValue: _progressBaseline,
                  decoration: const InputDecoration(
                    labelText: 'İlerleme başlangıcı',
                    border: OutlineInputBorder(),
                    helperText:
                        'Yalnız "Toplam oyun" için anlamlıdır.',
                  ),
                  items: [
                    for (final b in ProgressBaseline.values)
                      DropdownMenuItem(value: b, child: Text(b.displayLabel)),
                  ],
                  onChanged: (v) => setState(() {
                    if (v != null) _progressBaseline = v;
                  }),
                ),
              ],
              const SizedBox(height: 12),
              DropdownButtonFormField<String?>(
                initialValue: _prerequisiteId,
                decoration: const InputDecoration(
                  labelText: 'Ön koşul (opsiyonel)',
                  border: OutlineInputBorder(),
                ),
                items: [
                  const DropdownMenuItem<String?>(
                    child: Text('— yok —'),
                  ),
                  for (final p in prereqs)
                    DropdownMenuItem<String?>(
                      value: p.id,
                      child: Text(p.name),
                    ),
                ],
                onChanged: (v) => setState(() => _prerequisiteId = v),
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('İptal'),
        ),
        FilledButton(
          onPressed: _submit,
          child: Text(_isEdit ? 'Kaydet' : 'Oluştur'),
        ),
      ],
    );
  }

  String? _validatePositive(String? v) {
    if (v == null || v.trim().isEmpty) return 'Zorunlu';
    final parsed = int.tryParse(v.trim());
    if (parsed == null) return 'Sayı olmalı';
    if (parsed <= 0) return '0\'dan büyük olmalı';
    return null;
  }

  String? _validateNonNegative(String? v) {
    if (v == null || v.trim().isEmpty) return 'Zorunlu';
    final parsed = int.tryParse(v.trim());
    if (parsed == null) return 'Sayı olmalı';
    if (parsed < 0) return '0\'dan küçük olamaz';
    return null;
  }

  void _submit() {
    final formValid = _formKey.currentState!.validate();
    final energy = int.tryParse(_energyRewardController.text.trim()) ?? -1;
    final hint = int.tryParse(_hintRewardController.text.trim()) ?? -1;
    final undo = int.tryParse(_undoRewardController.text.trim()) ?? -1;
    final reset = int.tryParse(_resetRewardController.text.trim()) ?? -1;
    final atLeastOnePositive =
        energy > 0 || hint > 0 || undo > 0 || reset > 0;
    setState(() {
      _rewardSumError = atLeastOnePositive
          ? null
          : 'En az bir ödül (enerji, ipucu, geri al, sıfırla) > 0 olmalı.';
    });
    if (!formValid || !atLeastOnePositive) return;
    Navigator.of(context).pop(
      QuestDefinitionFormResult(
        name: _nameController.text.trim(),
        description: _descriptionController.text.trim(),
        trigger: _trigger!,
        threshold: int.parse(_thresholdController.text.trim()),
        energyReward: energy,
        hintReward: hint,
        undoReward: undo,
        resetReward: reset,
        progressBaseline: _progressBaseline,
        prerequisiteQuestDefinitionId: _prerequisiteId,
      ),
    );
  }
}
