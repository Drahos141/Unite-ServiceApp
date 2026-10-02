using MassTransit;
using Unite.Shared.Events.Inventory;
using Unite.Shared.Events.Orders;
using Unite.Shared.Telemetry;
using Unite.Telemetry.Service;
using Unite.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TelemetryStore>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy => policy
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.AddUniteObservability(serviceName: "Telemetry");
builder.AddUniteMessaging(x =>
{
    x.AddConsumer<IntegrationEventAuditConsumer<OrderCreated>>();
    x.AddConsumer<IntegrationEventAuditConsumer<OrderConfirmed>>();
    x.AddConsumer<IntegrationEventAuditConsumer<InventoryReserved>>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Dashboard");
app.MapUniteDefaultEndpoints();

app.MapGet("/api/activity", (TelemetryStore store) => store.GetRecentActivity())
    .WithName("GetRecentActivity");

app.MapGet("/api/services", (TelemetryStore store) => store.GetServiceStatuses())
    .WithName("GetServiceStatuses");

app.MapHub<TelemetryHub>(TelemetryHubRoute.Path);

app.Run();

public partial class Program;
