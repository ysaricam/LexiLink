using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Time;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;
using LexiLink.Modules.Payments.Domain;
using LexiLink.Modules.Payments.Infrastructure;
using LexiLink.Modules.Payments.Infrastructure.Configuration;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace LexiLink.Modules.Payments.IntegrationTests.IapPurchases;

[TestFixture]
[Category("Integration")]
public sealed class VerifyIapPurchaseIntegrationTests
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    private static readonly Guid PlayerId = Guid.Parse("57d1326c-75b7-4264-96d8-254995eec0e1");

    private IContainer _container = null!;
    private ILifetimeScope _scope = null!;
    private string _connectionString = null!;
    private FakeAppleIapVerifier _appleVerifier = null!;
    private CountingDiamondGrant _diamondGrant = null!;
    private IPaymentsModule _paymentsModule = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ASPNETCORE_LexiLink_IntegrationTests_ConnectionString")
            ?? DefaultConnectionString;

        _appleVerifier = new FakeAppleIapVerifier();
        _diamondGrant = new CountingDiamondGrant();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventsBus, InMemoryEventsBus>();
        services.AddSingleton<IAdminAuthorizationContext, NoAdminAuthorizationContext>();
        services.AddSingleton<IExecutionContextAccessor, TestExecutionContextAccessor>();
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));

        PaymentsStartup.Initialize(services, _connectionString);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        PaymentsStartup.InitializeCompositionRoot(containerBuilder, _connectionString);

        containerBuilder.RegisterInstance(_appleVerifier)
            .As<IAppleIapVerifier>()
            .SingleInstance();
        containerBuilder.RegisterInstance(new FakeGooglePlayIapVerifier())
            .As<IGooglePlayIapVerifier>()
            .SingleInstance();
        containerBuilder.RegisterInstance(new FakeGooglePlayPurchaseProcessor())
            .As<IGooglePlayPurchaseProcessor>()
            .SingleInstance();
        containerBuilder.RegisterInstance(_diamondGrant)
            .As<IDiamondGrant>()
            .SingleInstance();

        _container = containerBuilder.Build();
        _scope = _container.BeginLifetimeScope();
        _paymentsModule = _scope.Resolve<IPaymentsModule>();

        await ClearDatabaseAsync();
        await SeedPaymentProductAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
        _container.Dispose();
    }

    [Test]
    public async Task VerifyIapPurchase_WhenApplePurchaseIsGranted_CommitsOutboxAndReplaysWithoutSecondGrant()
    {
        const string transactionId = "apple-transaction-outbox-regression";
        _appleVerifier.AddVerified(
            transactionId,
            "diamond_2500",
            accountToken: PlayerId.ToString());

        var command = new VerifyIapPurchaseCommand(
            PlayerId,
            PaymentPlatform.Apple,
            "diamond_2500",
            transactionId,
            purchaseToken: null,
            signedTransactionJws: "signed-jws",
            accountToken: PlayerId.ToString(),
            clientRequestId: $"iap-diamond_2500-{transactionId}");

        var first = await _paymentsModule.ExecuteCommandAsync(command);
        var replay = await _paymentsModule.ExecuteCommandAsync(command);

        first.Status.Should().Be(IapPurchaseStatus.Granted.ToString());
        first.IsReplay.Should().BeFalse();
        first.CanFinishTransaction.Should().BeTrue();
        replay.Status.Should().Be(IapPurchaseStatus.Granted.ToString());
        replay.IsReplay.Should().BeTrue();
        replay.PaymentId.Should().Be(first.PaymentId);

        _diamondGrant.Grants.Should().ContainSingle().Which.Should().Be((PlayerId, 2500));

        var purchase = await QuerySingleAsync<IapPurchaseRow>(
            """
            SELECT "Id", "Status", "StoreTransactionId"
            FROM payments."IapPurchases"
            WHERE "StoreTransactionId" = @TransactionId
            """,
            new { TransactionId = transactionId });

        purchase.Id.Should().Be(first.PaymentId);
        purchase.Status.Should().Be((int)IapPurchaseStatus.Granted);

        var outboxIds = (await QueryAsync<Guid>(
            """
            SELECT "Id"
            FROM payments."OutboxMessages"
            ORDER BY "OccurredOn", "Type"
            """)).ToList();

        outboxIds.Should().HaveCount(3);
        outboxIds.Should().OnlyHaveUniqueItems();
    }

    private async Task ClearDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            DELETE FROM payments."OutboxMessages";
            DELETE FROM payments."IapPurchases";
            DELETE FROM payments."PaymentNotifications";
            DELETE FROM payments."PaymentProducts";
            """);
    }

    private async Task SeedPaymentProductAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO payments."PaymentProducts" (
                "Id",
                "StoreProductId",
                "DiamondAmount",
                "IsAppleAvailable",
                "IsGoogleAvailable",
                "SortOrder",
                "IsActive"
            )
            VALUES (
                '37e980fc-d337-478e-93f7-31e102d74a16',
                'diamond_2500',
                2500,
                TRUE,
                TRUE,
                40,
                TRUE
            );
            """);
    }

    private async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<T>(sql, parameters);
    }

    private async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<T>(sql, parameters);
    }

    private sealed class FakeAppleIapVerifier : IAppleIapVerifier
    {
        private readonly Dictionary<string, StorePurchaseVerificationResult> _resultsByTransactionId = new();

        public void AddVerified(
            string transactionId,
            string storeProductId,
            string? accountToken = null)
        {
            _resultsByTransactionId[transactionId] = StorePurchaseVerificationResult.Verified(
                PaymentPlatform.Apple,
                PaymentEnvironment.Sandbox,
                storeProductId,
                transactionId,
                purchaseToken: null,
                orderId: null,
                accountToken,
                StorePurchasePostProcessingAction.AppleClientFinishTransaction,
                DateTime.UtcNow);
        }

        public Task<StorePurchaseVerificationResult> VerifyAsync(
            AppleIapVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_resultsByTransactionId.TryGetValue(request.TransactionId, out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult(StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                PaymentEnvironment.Sandbox,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Fake Apple verifier has no result for the transaction id."));
        }
    }

    private sealed class FakeGooglePlayIapVerifier : IGooglePlayIapVerifier
    {
        public Task<StorePurchaseVerificationResult> VerifyAsync(
            GooglePlayIapVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Google,
                PaymentEnvironment.Sandbox,
                request.StoreProductId,
                storeTransactionId: null,
                request.PurchaseToken,
                StorePurchaseState.Invalid,
                "Google verification is not used by this test."));
    }

    private sealed class FakeGooglePlayPurchaseProcessor : IGooglePlayPurchaseProcessor
    {
        public Task<GooglePlayPostProcessingResult> AcknowledgeAsync(
            string storeProductId,
            string purchaseToken,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(GooglePlayPostProcessingResult.Success(isReplay: false));

        public Task<GooglePlayPostProcessingResult> ConsumeAsync(
            string storeProductId,
            string purchaseToken,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(GooglePlayPostProcessingResult.Success(isReplay: false));
    }

    private sealed class CountingDiamondGrant : IDiamondGrant
    {
        private readonly List<(Guid PlayerId, int Amount)> _grants = [];

        public IReadOnlyList<(Guid PlayerId, int Amount)> Grants => _grants;

        public Task GrantAsync(
            Guid playerId,
            int amount,
            CancellationToken cancellationToken = default)
        {
            _grants.Add((playerId, amount));
            return Task.CompletedTask;
        }
    }

    private sealed class NoAdminAuthorizationContext : IAdminAuthorizationContext
    {
        public bool IsAdmin => false;
        public Guid? AdminUserId => null;

        public Guid RequireAdminUserId() =>
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Payments integration test.");

        public void EnsureAuthorized()
        {
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Payments integration test.");
        }
    }

    private sealed class TestExecutionContextAccessor : IExecutionContextAccessor
    {
        public Guid UserId => PlayerId;
        public Guid CorrelationId { get; } = Guid.Parse("8f64d7d6-5e1a-4e66-9810-cc20f6e2db0b");
        public bool IsAvailable => true;
        public bool IsAdmin => false;
        public PlayerAuthSessionMode? PlayerAuthSessionMode =>
            LexiLink.Common.Application.PlayerAuthSessionMode.Guest;
        public Guid? AdminUserId => null;
    }

    private sealed class IapPurchaseRow
    {
        public Guid Id { get; init; }
        public int Status { get; init; }
        public string StoreTransactionId { get; init; } = string.Empty;
    }
}
