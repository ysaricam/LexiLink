import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('gets player energy snapshot', () async {
    final repository = EnergyRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/energy/me');

          return http.Response(
            '''
{
  "playerId": "11111111-1111-1111-1111-111111111111",
  "currentAmount": 3,
  "maximumAmount": 5,
  "isFull": false,
  "rechargeIntervalSeconds": 900,
  "lastRefilledOn": "2026-05-14T10:00:00Z",
  "secondsUntilNextRefill": 600,
  "fullyRefilledAt": "2026-05-14T10:30:00Z"
}
''',
            200,
          );
        }),
      ),
    );

    final energy = await repository.getMe();

    expect(energy.playerId, '11111111-1111-1111-1111-111111111111');
    expect(energy.currentAmount, 3);
    expect(energy.maximumAmount, 5);
    expect(energy.isFull, isFalse);
    expect(energy.rechargeIntervalSeconds, 900);
    expect(energy.secondsUntilNextRefill, 600);
    expect(energy.fullyRefilledAt, DateTime.parse('2026-05-14T10:30:00Z'));
  });

  test('parses full energy snapshot with null countdown fields', () async {
    final repository = EnergyRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((_) async => http.Response(
          '''
{
  "playerId": "22222222-2222-2222-2222-222222222222",
  "currentAmount": 5,
  "maximumAmount": 5,
  "isFull": true,
  "rechargeIntervalSeconds": 900,
  "lastRefilledOn": "2026-05-14T10:00:00Z",
  "secondsUntilNextRefill": null,
  "fullyRefilledAt": null
}
''',
          200,
        )),
      ),
    );

    final energy = await repository.getMe();

    expect(energy.isFull, isTrue);
    expect(energy.secondsUntilNextRefill, isNull);
    expect(energy.fullyRefilledAt, isNull);
  });
}
