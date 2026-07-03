namespace LexiLink.Modules.Games.Application.Games.GetGameOptions;

internal static class OutgoingLinkOrderer
{
    public static List<Guid> OrderForDisplay(
        IReadOnlyList<Guid> ids,
        Guid? previousLinkId)
    {
        var salt = StableShuffleSalt(ids);
        var ordered = ids
            .OrderBy(id => StableShuffleKey(id, salt))
            .ThenBy(id => id)
            .ToList();

        if (previousLinkId is { } prev && ordered.Contains(prev))
        {
            ordered.Remove(prev);
            ordered.Insert(0, prev);
        }

        return ordered;
    }

    private static ulong StableShuffleSalt(IEnumerable<Guid> ids)
    {
        const ulong offset = 14695981039346656037;
        const ulong prime = 1099511628211;

        var hash = offset;
        foreach (var id in ids.OrderBy(id => id))
        {
            foreach (var b in id.ToByteArray())
            {
                hash ^= b;
                hash *= prime;
            }
        }

        return hash;
    }

    private static ulong StableShuffleKey(Guid id, ulong salt)
    {
        const ulong prime = 1099511628211;

        var hash = salt;
        foreach (var b in id.ToByteArray())
        {
            hash ^= b;
            hash *= prime;
        }

        return hash;
    }
}
