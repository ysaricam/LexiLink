import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/admin_auth/application/admin_session_cubit.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_auth_repository.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class AdminLoginScreen extends StatefulWidget {
  const AdminLoginScreen({super.key, this.tokenStoreFactory});

  /// Optional override for tests. In production the screen builds a
  /// [SharedPreferencesAdminTokenStore] lazily on mount.
  final Future<TokenStore> Function()? tokenStoreFactory;

  @override
  State<AdminLoginScreen> createState() => _AdminLoginScreenState();
}

class _AdminLoginScreenState extends State<AdminLoginScreen> {
  late final TextEditingController _emailController;
  late final TextEditingController _tokenController;
  AdminSessionCubit? _cubit;
  bool _initializing = true;

  @override
  void initState() {
    super.initState();
    _emailController = TextEditingController();
    _tokenController = TextEditingController();
    _initialize();
  }

  Future<void> _initialize() async {
    final tokenStore =
        await (widget.tokenStoreFactory?.call() ??
            SharedPreferencesAdminTokenStore.create());

    if (!mounted) return;

    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: http.Client(),
      // Anonymous: /auth/admin/token doesn't need a bearer.
      tokenStore: const _AnonymousTokenStore(),
    );

    setState(() {
      _cubit = AdminSessionCubit(
        repository: AdminAuthRepository(apiClient: apiClient),
        adminTokenStore: tokenStore,
      )..checkSession();
      _initializing = false;
    });
  }

  @override
  void dispose() {
    _emailController.dispose();
    _tokenController.dispose();
    _cubit?.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_initializing || _cubit == null) {
      return const AppScreen(
        child: Center(child: CircularProgressIndicator()),
      );
    }

    return BlocProvider.value(
      value: _cubit!,
      child: BlocConsumer<AdminSessionCubit, AdminSessionState>(
        listenWhen: (prev, curr) =>
            prev.status != curr.status &&
            curr.status == AdminSessionStatus.authenticated,
        listener: (context, state) {
          context.go('/admin');
        },
        builder: (context, state) {
          return AppScreen(
            size: AppScreenSize.compact,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SizedBox(height: 32),
                Text(
                  context.l10n.adminSignInTitle,
                  style: Theme.of(context).textTheme.headlineMedium,
                ),
                const SizedBox(height: 8),
                Text(
                  context.l10n.adminSignInHelp,
                  style: Theme.of(context).textTheme.bodySmall,
                ),
                const SizedBox(height: 24),
                TextField(
                  controller: _emailController,
                  decoration: InputDecoration(
                    labelText: context.l10n.adminEmailLabel,
                    border: const OutlineInputBorder(),
                  ),
                  keyboardType: TextInputType.emailAddress,
                  autocorrect: false,
                  enableSuggestions: false,
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _tokenController,
                  decoration: InputDecoration(
                    labelText: context.l10n.adminExternalTokenLabel,
                    border: const OutlineInputBorder(),
                  ),
                  obscureText: true,
                ),
                const SizedBox(height: 24),
                AppPrimaryButton(
                  label: state.status == AdminSessionStatus.authenticating
                      ? context.l10n.adminSigningIn
                      : context.l10n.adminSignIn,
                  onPressed: state.status == AdminSessionStatus.authenticating
                      ? null
                      : () => _submit(context),
                ),
                if (state.status == AdminSessionStatus.failure) ...[
                  const SizedBox(height: 16),
                  Text(
                    state.errorMessage ?? context.l10n.adminSignInFailed,
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.error,
                    ),
                  ),
                ],
              ],
            ),
          );
        },
      ),
    );
  }

  void _submit(BuildContext context) {
    final email = _emailController.text.trim();
    final token = _tokenController.text;
    context.read<AdminSessionCubit>().signIn(
      email: email,
      externalToken: token,
    );
  }
}

class _AnonymousTokenStore implements TokenStore {
  const _AnonymousTokenStore();

  @override
  String? get accessToken => null;
  @override
  Future<String?> readAccessToken() async => null;
  @override
  Future<void> saveAccessToken(String token) async {}
  @override
  Future<String?> readPlayerId() async => null;
  @override
  Future<void> savePlayerId(String playerId) async {}
  @override
  Future<void> clear() async {}
}
