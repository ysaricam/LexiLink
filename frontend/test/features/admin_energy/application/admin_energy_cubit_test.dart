import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_energy/application/admin_energy_cubit.dart';
import 'package:lexilink_app/features/admin_energy/data/admin_energy_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class _Script {
  final List<http.Response Function(http.Request)> _steps = [];
  int _i = 0;
  void enqueue(http.Response Function(http.Request) r) => _steps.add(r);
  http.Response respond(http.Request req) {
    if (_i >= _steps.length) {
      fail('Unexpected HTTP call: ${req.method} ${req.url.path}');
    }
    return _steps[_i++](req);
  }
}

AdminEnergyRepository _repoFromScript(_Script s) => AdminEnergyRepository(
  apiClient: ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: MockClient((req) async => s.respond(req)),
  ),
);

String _snapshot({
  int current = 3,
  int max = 5,
  bool isFull = false,
}) =>
    '{'
    '"playerId":"00000000-0000-0000-0000-000000000abc",'
    '"currentAmount":$current,"maximumAmount":$max,"isFull":$isFull,'
    '"rechargeIntervalSeconds":600,'
    '"lastRefilledOn":"2026-05-22T09:00:00Z",'
    '"secondsUntilNextRefill":null,'
    '"fullyRefilledAt":null'
    '}';

void main() {
  group('AdminEnergyCubit', () {
    blocTest<AdminEnergyCubit, AdminEnergyState>(
      'load happy path emits loaded with snapshot',
      build: () {
        final s = _Script()..enqueue((_) => http.Response(_snapshot(), 200));
        return AdminEnergyCubit(repository: _repoFromScript(s));
      },
      act: (cubit) => cubit.load('00000000-0000-0000-0000-000000000abc'),
      verify: (cubit) {
        expect(cubit.state.status, AdminEnergyStatus.loaded);
        expect(cubit.state.snapshot?.currentAmount, 3);
      },
    );

    blocTest<AdminEnergyCubit, AdminEnergyState>(
      'load emits notFound on 404',
      build: () {
        final s = _Script()
          ..enqueue((_) => http.Response('{"detail":"nope"}', 404));
        return AdminEnergyCubit(repository: _repoFromScript(s));
      },
      act: (cubit) => cubit.load('00000000-0000-0000-0000-000000000abc'),
      verify: (cubit) {
        expect(cubit.state.status, AdminEnergyStatus.notFound);
        expect(cubit.state.snapshot, isNull);
      },
    );

    blocTest<AdminEnergyCubit, AdminEnergyState>(
      'setAmount reloads with new current',
      build: () {
        final s = _Script()
          ..enqueue((_) => http.Response(_snapshot(), 200))
          ..enqueue((_) => http.Response('', 204))
          ..enqueue((_) => http.Response(_snapshot(current: 1), 200));
        return AdminEnergyCubit(repository: _repoFromScript(s));
      },
      act: (cubit) async {
        await cubit.load('00000000-0000-0000-0000-000000000abc');
        await cubit.setAmount(1);
      },
      verify: (cubit) {
        expect(cubit.state.status, AdminEnergyStatus.loaded);
        expect(cubit.state.snapshot?.currentAmount, 1);
      },
    );

    blocTest<AdminEnergyCubit, AdminEnergyState>(
      'grant reloads showing over-max balance',
      build: () {
        final s = _Script()
          ..enqueue((_) => http.Response(_snapshot(current: 5), 200))
          ..enqueue((_) => http.Response('', 204))
          ..enqueue((_) => http.Response(_snapshot(current: 8), 200));
        return AdminEnergyCubit(repository: _repoFromScript(s));
      },
      act: (cubit) async {
        await cubit.load('00000000-0000-0000-0000-000000000abc');
        await cubit.grant(3);
      },
      verify: (cubit) {
        expect(cubit.state.snapshot?.currentAmount, 8);
        expect(cubit.state.snapshot?.maximumAmount, 5);
      },
    );

    blocTest<AdminEnergyCubit, AdminEnergyState>(
      'reset reloads showing full',
      build: () {
        final s = _Script()
          ..enqueue((_) => http.Response(_snapshot(current: 1), 200))
          ..enqueue((_) => http.Response('', 204))
          ..enqueue(
            (_) => http.Response(_snapshot(current: 5, isFull: true), 200),
          );
        return AdminEnergyCubit(repository: _repoFromScript(s));
      },
      act: (cubit) async {
        await cubit.load('00000000-0000-0000-0000-000000000abc');
        await cubit.reset();
      },
      verify: (cubit) {
        expect(cubit.state.snapshot?.currentAmount, 5);
        expect(cubit.state.snapshot?.isFull, isTrue);
      },
    );

    blocTest<AdminEnergyCubit, AdminEnergyState>(
      'mutations without prior load are no-ops',
      build: () => AdminEnergyCubit(repository: _repoFromScript(_Script())),
      act: (cubit) async {
        await cubit.setAmount(1);
        await cubit.grant(1);
        await cubit.reset();
      },
      verify: (cubit) {
        expect(cubit.state.status, AdminEnergyStatus.initial);
      },
    );
  });
}
