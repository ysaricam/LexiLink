using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;

public sealed class GetPlayerAdminDetailByHandleQuery : QueryBase<PlayerAdminDetailDto?>
{
    public string DisplayName { get; }

    public int Discriminator { get; }

    public GetPlayerAdminDetailByHandleQuery(string displayName, int discriminator)
    {
        DisplayName = displayName;
        Discriminator = discriminator;
    }
}
