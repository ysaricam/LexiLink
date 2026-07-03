using LexiLink.Modules.Games.Application.Links.AddOutgoingLink;
using LexiLink.Modules.Games.Application.Links.CreateLink;
using MediatR;

namespace LexiLink.Modules.Games.IntegrationTests.Links;

internal static class LinkHelper
{
    public static Task<Guid> CreateLinkAsync(ISender sender, Guid categoryId, string value, bool isActive = true)
        => sender.Send(new CreateLinkCommand(categoryId, value, "", isActive));

    public static async Task<List<Guid>> CreateLinksAsync(ISender sender, Guid categoryId, params string[] values)
    {
        var ids = new List<Guid>();
        foreach (var v in values)
            ids.Add(await CreateLinkAsync(sender, categoryId, v));
        return ids;
    }

    public static async Task LinkBidirectionallyAsync(ISender sender, Guid a, Guid b)
    {
        await sender.Send(new AddOutgoingLinkCommand(a, b));
        await sender.Send(new AddOutgoingLinkCommand(b, a));
    }

    public static Task LinkOneWayAsync(ISender sender, Guid from, Guid to)
        => sender.Send(new AddOutgoingLinkCommand(from, to));
}
