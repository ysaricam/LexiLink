import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _energyBody = '''
{
  "playerId": "11111111-1111-1111-1111-111111111111",
  "currentAmount": 4,
  "maximumAmount": 5,
  "isFull": false,
  "rechargeIntervalSeconds": 900,
  "lastRefilledOn": "2026-05-14T10:00:00Z",
  "secondsUntilNextRefill": 300,
  "fullyRefilledAt": "2026-05-14T10:15:00Z"
}
''';

EnergyCubit _buildCubit({required MockClientHandler handler}) {
  final apiClient = ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: MockClient(handler),
  );

  return EnergyCubit(energyRepository: EnergyRepository(apiClient: apiClient));
}

void main() {
  group('EnergyCubit', () {
    blocTest<EnergyCubit, EnergyState>(
      'loads energy snapshot',
      build: () => _buildCubit(
        handler: (request) async {
          expect(request.url.path, '/energy/me');
          return http.Response(_energyBody, 200);
        },
      ),
      act: (cubit) => cubit.loadEnergy(),
      verify: (cubit) {
        expect(cubit.state.status, EnergyStatus.success);
        expect(cubit.state.energy?.currentAmount, 4);
        expect(cubit.state.energy?.maximumAmount, 5);
        expect(cubit.state.energy?.secondsUntilNextRefill, 300);
      },
    );

    blocTest<EnergyCubit, EnergyState>(
      'maps ApiException to failure message',
      build: () => _buildCubit(
        handler: (_) async => http.Response('', 401),
      ),
      act: (cubit) => cubit.loadEnergy(),
      expect: () => const [
        EnergyState.loading(),
        EnergyState.failure(message: 'Authentication is required.'),
      ],
    );
  });
}
