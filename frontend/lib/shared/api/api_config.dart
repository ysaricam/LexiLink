class ApiConfig {
  const ApiConfig({
    required this.baseUrl,
    this.timeout = const Duration(seconds: 15),
  });

  factory ApiConfig.local() {
    return const ApiConfig(
      baseUrl: String.fromEnvironment(
        'LEXILINK_API_BASE_URL',
        defaultValue: 'http://127.0.0.1:5000',
      ),
    );
  }

  final String baseUrl;
  final Duration timeout;

  Uri uri(String path, [Map<String, String>? queryParameters]) {
    final normalizedBaseUrl = baseUrl.endsWith('/')
        ? baseUrl.substring(0, baseUrl.length - 1)
        : baseUrl;
    final normalizedPath = path.startsWith('/') ? path : '/$path';

    return Uri.parse('$normalizedBaseUrl$normalizedPath').replace(
      queryParameters: queryParameters,
    );
  }
}
