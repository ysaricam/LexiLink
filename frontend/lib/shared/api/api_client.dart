import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class ApiClient {
  const ApiClient({
    required ApiConfig config,
    required http.Client httpClient,
    required TokenStore tokenStore,
  }) : _config = config,
       _httpClient = httpClient,
       _tokenStore = tokenStore;

  final ApiConfig _config;
  final http.Client _httpClient;
  final TokenStore _tokenStore;

  Future<Map<String, dynamic>> getJson(
    String path, {
    Map<String, String>? queryParameters,
  }) async {
    final headers = await _headers();
    final response = await _send(
      () => _httpClient.get(
        _config.uri(path, queryParameters),
        headers: headers,
      ),
    );

    return _decodeObject(response.body);
  }

  Future<List<dynamic>> getJsonList(
    String path, {
    Map<String, String>? queryParameters,
  }) async {
    final headers = await _headers();
    final response = await _send(
      () => _httpClient.get(
        _config.uri(path, queryParameters),
        headers: headers,
      ),
    );

    return _decodeList(response.body);
  }

  Future<Map<String, dynamic>> postJson(
    String path, {
    Map<String, dynamic>? body,
  }) async {
    final headers = await _headers();
    final response = await _send(
      () => _httpClient.post(
        _config.uri(path),
        headers: headers,
        body: body == null ? null : jsonEncode(body),
      ),
    );

    return _decodeObject(response.body);
  }

  Future<http.Response> _send(Future<http.Response> Function() request) async {
    final response = await request().timeout(_config.timeout);

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return response;
    }

    throw mapApiError(
      statusCode: response.statusCode,
      body: response.body,
      headers: response.headers,
    );
  }

  Future<Map<String, String>> _headers() async {
    final token = await _tokenStore.readAccessToken();

    return {
      'accept': 'application/json',
      'content-type': 'application/json',
      if (token != null && token.isNotEmpty) 'authorization': 'Bearer $token',
    };
  }

  Map<String, dynamic> _decodeObject(String body) {
    if (body.isEmpty) {
      return {};
    }

    final decoded = jsonDecode(body);
    if (decoded is Map<String, dynamic>) {
      return decoded;
    }

    throw const ApiException(
      statusCode: 0,
      message: 'Unexpected API response.',
    );
  }

  List<dynamic> _decodeList(String body) {
    if (body.isEmpty) {
      return [];
    }

    final decoded = jsonDecode(body);
    if (decoded is List<dynamic>) {
      return decoded;
    }

    throw const ApiException(
      statusCode: 0,
      message: 'Unexpected API response.',
    );
  }
}
