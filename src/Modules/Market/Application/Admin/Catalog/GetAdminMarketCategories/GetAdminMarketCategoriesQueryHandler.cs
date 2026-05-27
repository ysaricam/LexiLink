using LexiLink.Common.Application.Data;
using LexiLink.Modules.Market.Application.Configuration.Queries;

namespace LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketCategories;

internal sealed class GetAdminMarketCategoriesQueryHandler
    : IQueryHandler<GetAdminMarketCategoriesQuery, IReadOnlyList<AdminMarketCategoryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetAdminMarketCategoriesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<IReadOnlyList<AdminMarketCategoryDto>> Handle(
        GetAdminMarketCategoriesQuery request,
        CancellationToken cancellationToken) =>
        AdminMarketCatalogSql.GetCategoriesAsync(_sqlConnectionFactory, cancellationToken);
}
