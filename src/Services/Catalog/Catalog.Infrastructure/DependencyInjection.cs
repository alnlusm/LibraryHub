using Catalog.Application.Abstractions;
using Catalog.Application.Books;
using Catalog.Infrastructure.Caching;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Messaging;
using Catalog.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CatalogDb")));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<BookService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });
                cfg.ReceiveEndpoint("catalog-order-events", e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(context);
                    e.ConfigureConsumer<OrderCancelledConsumer>(context);
                });
            });
        });

        return services;
    }
}
