using Household.Application.Managers;
using Household.Application.Managers.Demo;
using Household.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Household.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHouseholdManager, HouseholdManager>();
        services.AddScoped<IMembershipManager, MembershipManager>();
        services.AddScoped<IChoreManager, ChoreManager>();
        services.AddScoped<ICalendarEventManager, CalendarEventManager>();
        services.AddScoped<IDemoSeedManager, DemoSeedManager>();
        return services;
    }
}
