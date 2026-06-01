using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Ads.Application.Contracts;
using LexiLink.Modules.Ads.Application.RewardedAdGrants.GetRewardedAdStatus;
using LexiLink.Modules.Ads.Application.RewardedAdGrants.GrantRewardedAdReward;

namespace LexiLink.API.Modules.Ads;

public static class AdsEndpoints
{
    public static IEndpointRouteBuilder MapAdsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/ads")
            .WithTags("Ads")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // AdMob Server-Side Verification ingress. Google calls this directly,
        // so it is anonymous but signature-verified inside the handler. Always
        // returns 200 — a non-2xx makes AdMob retry the callback. The body
        // reports the outcome (Granted / AlreadyGranted / DailyLimitReached /
        // VerificationFailed) for diagnostics.
        group.MapGet("/rewarded/callback", async (
            HttpContext httpContext,
            IAdsModule adsModule,
            CancellationToken cancellationToken) =>
        {
            var query = httpContext.Request.Query;

            var command = new GrantRewardedAdRewardCommand(
                userId: query["user_id"].ToString(),
                transactionId: query["transaction_id"].ToString(),
                adUnitId: query["ad_unit"].ToString(),
                customData: NullIfEmpty(query["custom_data"].ToString()),
                rewardAmount: int.TryParse(query["reward_amount"].ToString(), out var rewardAmount)
                    ? rewardAmount
                    : 0,
                rewardItem: NullIfEmpty(query["reward_item"].ToString()),
                keyId: query["key_id"].ToString(),
                signature: query["signature"].ToString(),
                signedContent: ExtractSignedContent(httpContext.Request.QueryString.Value));

            var result = await adsModule.ExecuteCommandAsync(command, cancellationToken);

            return Results.Ok(result);
        })
        .AllowAnonymous()
        .Produces<GrantRewardedAdRewardResultDto>();

        group.MapGet("/rewarded/status", async (
            IExecutionContextAccessor executionContextAccessor,
            IAdsModule adsModule,
            CancellationToken cancellationToken) =>
        {
            var status = await adsModule.ExecuteQueryAsync(
                new GetRewardedAdStatusQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(status);
        })
        .Produces<RewardedAdStatusDto>();

        return app;
    }

    // AdMob signs the query-string content up to (not including) the
    // "&signature=" parameter; reconstruct exactly that span.
    private static string ExtractSignedContent(string? rawQueryString)
    {
        if (string.IsNullOrEmpty(rawQueryString))
        {
            return string.Empty;
        }

        var query = rawQueryString.StartsWith('?') ? rawQueryString[1..] : rawQueryString;
        var signatureIndex = query.IndexOf("&signature=", StringComparison.Ordinal);
        return signatureIndex >= 0 ? query[..signatureIndex] : query;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrEmpty(value) ? null : value;
}
