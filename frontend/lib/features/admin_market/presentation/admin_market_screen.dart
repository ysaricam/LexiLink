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
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
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
                  context.l10n.adminMarketConsoleTitle,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 4),
                Text(
                  context.l10n.adminMarketConsoleHelp,
                  style: Theme.of(context).textTheme.bodySmall,
                ),
                const SizedBox(height: 16),
                SegmentedButton<AdminMarketTab>(
                  segments: [
                    ButtonSegment(
                      value: AdminMarketTab.categories,
                      label: Text(context.l10n.adminMarketCategories),
                      icon: const Icon(Icons.category_outlined),
                    ),
                    ButtonSegment(
                      value: AdminMarketTab.items,
                      label: Text(context.l10n.adminMarketItems),
                      icon: const Icon(Icons.storefront_outlined),
                    ),
                    ButtonSegment(
                      value: AdminMarketTab.orders,
                      label: Text(context.l10n.adminMarketOrders),
                      icon: const Icon(Icons.receipt_long_outlined),
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
          label: Text(context.l10n.adminNewCategory),
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
                context.l10n.adminSortStatus(
                  category.sortOrder,
                  category.isActive
                      ? context.l10n.adminActive
                      : context.l10n.adminInactive,
                ),
              ),
              trailing: Wrap(
                spacing: 8,
                children: [
                  TextButton(
                    onPressed: () => _openCategoryDialog(
                      context,
                      category: category,
                    ),
                    child: Text(context.l10n.commonEdit),
                  ),
                  TextButton(
                    onPressed: category.isActive
                        ? () => context
                              .read<AdminMarketCubit>()
                              .deactivateCategory(category.id)
                        : null,
                    child: Text(context.l10n.commonDeactivate),
                  ),
                ],
              ),
            ),
          ),
        if (state.categories.isEmpty)
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(context.l10n.adminNoMarketCategories),
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
          label: Text(context.l10n.adminNewItem),
        ),
        const SizedBox(height: 12),
        for (final item in state.items)
          Card(
            child: ListTile(
              leading: Text(
                item.itemType.symbol,
                style: const TextStyle(fontSize: 26),
              ),
              title: Text(
                '${item.quantity} ${_itemTypeLabel(context, item.itemType)}',
              ),
              subtitle: Text(
                context.l10n.adminMarketItemSubtitle(
                  categoryNames[item.categoryId] ?? item.categoryId,
                  item.effectivePrice,
                  item.remainingStock?.toString() ??
                      context.l10n.commonUnlimited,
                ),
              ),
              trailing: Wrap(
                spacing: 8,
                children: [
                  TextButton(
                    onPressed: () => _openItemDialog(context, item: item),
                    child: Text(context.l10n.commonEdit),
                  ),
                  TextButton(
                    onPressed: item.isActive
                        ? () => context.read<AdminMarketCubit>().deactivateItem(
                            item.id,
                          )
                        : null,
                    child: Text(context.l10n.commonDeactivate),
                  ),
                ],
              ),
            ),
          ),
        if (state.items.isEmpty)
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(context.l10n.adminNoMarketItems),
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
                decoration: InputDecoration(
                  border: const OutlineInputBorder(),
                  isDense: true,
                  labelText: context.l10n.adminPlayerGuid,
                ),
                onSubmitted: (_) => _load(context),
              ),
            ),
            const SizedBox(width: 12),
            FilledButton.icon(
              onPressed: () => _load(context),
              icon: const Icon(Icons.search),
              label: Text(context.l10n.commonLoad),
            ),
          ],
        ),
        const SizedBox(height: 12),
        for (final order in state.orders)
          Card(
            child: ListTile(
              leading: const Icon(Icons.receipt_long_outlined),
              title: Text(
                '${order.quantity} ${_itemTypeLabel(context, order.itemType)}',
              ),
              subtitle: Text(
                context.l10n.adminMarketOrderSubtitle(
                  order.diamondsPaid,
                  order.purchasedAt,
                ),
              ),
            ),
          ),
        if (state.orderPlayerId != null && state.orders.isEmpty)
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(context.l10n.adminNoMarketOrders),
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
      title: Text(
        widget.category == null
            ? context.l10n.adminNewCategory
            : context.l10n.adminEditCategory,
      ),
      content: SizedBox(
        width: 420,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: _nameController,
              decoration: InputDecoration(labelText: context.l10n.adminName),
            ),
            TextField(
              controller: _sortController,
              decoration: InputDecoration(
                labelText: context.l10n.adminSortOrder,
              ),
              keyboardType: TextInputType.number,
            ),
            TextField(
              controller: _iconController,
              decoration: InputDecoration(labelText: context.l10n.adminIcon),
            ),
            TextField(
              controller: _visibilityStartsController,
              decoration: InputDecoration(
                labelText: context.l10n.adminVisibilityStarts,
              ),
            ),
            TextField(
              controller: _visibilityEndsController,
              decoration: InputDecoration(
                labelText: context.l10n.adminVisibilityEnds,
              ),
            ),
          ],
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
      title: Text(
        widget.item == null
            ? context.l10n.adminNewItem
            : context.l10n.adminEditItem,
      ),
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
                      segments: [
                        ButtonSegment(
                          value: _ItemFormMode.normal,
                          label: Text(context.l10n.adminNormal),
                        ),
                        ButtonSegment(
                          value: _ItemFormMode.promotion,
                          label: Text(context.l10n.adminPromotion),
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
                      decoration: InputDecoration(
                        labelText: context.l10n.adminCategory,
                      ),
                      items: [
                        for (final category in widget.categories)
                          DropdownMenuItem(
                            value: category.id,
                            child: Text(category.name),
                          ),
                      ],
                      validator: (value) => value == null || value.isEmpty
                          ? context.l10n.commonRequired
                          : null,
                      onChanged: (value) {
                        if (value != null) setState(() => _categoryId = value);
                      },
                    ),
                    DropdownButtonFormField<MarketItemType>(
                      initialValue: _itemType,
                      decoration: InputDecoration(
                        labelText: context.l10n.adminItemType,
                      ),
                      items: [
                        for (final type in MarketItemType.values)
                          DropdownMenuItem(
                            value: type,
                            child: Text(_itemTypeLabel(context, type)),
                          ),
                      ],
                      validator: (value) =>
                          value == null ? context.l10n.commonRequired : null,
                      onChanged: (value) {
                        if (value != null) setState(() => _itemType = value);
                      },
                    ),
                    _numberField(
                      _quantityController,
                      context.l10n.adminQuantity,
                      required: true,
                    ),
                    _numberField(
                      _priceController,
                      context.l10n.adminPriceDiamonds,
                      required: true,
                    ),
                    if (_mode == _ItemFormMode.promotion) ...[
                      _numberField(
                        _promoController,
                        context.l10n.adminPromoPrice,
                        required: true,
                        validator: _promoPriceValidator,
                      ),
                      _dateField(
                        context,
                        _promoStartsController,
                        context.l10n.adminPromotionStarts,
                      ),
                      _dateField(
                        context,
                        _promoEndsController,
                        context.l10n.adminPromotionEnds,
                        validator: _promoEndValidator,
                      ),
                      _numberField(
                        _stockController,
                        context.l10n.adminMaxStock,
                      ),
                      _numberField(
                        _limitController,
                        context.l10n.adminPerPlayerLimit,
                      ),
                      DropdownButtonFormField<PerPlayerLimitWindow>(
                        initialValue: _limitWindow,
                        decoration: InputDecoration(
                          labelText: context.l10n.adminLimitWindow,
                        ),
                        items: [
                          for (final window in PerPlayerLimitWindow.values)
                            DropdownMenuItem(
                              value: window,
                              child: Text(_limitWindowLabel(context, window)),
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
          child: Text(context.l10n.commonCancel),
        ),
        FilledButton(
          onPressed: _canSubmit ? _submit : null,
          child: Text(context.l10n.commonSave),
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
            if (trimmed.isEmpty) {
              return required ? context.l10n.commonRequired : null;
            }
            final parsed = int.tryParse(trimmed);
            if (parsed == null) return context.l10n.commonEnterNumber;
            if (parsed <= 0) return context.l10n.commonGreaterThanZero;
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
    if (trimmed.isEmpty) return context.l10n.commonRequired;
    final promoPrice = int.tryParse(trimmed);
    final price = int.tryParse(_priceController.text.trim());
    if (promoPrice == null) return context.l10n.commonEnterNumber;
    if (promoPrice <= 0) return context.l10n.commonGreaterThanZero;
    if (price != null && promoPrice >= price) {
      return context.l10n.adminMustBeLowerThanPrice;
    }
    return null;
  }

  String? _requiredDateValidator(String? value) {
    if (_optionalDate(value ?? '') == null) return context.l10n.commonRequired;
    return null;
  }

  String? _promoEndValidator(String? value) {
    final endError = _requiredDateValidator(value);
    if (endError != null) return endError;
    final startsAt = _optionalDate(_promoStartsController.text);
    final endsAt = _optionalDate(value ?? '');
    if (startsAt != null && endsAt != null && !startsAt.isBefore(endsAt)) {
      return context.l10n.adminMustBeAfterStart;
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

String _itemTypeLabel(BuildContext context, MarketItemType type) =>
    switch (type) {
      MarketItemType.energy => context.l10n.adminMarketTypeEnergy,
      MarketItemType.hint => context.l10n.adminMarketTypeHint,
      MarketItemType.undo => context.l10n.adminMarketTypeUndo,
      MarketItemType.reset => context.l10n.adminMarketTypeReset,
      MarketItemType.diamond => context.l10n.adminMarketTypeDiamond,
    };

String _limitWindowLabel(BuildContext context, PerPlayerLimitWindow window) =>
    switch (window) {
      PerPlayerLimitWindow.lifetime => context.l10n.adminLimitLifetime,
      PerPlayerLimitWindow.daily => context.l10n.adminLimitDaily,
      PerPlayerLimitWindow.perPromo => context.l10n.adminLimitPerPromo,
    };

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
