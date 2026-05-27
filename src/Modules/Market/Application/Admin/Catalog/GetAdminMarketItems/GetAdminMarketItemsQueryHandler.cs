using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Market.Application.Configuration.Queries;

namespace LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketItems;

internal sealed class GetAdminMarketItemsQueryHandler
    : IQueryHandler<GetAdminMarketItemsQuery, IReadOnlyList<AdminMarketItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetAdminMarketItemsQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public Task<IReadOnlyList<AdminMarketItemDto>> Handle(
        GetAdminMarketItemsQuery request,
        CancellationToken cancellationToken) =>
        AdminMarketCatalogSql.GetItemsAsync(
            _sqlConnectionFactory,
            _clock,
            request.CategoryId,
            request.ItemType,
            request.IsActive,
            cancellationToken);
}
