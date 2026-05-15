using Household.Application.Commands;
using Household.Application.Repositories;
using Household.Domain.ValueObjects;

namespace Household.Application.Managers;

public interface ICalendarEventManager
{
    Task<Guid> CreateAsync(CreateCalendarEventCommand command, CancellationToken ct = default);
    Task UpdateAsync(UpdateCalendarEventCommand command, CancellationToken ct = default);
    Task DeleteAsync(DeleteCalendarEventCommand command, CancellationToken ct = default);
}

public sealed class CalendarEventManager(
    ICalendarEventRepository calendarRepo,
    IHouseholdMembershipRepository membershipRepo) : ICalendarEventManager
{
    public async Task<Guid> CreateAsync(CreateCalendarEventCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var ev = Domain.Aggregates.HouseholdCalendarEvent.Create(
            householdId,
            UserId.Create(cmd.RequestingUserId),
            cmd.Title,
            cmd.Description,
            cmd.StartsAt,
            cmd.EndsAt,
            cmd.AllDay);
        await calendarRepo.AddAsync(ev, ct);
        await calendarRepo.SaveChangesAsync(ct);
        return ev.Id.Value;
    }

    public async Task UpdateAsync(UpdateCalendarEventCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var ev = await calendarRepo.GetByIdAsync(CalendarEventId.Create(cmd.CalendarEventId), ct);
        if (ev is null) throw new KeyNotFoundException("Calendar event not found.");
        ev.Update(cmd.Title, cmd.Description, cmd.StartsAt, cmd.EndsAt, cmd.AllDay);
        await calendarRepo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DeleteCalendarEventCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var ev = await calendarRepo.GetByIdAsync(CalendarEventId.Create(cmd.CalendarEventId), ct);
        if (ev is null) throw new KeyNotFoundException("Calendar event not found.");
        ev.Delete();
        await calendarRepo.SaveChangesAsync(ct);
    }

    private async Task EnsureMemberAsync(HouseholdId householdId, Guid userId, CancellationToken ct)
    {
        var membership = await membershipRepo.GetByHouseholdAndUserAsync(householdId, UserId.Create(userId), ct);
        if (membership is null || !membership.IsActive)
            throw new UnauthorizedAccessException("User is not a member of this household.");
    }
}
