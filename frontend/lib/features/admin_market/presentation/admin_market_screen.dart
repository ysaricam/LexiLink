import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_market/application/admin_market_cubit.dart';
import 'package:lexilink_app/features/admin_market/data/admin_market_repository.dart';
import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum AdminMarketTab { categories, items, orders }

enum _ItemFormMode { normal, promotion }

class AdminMarketScreen extends StatefulWidget {
  const AdminMarketScreen({
    super.key,
    this.initialTab = AdminMarketTab.categories,
    this.initialPlayerId,
    this.cubitFactory,
  });

  final AdminMarketTab initialTab;
  final String? initialPlayerId;
  final AdminMarketCubit Function()? cubitFactory;

  @override
  State<AdminMarketScreen> createState() => _AdminMarketScreenState();
}

class _AdminMarketScreenState extends State<AdminMarketScreen> {
  AdminMarketCubit? _cubit;
  http.Client? _client;

  @override
  void initState() {
    super.initState();
    unawaited(_initialize());
  }

  Future<void> _initialize() async {
    final AdminMarketCubit cubit;
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
    final initialPlayerId = widget.initialPlayerId;
    if (initialPlayerId != null && initialPlayerId.isNotEmpty) {
      unawaited(cubit.loadOrders(initialPlayerId));
    }
  }

  AdminMarketCubit _buildCubit(TokenStore tokenStore) {
    return AdminMarketCubit(
      repository: AdminMarketRepository(
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
      child: _AdminMarketView(
        initialTab: widget.initialTab,
        initialPlayerId: widget.initialPlayerId,
      ),
    );
  }
}

class _AdminMarketView extends StatefulWidget {
  const _AdminMarketView({
    required this.initialTab,
    required this.initialPlayerId,
  });

  final AdminMarketTab initialTab;
  final String? initialPlayerId;

  @override
  State<_AdminMarketView> createState() => _AdminMarketViewState();
}

class _AdminMarketViewState extends State<_AdminMarketView> {
  late AdminMarketTab _tab;
  late final TextEditingController _playerIdController;

  @override
  void initState() {
    super.initState();
    _tab = widget.initialTab;
    _playerIdController = TextEditingController(
      text: widget.initialPlayerId ?? '',
    );
  }

  @override
  void dispose() {
    _playerIdController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<AdminMarketCubit, AdminMarketState>(
      listenWhen: (prev, curr) =>
          prev.errorMessage != curr.errorMessage &&
          curr.errorMessage != null &&
          curr.status == AdminMarketStatus.failure,
      listener: (context, state) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(state.errorMessage!)),
        );
      },
      builder: (context, state) {
        final busy =
            state.status == AdminMarketStatus.loading ||
            state.status == AdminMarketStatus.saving;

        return SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 32),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 980),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Market console',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  'Manage shop categories, diamond-priced items, and '
                  'player purchase history.',
                  style: Theme.of(context).textTheme.bodySmall,
                ),
                const SizedBox(height: 16),
                SegmentedButton<AdminMarketTab>(
                  segments: const [
                    ButtonSegment(
                      value: AdminMarketTab.categories,
                      label: Text('Categories'),
                      icon: Icon(Icons.category_outlined),
                    ),
                    ButtonSegment(
                      value: AdminMarketTab.items,
                      label: Text('Items'),
                      icon: Icon(Icons.storefront_outlined),
                    ),
                    ButtonSegment(
                      value: AdminMarketTab.orders,
                      label: Text('Orders'),
                      icon: Icon(Icons.receipt_long_outlined),
                    ),
                  ],
                  selected: {_tab},
                  onSelectionChanged: busy
                      ? null
                      : (value) => setState(() => _tab = value.single),
                ),
                const SizedBox(height: 20),
                if (busy) const LinearProgressIndicator(),
                if (busy) const SizedBox(height: 16),
                switch (_tab) {
                  AdminMarketTab.categories => _CategoriesPanel(state: state),
                  AdminMarketTab.items => _ItemsPanel(state: state),
                  AdminMarketTab.orders => _OrdersPanel(
                    state: state,
                    playerIdController: _playerIdController,
                  ),
                },
              ],
            ),
          ),
        );
      },
    );
  }
}

class _CategoriesPanel extends StatelessWidget {
  const _CategoriesPanel({required this.state});

  final AdminMarketState state;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        FilledButton.icon(
          onPressed: () => _openCategoryDialog(context),
          icon: const Icon(Icons.add),
          label: const Text('New category'),
        ),
        const SizedBox(height: 12),
        for (final category in state.categories)
          Card(
            child: ListTile(
              leading: Text(
                (category.icon?.isNotEmpty ?? false) ? category.icon! : '#',
                style: const TextStyle(fontSize: 24),
              ),
              title: Text(category.name),
              subtitle: Text(
                'Sort ${category.sortOrder} - '
                '${category.isActive ? 'Active' : 'Inactive'}',
              ),
              trailing: Wrap(
                spacing: 8,
                children: [
                  TextButton(
                    onPressed: () => _openCategoryDialog(
                      context,
                      category: category,
                    ),
                    child: const Text('Edit'),
                  ),
                  TextButton(
                    onPressed: category.isActive
                        ? () => context
                              .read<AdminMarketCubit>()
                              .deactivateCategory(category.id)
                        : null,
                    child: const Text('Deactivate'),
                  ),
                ],
              ),
            ),
          ),
        if (state.categories.isEmpty)
          const Padding(
            padding: EdgeInsets.all(16),
            child: Text('No market categories yet.'),
          ),
      ],
    );
  }

  Future<void> _openCategoryDialog(
    BuildContext context, {
    MarketCategory? category,
  }) async {
    final cubit = context.read<AdminMarketCubit>();
    await showDialog<void>(
      context: context,
      builder: (_) => BlocProvider.value(
        value: cubit,
        child: _CategoryDialog(category: category),
      ),
    );
  }
}

class _ItemsPanel extends StatelessWidget {
  const _ItemsPanel({required this.state});

  final AdminMarketState state;

  @override
  Widget build(BuildContext context) {
    final categoryNames = {
      for (final category in state.categories) category.id: category.name,
    };
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        FilledButton.icon(
          onPressed: state.categories.isEmpty
              ? null
              : () => _openItemDialog(context),
          icon: const Icon(Icons.add),
          label: const Text('New item'),
        ),
        const SizedBox(height: 12),
        for (final item in state.items)
          Card(
            child: ListTile(
              leading: Text(
                item.itemType.symbol,
                style: const TextStyle(fontSize: 26),
              ),
              title: Text('${item.quantity} ${item.itemType.wire}'),
              subtitle: Text(
                '${categoryNames[item.categoryId] ?? item.categoryId} - '
                '${item.effectivePrice} diamonds - '
                'stock ${item.remainingStock?.toString() ?? 'unlimited'}',
              ),
              trailing: Wrap(
                spacing: 8,
                children: [
                  TextButton(
                    onPressed: () => _openItemDialog(context, item: item),
                    child: const Text('Edit'),
                  ),
                  TextButton(
                    onPressed: item.isActive
                        ? () => context.read<AdminMarketCubit>().deactivateItem(
                            item.id,
                          )
                        : null,
                    child: const Text('Deactivate'),
                  ),
                ],
              ),
            ),
          ),
        if (state.items.isEmpty)
          const Padding(
            padding: EdgeInsets.all(16),
            child: Text('No market items yet.'),
          ),
      ],
    );
  }

  Future<void> _openItemDialog(BuildContext context, {MarketItem? item}) async {
    final cubit = context.read<AdminMarketCubit>();
    await showDialog<void>(
      context: context,
      builder: (_) => BlocProvider.value(
        value: cubit,
        child: _ItemDialog(categories: state.categories, item: item),
      ),
    );
  }
}

class _OrdersPanel extends StatelessWidget {
  const _OrdersPanel({
    required this.state,
    required this.playerIdController,
  });

  final AdminMarketState state;
  final TextEditingController playerIdController;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: playerIdController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  isDense: true,
                  labelText: 'Player GUID',
                ),
                onSubmitted: (_) => _load(context),
              ),
            ),
            const SizedBox(width: 12),
            FilledButton.icon(
              onPressed: () => _load(context),
              icon: const Icon(Icons.search),
              label: const Text('Load'),
            ),
          ],
        ),
        const SizedBox(height: 12),
        for (final order in state.orders)
          Card(
            child: ListTile(
              leading: const Icon(Icons.receipt_long_outlined),
              title: Text('${order.quantity} ${order.itemType.wire}'),
              subtitle: Text(
                '${order.diamondsPaid} diamonds - ${order.purchasedAt}',
              ),
            ),
          ),
        if (state.orderPlayerId != null && state.orders.isEmpty)
          const Padding(
            padding: EdgeInsets.all(16),
            child: Text('No market orders for this player.'),
          ),
      ],
    );
  }

  void _load(BuildContext context) {
    final playerId = playerIdController.text.trim();
    if (playerId.isEmpty) return;
    context.read<AdminMarketCubit>().loadOrders(playerId);
  }
}

class _CategoryDialog extends StatefulWidget {
  const _CategoryDialog({this.category});

  final MarketCategory? category;

  @override
  State<_CategoryDialog> createState() => _CategoryDialogState();
}

class _CategoryDialogState extends State<_CategoryDialog> {
  late final TextEditingController _nameController;
  late final TextEditingController _sortController;
  late final TextEditingController _iconController;
  late final TextEditingController _visibilityStartsController;
  late final TextEditingController _visibilityEndsController;

  @override
  void initState() {
    super.initState();
    final category = widget.category;
    _nameController = TextEditingController(text: category?.name ?? '');
    _sortController = TextEditingController(
      text: (category?.sortOrder ?? 0).toString(),
    );
    _iconController = TextEditingController(text: category?.icon ?? '');
    _visibilityStartsController = TextEditingController(
      text: _dateText(category?.visibilityStartsAt),
    );
    _visibilityEndsController = TextEditingController(
      text: _dateText(category?.visibilityEndsAt),
    );
  }

  @override
  void dispose() {
    _visibilityEndsController.dispose();
    _visibilityStartsController.dispose();
    _iconController.dispose();
    _sortController.dispose();
    _nameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.category == null ? 'New category' : 'Edit category'),
      content: SizedBox(
        width: 420,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: _nameController,
              decoration: const InputDecoration(labelText: 'Name'),
            ),
            TextField(
              controller: _sortController,
              decoration: const InputDecoration(labelText: 'Sort order'),
              keyboardType: TextInputType.number,
            ),
            TextField(
              controller: _iconController,
              decoration: const InputDecoration(labelText: 'Icon'),
            ),
            TextField(
              controller: _visibilityStartsController,
              decoration: const InputDecoration(
                labelText: 'Visibility starts at (ISO, optional)',
              ),
            ),
            TextField(
              controller: _visibilityEndsController,
              decoration: const InputDecoration(
                labelText: 'Visibility ends at (ISO, optional)',
              ),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          onPressed: _submit,
          child: const Text('Save'),
        ),
      ],
    );
  }

  void _submit() {
    final name = _nameController.text.trim();
    final sortOrder = int.tryParse(_sortController.text.trim());
    if (name.isEmpty || sortOrder == null) return;
    context.read<AdminMarketCubit>().saveCategory(
      id: widget.category?.id,
      name: name,
      sortOrder: sortOrder,
      icon: _emptyToNull(_iconController.text),
      visibilityStartsAt: _optionalDate(_visibilityStartsController.text),
      visibilityEndsAt: _optionalDate(_visibilityEndsController.text),
    );
    Navigator.of(context).pop();
  }
}

class _ItemDialog extends StatefulWidget {
  const _ItemDialog({required this.categories, this.item});

  final List<MarketCategory> categories;
  final MarketItem? item;

  @override
  State<_ItemDialog> createState() => _ItemDialogState();
}

class _ItemDialogState extends State<_ItemDialog> {
  final _formKey = GlobalKey<FormState>();
  late String _categoryId;
  late MarketItemType _itemType;
  late PerPlayerLimitWindow _limitWindow;
  late _ItemFormMode _mode;
  late final TextEditingController _quantityController;
  late final TextEditingController _priceController;
  late final TextEditingController _promoController;
  late final TextEditingController _promoStartsController;
  late final TextEditingController _promoEndsController;
  late final TextEditingController _stockController;
  late final TextEditingController _limitController;

  @override
  void initState() {
    super.initState();
    final item = widget.item;
    _categoryId = item?.categoryId ?? widget.categories.first.id;
    _itemType = item?.itemType ?? MarketItemType.energy;
    _limitWindow = item?.perPlayerLimitWindow ?? PerPlayerLimitWindow.lifetime;
    _mode =
        item == null ||
            item.promoPrice == null &&
                item.promotionStartsAt == null &&
                item.promotionEndsAt == null
        ? _ItemFormMode.normal
        : _ItemFormMode.promotion;
    _quantityController = TextEditingController(
      text: (item?.quantity ?? 1).toString(),
    );
    _priceController = TextEditingController(
      text: (item?.price ?? 1).toString(),
    );
    _promoController = TextEditingController(
      text: item?.promoPrice?.toString() ?? '',
    );
    _promoStartsController = TextEditingController(
      text: _dateOnlyText(item?.promotionStartsAt),
    );
    _promoEndsController = TextEditingController(
      text: _dateOnlyText(item?.promotionEndsAt),
    );
    _stockController = TextEditingController(
      text: item?.maxStock?.toString() ?? '',
    );
    _limitController = TextEditingController(
      text: item?.perPlayerLimit?.toString() ?? '',
    );
    for (final controller in [
      _quantityController,
      _priceController,
      _promoController,
      _promoStartsController,
      _promoEndsController,
      _stockController,
      _limitController,
    ]) {
      controller.addListener(_refreshForm);
    }
  }

  @override
  void dispose() {
    _limitController.dispose();
    _stockController.dispose();
    _promoEndsController.dispose();
    _promoStartsController.dispose();
    _promoController.dispose();
    _priceController.dispose();
    _quantityController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.item == null ? 'New item' : 'Edit item'),
      content: SizedBox(
        width: 480,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Form(
                key: _formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    SegmentedButton<_ItemFormMode>(
                      segments: const [
                        ButtonSegment(
                          value: _ItemFormMode.normal,
                          label: Text('Normal'),
                        ),
                        ButtonSegment(
                          value: _ItemFormMode.promotion,
                          label: Text('Promotion'),
                        ),
                      ],
                      selected: {_mode},
                      onSelectionChanged: (selection) {
                        setState(() => _mode = selection.first);
                      },
                    ),
                    const SizedBox(height: 12),
                    DropdownButtonFormField<String>(
                      initialValue: _categoryId,
                      decoration: const InputDecoration(labelText: 'Category'),
                      items: [
                        for (final category in widget.categories)
                          DropdownMenuItem(
                            value: category.id,
                            child: Text(category.name),
                          ),
                      ],
                      validator: (value) =>
                          value == null || value.isEmpty ? 'Required' : null,
                      onChanged: (value) {
                        if (value != null) setState(() => _categoryId = value);
                      },
                    ),
                    DropdownButtonFormField<MarketItemType>(
                      initialValue: _itemType,
                      decoration: const InputDecoration(labelText: 'Item type'),
                      items: [
                        for (final type in MarketItemType.values)
                          DropdownMenuItem(
                            value: type,
                            child: Text(type.wire),
                          ),
                      ],
                      validator: (value) => value == null ? 'Required' : null,
                      onChanged: (value) {
                        if (value != null) setState(() => _itemType = value);
                      },
                    ),
                    _numberField(
                      _quantityController,
                      'Quantity',
                      required: true,
                    ),
                    _numberField(
                      _priceController,
                      'Price diamonds',
                      required: true,
                    ),
                    if (_mode == _ItemFormMode.promotion) ...[
                      _numberField(
                        _promoController,
                        'Promo price',
                        required: true,
                        validator: _promoPriceValidator,
                      ),
                      _dateField(
                        context,
                        _promoStartsController,
                        'Promotion starts',
                      ),
                      _dateField(
                        context,
                        _promoEndsController,
                        'Promotion ends',
                        validator: _promoEndValidator,
                      ),
                      _numberField(_stockController, 'Max stock'),
                      _numberField(_limitController, 'Per-player limit'),
                      DropdownButtonFormField<PerPlayerLimitWindow>(
                        initialValue: _limitWindow,
                        decoration: const InputDecoration(
                          labelText: 'Limit window',
                        ),
                        items: [
                          for (final window in PerPlayerLimitWindow.values)
                            DropdownMenuItem(
                              value: window,
                              child: Text(window.wire),
                            ),
                        ],
                        onChanged: (value) {
                          if (value != null) {
                            setState(() => _limitWindow = value);
                          }
                        },
                      ),
                    ],
                  ],
                ),
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
          onPressed: _canSubmit ? _submit : null,
          child: const Text('Save'),
        ),
      ],
    );
  }

  Widget _numberField(
    TextEditingController controller,
    String label, {
    bool required = false,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      decoration: InputDecoration(labelText: label),
      keyboardType: TextInputType.number,
      validator:
          validator ??
          (value) {
            final trimmed = value?.trim() ?? '';
            if (trimmed.isEmpty) return required ? 'Required' : null;
            final parsed = int.tryParse(trimmed);
            if (parsed == null) return 'Enter a number';
            if (parsed <= 0) return 'Must be greater than 0';
            return null;
          },
    );
  }

  Widget _dateField(
    BuildContext context,
    TextEditingController controller,
    String label, {
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      readOnly: true,
      decoration: InputDecoration(
        labelText: label,
        suffixIcon: const Icon(Icons.calendar_today),
      ),
      validator: validator ?? _requiredDateValidator,
      onTap: () => _pickDate(context, controller),
    );
  }

  Future<void> _pickDate(
    BuildContext context,
    TextEditingController controller,
  ) async {
    final now = DateTime.now();
    final existing = _optionalDate(controller.text);
    final picked = await showDatePicker(
      context: context,
      initialDate: existing ?? now,
      firstDate: DateTime(now.year - 1),
      lastDate: DateTime(now.year + 5),
    );
    if (picked == null) return;
    controller.text = _dateOnlyText(picked);
  }

  bool get _canSubmit {
    final quantity = int.tryParse(_quantityController.text.trim());
    final price = int.tryParse(_priceController.text.trim());
    if (_categoryId.isEmpty || quantity == null || quantity <= 0) return false;
    if (price == null || price <= 0) return false;
    if (_mode == _ItemFormMode.normal) return true;

    final promoPrice = int.tryParse(_promoController.text.trim());
    final startsAt = _optionalDate(_promoStartsController.text);
    final endsAt = _optionalDate(_promoEndsController.text);
    if (promoPrice == null || promoPrice <= 0 || promoPrice >= price) {
      return false;
    }
    if (startsAt == null || endsAt == null || !startsAt.isBefore(endsAt)) {
      return false;
    }
    return _optionalPositiveIntIsValid(_stockController.text) &&
        _optionalPositiveIntIsValid(_limitController.text);
  }

  String? _promoPriceValidator(String? value) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return 'Required';
    final promoPrice = int.tryParse(trimmed);
    final price = int.tryParse(_priceController.text.trim());
    if (promoPrice == null) return 'Enter a number';
    if (promoPrice <= 0) return 'Must be greater than 0';
    if (price != null && promoPrice >= price) {
      return 'Must be lower than price';
    }
    return null;
  }

  String? _requiredDateValidator(String? value) {
    if (_optionalDate(value ?? '') == null) return 'Required';
    return null;
  }

  String? _promoEndValidator(String? value) {
    final endError = _requiredDateValidator(value);
    if (endError != null) return endError;
    final startsAt = _optionalDate(_promoStartsController.text);
    final endsAt = _optionalDate(value ?? '');
    if (startsAt != null && endsAt != null && !startsAt.isBefore(endsAt)) {
      return 'Must be after start';
    }
    return null;
  }

  void _refreshForm() {
    if (mounted) setState(() {});
  }

  void _submit() {
    if (!_canSubmit || !(_formKey.currentState?.validate() ?? false)) return;

    final isPromotion = _mode == _ItemFormMode.promotion;
    final quantity = int.parse(_quantityController.text.trim());
    final price = int.parse(_priceController.text.trim());

    context.read<AdminMarketCubit>().saveItem(
      id: widget.item?.id,
      categoryId: _categoryId,
      itemType: _itemType,
      quantity: quantity,
      price: price,
      promoPrice: isPromotion ? _optionalInt(_promoController.text) : null,
      promotionStartsAt: isPromotion
          ? _optionalDate(_promoStartsController.text)
          : null,
      promotionEndsAt: isPromotion
          ? _optionalDate(_promoEndsController.text)
          : null,
      maxStock: isPromotion ? _optionalInt(_stockController.text) : null,
      perPlayerLimit: isPromotion ? _optionalInt(_limitController.text) : null,
      perPlayerLimitWindow: _limitWindow,
    );
    Navigator.of(context).pop();
  }
}

String _dateText(DateTime? value) => value?.toIso8601String() ?? '';

String _dateOnlyText(DateTime? value) {
  if (value == null) return '';
  final year = value.year.toString().padLeft(4, '0');
  final month = value.month.toString().padLeft(2, '0');
  final day = value.day.toString().padLeft(2, '0');
  return '$year-$month-$day';
}

DateTime? _optionalDate(String value) {
  final trimmed = value.trim();
  if (trimmed.isEmpty) return null;
  return DateTime.tryParse(trimmed);
}

int? _optionalInt(String value) {
  final trimmed = value.trim();
  if (trimmed.isEmpty) return null;
  return int.tryParse(trimmed);
}

bool _optionalPositiveIntIsValid(String value) {
  final trimmed = value.trim();
  if (trimmed.isEmpty) return true;
  final parsed = int.tryParse(trimmed);
  return parsed != null && parsed > 0;
}

String? _emptyToNull(String value) {
  final trimmed = value.trim();
  return trimmed.isEmpty ? null : trimmed;
}
