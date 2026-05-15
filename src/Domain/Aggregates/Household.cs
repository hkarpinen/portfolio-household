using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Household.Domain.Aggregates;

public sealed class Household : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    public HouseholdId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public UserId OwnerId { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Household() { }

    public static Household Create(UserId ownerId, string name, string? description, string currencyCode)
    {
        var now = DateTime.UtcNow;
        var household = new Household
        {
            Id = HouseholdId.New(),
            OwnerId = ownerId,
            Name = name,
            Description = description,
            CurrencyCode = currencyCode,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };
        household._domainEvents.Add(new HouseholdCreated(
            household.Id, ownerId, name, description, currencyCode, now));
        return household;
    }

    public void Update(string name, string? description, string currencyCode)
    {
        Name = name;
        Description = description;
        CurrencyCode = currencyCode;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdUpdated(Id, name, description, currencyCode, UpdatedAt));
    }

    public void TransferOwnership(UserId newOwnerId)
    {
        var previous = OwnerId;
        OwnerId = newOwnerId;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdOwnershipTransferred(Id, previous, newOwnerId, UpdatedAt));
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdDeleted(Id, UpdatedAt));
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
