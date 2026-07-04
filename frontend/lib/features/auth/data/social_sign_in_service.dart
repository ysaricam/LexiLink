import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:sign_in_with_apple/sign_in_with_apple.dart';

class SocialSignInException implements Exception {
  const SocialSignInException(this.message);

  final String message;
}

class SocialSignInService {
  const SocialSignInService();

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
