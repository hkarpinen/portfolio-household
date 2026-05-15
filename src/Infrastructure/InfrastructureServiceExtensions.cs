using Household.Application.Queries;
using Household.Application.Repositories;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Persistence;
using Infrastructure.Queries;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HouseholdDbContext>(options =>
            options.UseNpgsql(
                    configuration.GetConnectionString("Household"),
                    npgsql => npgsql.MigrationsAssembly("Infrastructure"))
                .UseSnakeCaseNamingConvention());

        var rabbitConfig = configuration.GetSection("RabbitMq");
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<UserProfileUpdatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = rabbitConfig["Host"] ?? "localhost";
                cfg.Host(host, h =>
                {
                    var username = rabbitConfig["Username"];
                    var password = rabbitConfig["Password"];
                    if (!string.IsNullOrWhiteSpace(username)) h.Username(username);
                    if (!string.IsNullOrWhiteSpace(password)) h.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        services.AddScoped<IHouseholdMembershipRepository, HouseholdMembershipRepository>();
        services.AddScoped<IChoreRepository, ChoreRepository>();
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();

        services.AddScoped<IHouseholdQuery, HouseholdQuery>();
        services.AddScoped<IChoreQuery, ChoreQuery>();
        services.AddScoped<ICalendarEventQuery, CalendarEventQuery>();

        services.AddHostedService<OutboxPublisher>();

        return services;
    }
}
