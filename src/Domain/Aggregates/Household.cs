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
    /// <summary>IANA timezone identifier (e.g. "America/Los_Angeles"). Defaults to UTC.</summary>
    public string Timezone { get; private set; } = "UTC";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Household() { }

    public static Household Create(UserId ownerId, string name, string? description, string currencyCode, string? timezone = null)
    {
        var now = DateTime.UtcNow;
        var household = new Household
        {
            Id = HouseholdId.New(),
            OwnerId = ownerId,
            Name = name,
            Description = description,
            CurrencyCode = currencyCode,
            Timezone = string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };
        household._domainEvents.Add(new HouseholdCreated(
            household.Id.Value, ownerId.Value, name, description, currencyCode, now));
        return household;
    }

    public void Update(string name, string? description, string currencyCode, string? timezone = null)
    {
        Name = name;
        Description = description;
        CurrencyCode = currencyCode;
        if (!string.IsNullOrWhiteSpace(timezone))
            Timezone = timezone;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdUpdated(Id.Value, name, description, currencyCode, UpdatedAt));
    }

    public void TransferOwnership(UserId newOwnerId)
    {
        var previous = OwnerId;
        OwnerId = newOwnerId;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdOwnershipTransferred(Id.Value, previous.Value, newOwnerId.Value, UpdatedAt));
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdDeleted(Id.Value, UpdatedAt));
    }

    /// <summary>
    /// Authorize a member's allocation (their share) on a finance charge and emit the
    /// authoritative fact. The Household aggregate owns role-authorization, so this fact
    /// belongs here — not on a membership aggregate that owns none of its state. The caller
    /// (MembershipManager) performs the role check against the requester's membership before
    /// invoking this; <paramref name="forUserId"/> is the member the share is for and is
    /// authoritative on the wire (household has already verified the requester may act for them).
    /// Pure event — no household state changes; it drains to the outbox on save and finance
    /// consumes <see cref="GroupAllocationAssigned"/> to upsert the allocation. No service-to-service call.
    /// </summary>
    public void AssignAllocation(Guid chargeId, UserId forUserId, decimal amount, string currency)
    {
        _domainEvents.Add(new GroupAllocationAssigned(
            Guid.NewGuid(), DateTime.UtcNow, Id.Value, chargeId, forUserId.Value, amount, currency));
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
