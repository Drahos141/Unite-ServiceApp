using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Unite.ServiceDefaults;

/// <summary>
/// Cross-cutting bootstrapping shared by every backend microservice in the system:
/// OpenTelemetry tracing/metrics and MassTransit messaging wired to RabbitMQ (or, when
/// no broker is configured, an in-memory transport so each service can run standalone
/// for local development and testing).
/// </summary>
public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// Registers OpenTelemetry instrumentation (ASP.NET Core, HTTP client, runtime
    /// metrics) exporting to the console and, when configured, to an OTLP collector
    /// (e.g. feeding Grafana/Seq) so every service's event flow can be observed.
    /// </summary>
    public static IHostApplicationBuilder AddUniteObservability(this IHostApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];

        builder.Services.AddHealthChecks();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("MassTransit")
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return builder;
    }

    /// <summary>
    /// Registers MassTransit for this service. When a <c>MessageBroker:Host</c>
    /// configuration value is present the RabbitMQ transport is used; otherwise the
    /// service falls back to MassTransit's in-memory transport, which is convenient
    /// for local development and automated tests that don't have a broker available.
    /// </summary>
    public static IHostApplicationBuilder AddUniteMessaging(
        this IHostApplicationBuilder builder,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var brokerOptions = builder.Configuration.GetSection("MessageBroker").Get<MessageBrokerOptions>()
                            ?? new MessageBrokerOptions();

        builder.Services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            if (!string.IsNullOrWhiteSpace(brokerOptions.Host))
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(brokerOptions.Host, brokerOptions.VirtualHost, h =>
                    {
                        h.Username(brokerOptions.Username);
                        h.Password(brokerOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
        });

        return builder;
    }

    /// <summary>Maps the standard liveness/readiness health check endpoint for this service.</summary>
    public static WebApplication MapUniteDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}

/// <summary>Bound from the <c>MessageBroker</c> configuration section.</summary>
public sealed class MessageBrokerOptions
{
    public string? Host { get; set; }

    public string VirtualHost { get; set; } = "/";

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";
}
