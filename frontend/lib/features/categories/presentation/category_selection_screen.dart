import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/categories/application/category_list_cubit.dart';
import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/features/energy/presentation/energy_badge.dart';
import 'package:lexilink_app/features/game/application/game_start_cubit.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class CategorySelectionScreen extends StatefulWidget {
  const CategorySelectionScreen({super.key});

  @override
  State<CategorySelectionScreen> createState() =>
      _CategorySelectionScreenState();
}

class _CategorySelectionScreenState extends State<CategorySelectionScreen> {
  late final Future<SharedPreferencesTokenStore> _tokenStoreFuture;

  @override
  void initState() {
    super.initState();
    _tokenStoreFuture = SharedPreferencesTokenStore.create();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<TokenStore>(
      future: _tokenStoreFuture,
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return AppScreen(
            child: AppErrorState(
              title: context.l10n.sessionStorageFailedTitle,
              message: context.l10n.sessionStorageFailedMessage,
            ),
          );
        }

        final tokenStore = snapshot.data;
        if (tokenStore == null) {
          return AppScreen(
            child: AppLoadingState(message: context.l10n.preparingSession),
          );
        }

        return _CategorySelectionProviders(tokenStore: tokenStore);
      },
    );
  }
}

class _CategorySelectionProviders extends StatefulWidget {
  const _CategorySelectionProviders({required this.tokenStore});

  final TokenStore tokenStore;

  @override
  State<_CategorySelectionProviders> createState() =>
      _CategorySelectionProvidersState();
}

class _CategorySelectionProvidersState
    extends State<_CategorySelectionProviders> {
  late final http.Client _httpClient;
  late final CategoryListCubit _categoryListCubit;
  late final GameStartCubit _gameStartCubit;
  late final EnergyCubit _energyCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );
    _categoryListCubit = CategoryListCubit(
      categoryRepository: CategoryRepository(apiClient: apiClient),
    );
    _gameStartCubit = GameStartCubit(
      gameRepository: GameRepository(apiClient: apiClient),
      tokenStore: widget.tokenStore,
    );
    _energyCubit = EnergyCubit(
      energyRepository: EnergyRepository(apiClient: apiClient),
    );
    final contentLocale = context.read<LocaleCubit>().state.backendLocale;
    unawaited(_categoryListCubit.loadCategories(locale: contentLocale));
    unawaited(_energyCubit.loadEnergy());
  }

  @override
  void dispose() {
    _energyCubit.close();
    _categoryListCubit.close();
    _gameStartCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider.value(value: _categoryListCubit),
        BlocProvider.value(value: _gameStartCubit),
        BlocProvider.value(value: _energyCubit),
      ],
      child: const _CategorySelectionView(),
    );
  }
}

class _CategorySelectionView extends StatelessWidget {
  const _CategorySelectionView();

  @override
  Widget build(BuildContext context) {
    return BlocListener<GameStartCubit, GameStartState>(
      listenWhen: (previous, current) => previous.status != current.status,
      listener: (context, state) {
        if (state.status == GameStartStatus.success && state.gameId != null) {
          context.go('/games/${state.gameId}');
        } else if (state.status == GameStartStatus.failure) {
          // Refresh badge so the user sees the new energy after the backend
          // either rejected (insufficient) or consumed in flight.
          context.read<EnergyCubit>().loadEnergy();
        }
      },
      child: AppScreen(
        child: BlocBuilder<CategoryListCubit, CategoryListState>(
          builder: (context, state) {
            return BlocBuilder<GameStartCubit, GameStartState>(
              builder: (context, gameStartState) {
                return Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      context.l10n.chooseCategory,
                      style: Theme.of(context).textTheme.displaySmall,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 12),
                    Text(
                      context.l10n.chooseCategorySubtitle,
                      style: Theme.of(context).textTheme.bodyLarge,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    const Center(child: EnergyBadge()),
                    const SizedBox(height: 24),
                    if (state.isLoading)
                      AppLoadingState(message: context.l10n.loadingCategories)
                    else if (state.status == CategoryListStatus.failure)
                      AppErrorState(
                        title: context.l10n.couldNotLoadCategories,
                        message: state.message ?? context.l10n.commonTryAgain,
                        onRetry: () =>
                            context.read<CategoryListCubit>().loadCategories(
                              locale: context
                                  .read<LocaleCubit>()
                                  .state
                                  .backendLocale,
                            ),
                      )
                    else if (state.categories.isEmpty)
                      AppEmptyState(
                        title: context.l10n.noCategoriesTitle,
                        message: context.l10n.noCategoriesMessage,
                        actionLabel: context.l10n.commonRefresh,
                        onAction: () =>
                            context.read<CategoryListCubit>().loadCategories(
                              locale: context
                                  .read<LocaleCubit>()
                                  .state
                                  .backendLocale,
                            ),
                      )
                    else
                      _CategoryList(
                        categories: state.categories,
                        selectedCategoryId: state.selectedCategoryId,
                      ),
                    if (state.selectedCategoryId != null) ...[
                      const SizedBox(height: 14),
                      AppPrimaryButton(
                        label: gameStartState.isSubmitting
                            ? context.l10n.commonStarting
                            : context.l10n.startEasyGame,
                        onPressed: gameStartState.isSubmitting
                            ? null
                            : () => context.read<GameStartCubit>().startGame(
                                categoryId: state.selectedCategoryId!,
                              ),
                      ),
                    ],
                    if (gameStartState.status == GameStartStatus.failure) ...[
                      const SizedBox(height: 12),
                      AppErrorState(
                        title: context.l10n.couldNotStartGame,
                        message:
                            gameStartState.message ??
                            context.l10n.commonTryAgain,
                      ),
                    ],
                  ],
                );
              },
            );
          },
        ),
      ),
    );
  }
}

class _CategoryList extends StatelessWidget {
  const _CategoryList({
    required this.categories,
    required this.selectedCategoryId,
  });

  final List<Category> categories;
  final String? selectedCategoryId;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        for (final category in categories) ...[
          _CategoryTile(
            category: category,
            selected: category.id == selectedCategoryId,
          ),
          const SizedBox(height: 10),
        ],
      ],
    );
  }
}

class _CategoryTile extends StatelessWidget {
  const _CategoryTile({
    required this.category,
    required this.selected,
  });

  final Category category;
  final bool selected;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(
          color: selected
              ? colorScheme.primary
              : colorScheme.outline.withValues(alpha: 0.42),
        ),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Material(
        color: selected ? colorScheme.primaryContainer : colorScheme.surface,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: () =>
              context.read<CategoryListCubit>().selectCategory(category.id),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    category.name,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                if (selected)
                  Icon(
                    Icons.check_circle,
                    color: colorScheme.primary,
                  )
                else
                  Icon(
                    Icons.chevron_right,
                    color: colorScheme.onSurfaceVariant,
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
