using Microsoft.AspNetCore.SignalR;
using Unite.Shared.Telemetry;

namespace Unite.Telemetry.Service;

/// <summary>
/// SignalR hub the Dashboard connects to for a live feed of integration event
/// activity and connected-service status, backing the "overview of what is going on
/// in the backend" requirement.
/// </summary>
public sealed class TelemetryHub : Hub<ITelemetryHubClient>
{
}
