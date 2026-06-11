using Household.Domain.Aggregates;
using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Tests;

public class HouseholdCalendarEventTests
{
    private static UserId NewUserId() => UserId.Create(Guid.NewGuid());
    private static HouseholdId NewHouseholdId() => HouseholdId.New();

    private static HouseholdCalendarEvent CreateEvent() =>
        HouseholdCalendarEvent.Create(
            NewHouseholdId(), NewUserId(), "Team Meeting", null,
            DateTime.UtcNow.AddDays(1), null, false);

    [Fact]
    public void Create_SetsProperties()
    {
        var householdId = NewHouseholdId();
        var userId = NewUserId();
        var startsAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var endsAt = new DateTime(2026, 6, 1, 11, 0, 0, DateTimeKind.Utc);

        var ev = HouseholdCalendarEvent.Create(householdId, userId, "Meeting", "Weekly sync", startsAt, endsAt, false);

        Assert.Equal("Meeting", ev.Title);
        Assert.Equal("Weekly sync", ev.Description);
        Assert.Equal(householdId, ev.HouseholdId);
        Assert.Equal(userId, ev.CreatedByUserId);
        Assert.Equal(startsAt, ev.StartsAt);
        Assert.Equal(endsAt, ev.EndsAt);
        Assert.False(ev.AllDay);
        Assert.Null(ev.DeletedAt);
        Assert.Null(ev.UpdatedAt);
    }

    [Fact]
    public void Create_AllDay_SetsAllDayTrue()
    {
        var ev = HouseholdCalendarEvent.Create(
            NewHouseholdId(), NewUserId(), "Holiday", null,
            DateTime.UtcNow, null, true);

        Assert.True(ev.AllDay);
    }

    [Fact]
    public void Create_RaisesCalendarEventCreatedEvent()
    {
        var ev = CreateEvent();

        Assert.Single(ev.DomainEvents);
        Assert.IsType<CalendarEventCreated>(ev.DomainEvents.First());
    }

    [Fact]
    public void Update_ChangesProperties()
    {
        var ev = CreateEvent();
        ev.ClearDomainEvents();
        var newStart = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc);

        ev.Update("Updated Title", "New desc", newStart, null, true);

        Assert.Equal("Updated Title", ev.Title);
        Assert.Equal("New desc", ev.Description);
        Assert.Equal(newStart, ev.StartsAt);
        Assert.Null(ev.EndsAt);
        Assert.True(ev.AllDay);
        Assert.NotNull(ev.UpdatedAt);
    }

    [Fact]
    public void Update_RaisesCalendarEventUpdatedEvent()
    {
        var ev = CreateEvent();
        ev.ClearDomainEvents();

        ev.Update("New Title", null, DateTime.UtcNow, null, false);

        Assert.Single(ev.DomainEvents);
        Assert.IsType<CalendarEventUpdated>(ev.DomainEvents.First());
    }

    [Fact]
    public void Delete_SetsDeletedAt()
    {
        var ev = CreateEvent();
        ev.ClearDomainEvents();

        ev.Delete();

        Assert.NotNull(ev.DeletedAt);
    }

    [Fact]
    public void Delete_RaisesCalendarEventDeletedEvent()
    {
        var ev = CreateEvent();
        ev.ClearDomainEvents();

        ev.Delete();

        Assert.Single(ev.DomainEvents);
        Assert.IsType<CalendarEventDeleted>(ev.DomainEvents.First());
    }

    // ── Bill-path mutators are projection-only (no domain events) ─────────────
    // These mirror finance's authoritative Charge lifecycle into a local read model.
    // They must NOT raise domain events — doing so would drain them to the outbox
    // (see HouseholdDbContext.DrainDomainEventsToOutbox) and re-publish a change
    // household never authored. This guard asserts the asymmetric event contract.

    private static HouseholdCalendarEvent CreateBillEvent() =>
        HouseholdCalendarEvent.CreateFromBill(
            NewHouseholdId(), NewUserId(), Guid.NewGuid(), "Electric bill",
            DateTime.UtcNow.AddDays(3), RecurrenceFrequency.Monthly, null);

    [Fact]
    public void CreateFromBill_RaisesNoDomainEvent()
    {
        var ev = CreateBillEvent();

        Assert.Empty(ev.DomainEvents);
        Assert.Equal(CalendarEventSource.FinanceBill, ev.Source);
    }

    [Fact]
    public void UpdateFromBill_RaisesNoDomainEvent()
    {
        var ev = CreateBillEvent();

        ev.UpdateFromBill("Electric bill (revised)", DateTime.UtcNow.AddDays(5), RecurrenceFrequency.Monthly, null);

        Assert.Empty(ev.DomainEvents);
        Assert.Equal("Electric bill (revised)", ev.Title);
    }

    [Fact]
    public void DeactivateFromBill_RaisesNoDomainEvent()
    {
        var ev = CreateBillEvent();

        ev.DeactivateFromBill();

        Assert.Empty(ev.DomainEvents);
        Assert.NotNull(ev.DeletedAt);
    }

    [Fact]
    public void ActivateFromBill_RaisesNoDomainEvent()
    {
        var ev = CreateBillEvent();
        ev.DeactivateFromBill();

        ev.ActivateFromBill();

        Assert.Empty(ev.DomainEvents);
        Assert.Null(ev.DeletedAt);
    }

    [Fact]
    public void BillMutators_RejectMemberSourcedEvents()
    {
        var member = CreateEvent();

        Assert.Throws<InvalidOperationException>(() =>
            member.UpdateFromBill("x", DateTime.UtcNow, null, null));
        Assert.Throws<InvalidOperationException>(() => member.DeactivateFromBill());
        Assert.Throws<InvalidOperationException>(() => member.ActivateFromBill());
    }
}
