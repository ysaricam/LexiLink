import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_content/application/admin_content_cubit.dart';
import 'package:lexilink_app/features/admin_content/data/admin_content_models.dart';
import 'package:lexilink_app/features/admin_content/data/admin_content_repository.dart';
import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class AdminContentScreen extends StatefulWidget {
  const AdminContentScreen({super.key, this.cubitFactory});

  final AdminContentCubit Function()? cubitFactory;

  @override
  State<AdminContentScreen> createState() => _AdminContentScreenState();
}

class _AdminContentScreenState extends State<AdminContentScreen> {
  AdminContentCubit? _cubit;
  http.Client? _client;

  @override
  void initState() {
    super.initState();
    unawaited(_initialize());
  }

  Future<void> _initialize() async {
    final AdminContentCubit cubit;
    if (widget.cubitFactory != null) {
      cubit = widget.cubitFactory!();
    } else {
      final tokenStore = await SharedPreferencesAdminTokenStore.create();
      _client = http.Client();
      cubit = _buildCubit(tokenStore);
    }
    if (!mounted) {
      await cubit.close();
      return;
    }
    setState(() => _cubit = cubit..load());
  }

  AdminContentCubit _buildCubit(TokenStore tokenStore) {
    return AdminContentCubit(
      repository: AdminContentRepository(
        apiClient: ApiClient(
          config: ApiConfig.local(),
          httpClient: _client!,
          tokenStore: tokenStore,
        ),
      ),
    );
  }

  @override
  void dispose() {
    _cubit?.close();
    _client?.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final cubit = _cubit;
    if (cubit == null) {
      return const Center(child: CircularProgressIndicator());
    }

    return BlocProvider.value(
      value: cubit,
      child: const _AdminContentView(),
    );
  }
}

class _AdminContentView extends StatelessWidget {
  const _AdminContentView();

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<AdminContentCubit, AdminContentState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminContentStatus.failure,
      listener: (context, state) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(state.errorMessage!)),
        );
      },
      builder: (context, state) {
        final busy =
            state.status == AdminContentStatus.loading ||
            state.status == AdminContentStatus.saving;

        return SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 32),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 980),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  context.l10n.adminContentConsoleTitle,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 16),
                Wrap(
                  spacing: 12,
                  runSpacing: 12,
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [
                    SizedBox(
                      width: 260,
                      child: _LocaleFilterDropdown(
                        localeFilter: state.localeFilter,
                        busy: busy,
                      ),
                    ),
                    FilledButton.icon(
                      onPressed: busy
                          ? null
                          : () => _openCategoryDialog(context),
                      icon: const Icon(Icons.add),
                      label: Text(context.l10n.adminContentNewCategory),
                    ),
                  ],
                ),
                const SizedBox(height: 20),
                if (busy) const LinearProgressIndicator(),
                if (busy) const SizedBox(height: 16),
                for (final category in state.categories)
                  Card(
                    child: ListTile(
                      leading: const Icon(Icons.category_outlined),
                      title: Text(category.name),
                      subtitle: Text(category.language),
                      trailing: IconButton(
                        tooltip: context.l10n.commonEdit,
                        icon: const Icon(Icons.edit_outlined),
                        onPressed: busy
                            ? null
                            : () => _openCategoryDialog(
                                context,
                                category: category,
                              ),
                      ),
                    ),
                  ),
                if (state.categories.isEmpty &&
                    state.status != AdminContentStatus.loading)
                  Padding(
                    padding: const EdgeInsets.all(16),
                    child: Text(context.l10n.adminContentNoCategories),
                  ),
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> _openCategoryDialog(
    BuildContext context, {
    AdminContentCategory? category,
  }) async {
    final cubit = context.read<AdminContentCubit>();
    AdminContentCategoryDetail? detail;
    if (category != null) {
      detail = await cubit.fetchCategory(category.id);
      if (detail == null || !context.mounted) return;
    }

    if (!context.mounted) return;
    await showDialog<void>(
      context: context,
      builder: (_) => BlocProvider.value(
        value: cubit,
        child: _CategoryDialog(category: detail),
      ),
    );
  }
}

class _LocaleFilterDropdown extends StatelessWidget {
  const _LocaleFilterDropdown({
    required this.localeFilter,
    required this.busy,
  });

  static const _all = '';

  final String? localeFilter;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    return DropdownButtonFormField<String>(
      initialValue: localeFilter ?? _all,
      decoration: InputDecoration(
        border: const OutlineInputBorder(),
        isDense: true,
        labelText: context.l10n.adminContentLanguageFilter,
      ),
      items: [
        DropdownMenuItem(
          value: _all,
          child: Text(context.l10n.adminContentAllLanguages),
        ),
        for (final language in AppLanguage.values)
          DropdownMenuItem(
            value: language.backendLocale,
            child: Text(language.nativeName),
          ),
      ],
      onChanged: busy
          ? null
          : (value) => context.read<AdminContentCubit>().changeLocaleFilter(
              value == _all ? null : value,
            ),
    );
  }
}

class _CategoryDialog extends StatefulWidget {
  const _CategoryDialog({this.category});

  final AdminContentCategoryDetail? category;

  @override
  State<_CategoryDialog> createState() => _CategoryDialogState();
}

class _CategoryDialogState extends State<_CategoryDialog> {
  late final TextEditingController _nameController;
  late final TextEditingController _descriptionController;
  late String _language;

  @override
  void initState() {
    super.initState();
    final category = widget.category;
    _nameController = TextEditingController(text: category?.name ?? '');
    _descriptionController = TextEditingController(
      text: category?.description ?? '',
    );
    _language = category?.language ?? AppLanguage.fallback.backendLocale;
  }

  @override
  void dispose() {
    _descriptionController.dispose();
    _nameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final category = widget.category;

    return AlertDialog(
      title: Text(
        category == null
            ? context.l10n.adminContentNewCategory
            : context.l10n.adminContentEditCategory,
      ),
      content: SizedBox(
        width: 420,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: _nameController,
                decoration: InputDecoration(labelText: context.l10n.adminName),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: _descriptionController,
                decoration: InputDecoration(
                  labelText: context.l10n.adminDescription,
                ),
                maxLines: 3,
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<String>(
                initialValue: _language,
                decoration: InputDecoration(
                  border: const OutlineInputBorder(),
                  isDense: true,
                  labelText: context.l10n.adminContentLanguage,
                ),
                items: [
                  for (final language in AppLanguage.values)
                    DropdownMenuItem(
                      value: language.backendLocale,
                      child: Text(language.nativeName),
                    ),
                ],
                onChanged: (value) {
                  if (value == null) return;
                  setState(() => _language = value);
                },
              ),
              if (category != null) ...[
                const SizedBox(height: 12),
                Align(
                  alignment: Alignment.centerLeft,
                  child: Text(
                    context.l10n.adminContentLinkCount(category.linkCount),
                    style: Theme.of(context).textTheme.bodySmall,
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(context.l10n.commonCancel),
        ),
        FilledButton(
          onPressed: _submit,
          child: Text(context.l10n.commonSave),
        ),
      ],
    );
  }

  void _submit() {
    final name = _nameController.text.trim();
    final description = _descriptionController.text.trim();
    if (name.isEmpty || description.isEmpty) return;

    context.read<AdminContentCubit>().saveCategory(
      id: widget.category?.id,
      name: name,
      description: description,
      language: _language,
    );
    Navigator.of(context).pop();
  }
}
