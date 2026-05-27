using LexiLink.Common.Domain;
using LexiLink.Modules.Market.Domain.Events;
using LexiLink.Modules.Market.Domain.Rules;

namespace LexiLink.Modules.Market.Domain;

public class Category : Entity, IAggregateRoot
{
    private const int NameMaxLength = 100;
    private const int IconMaxLength = 64;

    public CategoryId Id { get; private set; }
    private string _name = null!;
    private int _sortOrder;
    private string? _icon;
    private bool _isActive;
    private VisibilityWindow? _visibilityWindow;

    public string Name => _name;
    public int SortOrder => _sortOrder;
    public string? Icon => _icon;
    public bool IsActive => _isActive;
    public VisibilityWindow? VisibilityWindow => _visibilityWindow;

    private Category()
    {
        Id = null!;
    }

    private Category(
        CategoryId id,
        string name,
        int sortOrder,
        string? icon,
        VisibilityWindow? visibilityWindow)
    {
        Id = id;
        _name = name.Trim();
        _sortOrder = sortOrder;
        _icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        _visibilityWindow = visibilityWindow;
        _isActive = true;

        AddDomainEvent(new MarketCategoryCreatedDomainEvent(Id.Value));
    }

    internal static Category Create(
        string name,
        int sortOrder,
        string? icon,
        VisibilityWindow? visibilityWindow)
    {
        CheckRule(new NameMustNotBeEmptyRule(name));
        CheckRule(new NameMustNotExceedMaxLengthRule(name.Trim(), NameMaxLength));
        CheckRule(new IconMustNotExceedMaxLengthRule(icon?.Trim(), IconMaxLength));

        return new Category(
            new CategoryId(Guid.NewGuid()),
            name,
            sortOrder,
            icon,
            visibilityWindow);
    }

    internal void Update(
        string name,
        int sortOrder,
        string? icon,
        VisibilityWindow? visibilityWindow)
    {
        CheckRule(new NameMustNotBeEmptyRule(name));
        CheckRule(new NameMustNotExceedMaxLengthRule(name.Trim(), NameMaxLength));
        CheckRule(new IconMustNotExceedMaxLengthRule(icon?.Trim(), IconMaxLength));

        _name = name.Trim();
        _sortOrder = sortOrder;
        _icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        _visibilityWindow = visibilityWindow;

        AddDomainEvent(new MarketCategoryUpdatedDomainEvent(Id.Value));
    }

    internal void Deactivate()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;

        AddDomainEvent(new MarketCategoryDeactivatedDomainEvent(Id.Value));
    }

    public bool IsVisibleAt(DateTime now) =>
        _isActive && (_visibilityWindow is null || _visibilityWindow.IsOpenAt(now));
}
