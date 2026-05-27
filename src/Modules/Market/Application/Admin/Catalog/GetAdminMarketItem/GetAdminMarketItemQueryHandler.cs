using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Market.Application.Configuration.Queries;

namespace LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketItem;

internal sealed class GetAdminMarketItemQueryHandler
    : IQueryHandler<GetAdminMarketItemQuery, AdminMarketItemDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetAdminMarketItemQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public Task<AdminMarketItemDto> Handle(
        GetAdminMarketItemQuery request,
        CancellationToken cancellationToken) =>
        AdminMarketCatalogSql.GetItemAsync(
            _sqlConnectionFactory,
            _clock,
            request.ShopItemId,
            cancellationToken);
}
