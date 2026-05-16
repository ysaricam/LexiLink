import 'dart:convert';

class ApiException implements Exception {
  const ApiException({
    required this.statusCode,
    required this.message,
    this.problem,
  });

  final int statusCode;
  final String message;
  final ApiProblemDetails? problem;

  bool get isUnauthorized => statusCode == 401;

  @override
  String toString() => 'ApiException($statusCode, $message)';
}

class ApiProblemDetails {
  const ApiProblemDetails({
    required this.title,
    this.status,
    this.detail,
    this.type,
    this.instance,
    this.traceId,
    this.errors = const {},
  });

  factory ApiProblemDetails.fromJson(Map<String, dynamic> json) {
    final errorsJson = json['errors'];
    final errors = <String, List<String>>{};

    if (errorsJson is Map<String, dynamic>) {
      for (final entry in errorsJson.entries) {
        final value = entry.value;
        if (value is List) {
          errors[entry.key] = value.map((item) => item.toString()).toList();
        }
      }
    }

    return ApiProblemDetails(
      title: json['title']?.toString() ?? 'Request failed',
      status: json['status'] is int ? json['status'] as int : null,
      detail: json['detail']?.toString(),
      type: json['type']?.toString(),
      instance: json['instance']?.toString(),
      traceId: json['traceId']?.toString(),
      errors: errors,
    );
  }

  final String title;
  final int? status;
  final String? detail;
  final String? type;
  final String? instance;
  final String? traceId;
  final Map<String, List<String>> errors;

  String get userMessage => detail == null || detail!.isEmpty ? title : detail!;
}

ApiException mapApiError({
  required int statusCode,
  required String body,
  required Map<String, String> headers,
}) {
  final contentType = headers.entries
      .firstWhere(
        (entry) => entry.key.toLowerCase() == 'content-type',
        orElse: () => const MapEntry('content-type', ''),
      )
      .value;

  if (contentType.contains('application/problem+json') && body.isNotEmpty) {
    final decoded = jsonDecode(body);
    if (decoded is Map<String, dynamic>) {
      final problem = ApiProblemDetails.fromJson(decoded);
      return ApiException(
        statusCode: statusCode,
        message: problem.userMessage,
        problem: problem,
      );
    }
  }

  return ApiException(
    statusCode: statusCode,
    message: switch (statusCode) {
      401 => 'Authentication is required.',
      404 => 'The requested resource was not found.',
      >= 500 => 'Server error. Try again later.',
      _ => 'Request failed.',
    },
  );
}
