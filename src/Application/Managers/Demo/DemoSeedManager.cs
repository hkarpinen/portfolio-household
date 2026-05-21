using Household.Application.Repositories;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;

namespace Household.Application.Managers.Demo;

internal sealed class DemoSeedManager : IDemoSeedManager
{
    private readonly IHouseholdRepository _householdRepo;
    private readonly IHouseholdMembershipRepository _membershipRepo;
    private readonly IChoreRepository _choreRepo;
    private readonly ICalendarEventRepository _calendarRepo;

    public DemoSeedManager(
        IHouseholdRepository householdRepo,
        IHouseholdMembershipRepository membershipRepo,
        IChoreRepository choreRepo,
        ICalendarEventRepository calendarRepo)
    {
        _householdRepo = householdRepo;
        _membershipRepo = membershipRepo;
        _choreRepo = choreRepo;
        _calendarRepo = calendarRepo;
    }

    public async Task<Guid?> SeedAsync(Guid userId, string displayName, CancellationToken ct = default)
    {
        var uid = UserId.Create(userId);

        // Idempotency: skip if the user already has a household (e.g. consumer retried)
        var existing = await _membershipRepo.ListByUserAsync(uid, ct);
        if (existing.Count > 0) return null;

        var household = Domain.Aggregates.Household.Create(uid, "Demo Home", "Your demo household", "USD");
        await _householdRepo.AddAsync(household, ct);

        var membership = HouseholdMembership.Create(household.Id, uid, HouseholdRole.Owner);
        await _membershipRepo.AddAsync(membership, ct);

        var now = DateTime.UtcNow;
        var chore1 = Chore.Create(household.Id, uid, "Take out the trash", null,
            now.AddDays(2), RecurrenceFrequency.Weekly);
        var chore2 = Chore.Create(household.Id, uid, "Vacuum living room", null,
            now.AddDays(4), RecurrenceFrequency.BiWeekly);
        await _choreRepo.AddAsync(chore1, ct);
        await _choreRepo.AddAsync(chore2, ct);

        var calendarEvent = HouseholdCalendarEvent.Create(
            household.Id, uid,
            "Demo: Monthly House Meeting",
            "A recurring house meeting to discuss shared responsibilities.",
            now.AddDays(7), now.AddDays(7).AddHours(1), allDay: false);
        await _calendarRepo.AddAsync(calendarEvent, ct);

        await _householdRepo.SaveChangesAsync(ct);
        return household.Id.Value;
    }

    public async Task CleanupAsync(Guid userId, CancellationToken ct = default)
    {
        var uid = UserId.Create(userId);
        var households = await _householdRepo.ListByOwnerAsync(uid, ct);
        var householdIds = households.Select(h => h.Id).ToList();

        if (householdIds.Count > 0)
        {
            await _choreRepo.DeleteByHouseholdIdsAsync(householdIds, ct);
            await _calendarRepo.DeleteByHouseholdIdsAsync(householdIds, ct);
        }

        await _membershipRepo.DeleteByUserAsync(uid, ct);
        await _householdRepo.DeleteByOwnerAsync(uid, ct);
    }
}
