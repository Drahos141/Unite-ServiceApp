using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Unite.Shared.Events;
using Unite.Shared.Telemetry;

namespace Unite.Telemetry.Service;

/// <summary>
/// Generic MassTransit consumer that records every integration event of type
/// <typeparamref name="TEvent"/> into the <see cref="TelemetryStore"/> and broadcasts
/// it to connected Dashboard clients over SignalR. Registering this consumer for
/// every concrete event type lets the Telemetry service audit the entire system's
/// event flow without each producing service needing any telemetry-specific code.
/// </summary>
public sealed class IntegrationEventAuditConsumer<TEvent> : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
{
    private readonly TelemetryStore _store;
    private readonly IHubContext<TelemetryHub, ITelemetryHubClient> _hubContext;

    public IntegrationEventAuditConsumer(TelemetryStore store, IHubContext<TelemetryHub, ITelemetryHubClient> hubContext)
    {
        _store = store;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var activity = _store.Record(context.Message);
        var status = _store.GetServiceStatus(activity.SourceService);

        await _hubContext.Clients.All.EventRecorded(activity);
        await _hubContext.Clients.All.ServiceStatusChanged(status);
    }
}
