enum SocialAuthProvider {
  apple('Apple');

  const SocialAuthProvider(this.apiValue);

  final String apiValue;
}

class SocialIdentity {
  const SocialIdentity({
    required this.provider,
    required this.externalId,
    required this.externalToken,
    this.email,
  });

  final SocialAuthProvider provider;
  final String externalId;
  final String externalToken;
  final String? email;
}
