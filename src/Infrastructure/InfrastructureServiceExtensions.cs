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
        services.AddScoped<DomainEventPublishingInterceptor>();
        services.AddDbContext<HouseholdDbContext>((sp, options) =>
            options.UseNpgsql(
                    configuration.GetConnectionString("Household"),
                    npgsql => npgsql.MigrationsAssembly("Infrastructure"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<DomainEventPublishingInterceptor>()));

        var rabbitConfig = configuration.GetSection("RabbitMq");
        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("household", false));

            // Replaces the hand-rolled outbox table and its polling BackgroundService. UseBusOutbox
            // routes a Publish made during SaveChanges into the outbox rather than the broker, so
            // the event commits with the aggregate and the delivery service sends it.
            x.AddEntityFrameworkOutbox<HouseholdDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            // Turns the inbox ON for every receive endpoint. AddEntityFrameworkOutbox alone sets up
            // the tables and the send side only — without this the consumers have no dedup at all.
            x.AddConfigureEndpointsCallback((context, _, cfg) =>
                cfg.UseEntityFrameworkOutbox<HouseholdDbContext>(context));

            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<UserProfileUpdatedConsumer>();
            x.AddConsumer<DemoUserCreatedConsumer>();
            x.AddConsumer<DemoUserExpiredConsumer>();
            x.AddConsumer<ExpenseCreatedConsumer>();
            x.AddConsumer<ExpenseUpdatedConsumer>();
            x.AddConsumer<ExpenseDeactivatedConsumer>();
            x.AddConsumer<ExpenseActivatedConsumer>();
            x.AddConsumer<SettlementRecordedConsumer>();

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
        services.AddScoped<IDemoQuery, DemoQuery>();
        services.AddScoped<IActivityFeedQuery, ActivityFeedQuery>();


        return services;
    }
}
