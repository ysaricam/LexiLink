import 'package:google_sign_in/google_sign_in.dart';
import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:sign_in_with_apple/sign_in_with_apple.dart';

class SocialSignInException implements Exception {
  const SocialSignInException(this.message);

  final String message;
}

class SocialSignInService {
  SocialSignInService({
    GoogleSignIn? googleSignIn,
  }) : _googleSignIn =
           googleSignIn ??
           GoogleSignIn(
             scopes: const ['email'],
             serverClientId: _googleServerClientId.isEmpty
                 ? null
                 : _googleServerClientId,
           );

  static const _googleServerClientId = String.fromEnvironment(
    'GOOGLE_SIGN_IN_SERVER_CLIENT_ID',
  );

  final GoogleSignIn _googleSignIn;

  Future<SocialIdentity> signInWithGoogle() async {
    final account = await _googleSignIn.signIn();
    if (account == null) {
      throw const SocialSignInException('Google sign-in was cancelled.');
    }

    final auth = await account.authentication;
    final idToken = auth.idToken;
    if (idToken == null || idToken.isEmpty) {
      throw const SocialSignInException(
        'Google did not return an identity token.',
      );
    }

    return SocialIdentity(
      provider: SocialAuthProvider.google,
      externalId: account.id,
      externalToken: idToken,
      email: account.email,
    );
  }

  Future<SocialIdentity> signInWithApple() async {
    final available = await SignInWithApple.isAvailable();
    if (!available) {
      throw const SocialSignInException(
        'Sign in with Apple is not available on this device.',
      );
    }

    final credential = await SignInWithApple.getAppleIDCredential(
      scopes: [
        AppleIDAuthorizationScopes.email,
        AppleIDAuthorizationScopes.fullName,
      ],
    );
    final idToken = credential.identityToken;
    if (idToken == null || idToken.isEmpty) {
      throw const SocialSignInException(
        'Apple did not return an identity token.',
      );
    }
    final userIdentifier = credential.userIdentifier;
    if (userIdentifier == null || userIdentifier.isEmpty) {
      throw const SocialSignInException(
        'Apple did not return a user identifier.',
      );
    }

    return SocialIdentity(
      provider: SocialAuthProvider.apple,
      externalId: userIdentifier,
      externalToken: idToken,
      email: credential.email,
    );
  }
}
