using Household.Domain.Events;
using Household.Domain.ValueObjects;
using HouseholdModel = Household.Domain.Aggregates.Household;

namespace Tests;

public class HouseholdTests
{
    private static UserId NewUserId() => UserId.Create(Guid.NewGuid());

    [Fact]
    public void Create_SetsProperties()
    {
        var ownerId = NewUserId();

        var household = HouseholdModel.Create(ownerId, "My Home", "A nice place", "USD");

        Assert.Equal("My Home", household.Name);
        Assert.Equal("A nice place", household.Description);
        Assert.Equal("USD", household.CurrencyCode);
        Assert.Equal(ownerId, household.OwnerId);
        Assert.True(household.IsActive);
    }

    [Fact]
    public void Create_RaisesHouseholdCreatedEvent()
    {
        var household = HouseholdModel.Create(NewUserId(), "My Home", null, "USD");

        Assert.Single(household.DomainEvents);
        Assert.IsType<HouseholdCreated>(household.DomainEvents.First());
    }

    [Fact]
    public void Update_ChangesNameDescriptionAndCurrency()
    {
        var household = HouseholdModel.Create(NewUserId(), "Old Name", "Old desc", "USD");
        household.ClearDomainEvents();

        household.Update("New Name", "New desc", "EUR");

        Assert.Equal("New Name", household.Name);
        Assert.Equal("New desc", household.Description);
        Assert.Equal("EUR", household.CurrencyCode);
    }

    [Fact]
    public void Update_RaisesHouseholdUpdatedEvent()
    {
        var household = HouseholdModel.Create(NewUserId(), "Name", null, "USD");
        household.ClearDomainEvents();

        household.Update("New Name", null, "USD");

        Assert.Single(household.DomainEvents);
        Assert.IsType<HouseholdUpdated>(household.DomainEvents.First());
    }

    [Fact]
    public void TransferOwnership_ChangesOwner()
    {
        var household = HouseholdModel.Create(NewUserId(), "Name", null, "USD");
        var newOwner = NewUserId();
        household.ClearDomainEvents();

        household.TransferOwnership(newOwner);

        Assert.Equal(newOwner, household.OwnerId);
    }

    [Fact]
    public void TransferOwnership_RaisesOwnershipTransferredEvent()
    {
        var household = HouseholdModel.Create(NewUserId(), "Name", null, "USD");
        household.ClearDomainEvents();

        household.TransferOwnership(NewUserId());

        Assert.Single(household.DomainEvents);
        Assert.IsType<HouseholdOwnershipTransferred>(household.DomainEvents.First());
    }

    [Fact]
    public void Delete_SetsIsActiveFalse()
    {
        var household = HouseholdModel.Create(NewUserId(), "Name", null, "USD");
        household.ClearDomainEvents();

        household.Delete();

        Assert.False(household.IsActive);
    }

    [Fact]
    public void Delete_RaisesHouseholdDeletedEvent()
    {
        var household = HouseholdModel.Create(NewUserId(), "Name", null, "USD");
        household.ClearDomainEvents();

        household.Delete();

        Assert.Single(household.DomainEvents);
        Assert.IsType<HouseholdDeleted>(household.DomainEvents.First());
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var household = HouseholdModel.Create(NewUserId(), "Name", null, "USD");
        Assert.NotEmpty(household.DomainEvents);

        household.ClearDomainEvents();

        Assert.Empty(household.DomainEvents);
    }
}
