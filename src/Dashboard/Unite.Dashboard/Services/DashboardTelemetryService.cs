using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Unite.Shared.Telemetry;

namespace Unite.Dashboard.Services;

/// <summary>
/// Connects to the Telemetry service's SignalR hub and REST endpoints, maintaining
/// the live "connected services" overview and recent event activity feed shown on
/// the Dashboard's Overview page.
/// </summary>
public sealed class DashboardTelemetryService : IAsyncDisposable
{
    private const int MaxActivityItems = 100;

    private readonly HttpClient _httpClient;
    private readonly HubConnection _hubConnection;
    private readonly List<EventActivityRecord> _activity = new();
    private readonly Dictionary<string, ServiceStatusSnapshot> _services = new();
    private Task? _startTask;

    public DashboardTelemetryService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var telemetryBaseUrl = configuration["TelemetryService:BaseUrl"] ?? "http://localhost:5311";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{telemetryBaseUrl.TrimEnd('/')}{TelemetryHubRoute.Path}")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<EventActivityRecord>("EventRecorded", activity =>
        {
            _activity.Insert(0, activity);
            if (_activity.Count > MaxActivityItems)
            {
                _activity.RemoveAt(_activity.Count - 1);
            }

            OnChanged?.Invoke();
        });

        _hubConnection.On<ServiceStatusSnapshot>("ServiceStatusChanged", status =>
        {
            _services[status.ServiceName] = status;
            OnChanged?.Invoke();
        });
    }

    /// <summary>Raised whenever new telemetry data arrives so the UI can re-render.</summary>
    public event Action? OnChanged;

    public IReadOnlyList<EventActivityRecord> RecentActivity => _activity;

    public IReadOnlyCollection<ServiceStatusSnapshot> ConnectedServices => _services.Values;

    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

    public async Task InitializeAsync()
    {
        _startTask ??= StartAsync();
        await _startTask;
    }

    private async Task StartAsync()
    {
        try
        {
            var activity = await _httpClient.GetFromJsonAsync<EventActivityRecord[]>("api/activity");
            if (activity is not null)
            {
                _activity.AddRange(activity.OrderByDescending(a => a.RecordedAtUtc));
            }

            var services = await _httpClient.GetFromJsonAsync<ServiceStatusSnapshot[]>("api/services");
            if (services is not null)
            {
                foreach (var service in services)
                {
                    _services[service.ServiceName] = service;
                }
            }
        }
        catch (HttpRequestException)
        {
            // Telemetry service may not be reachable yet (e.g. still starting up); the
            // SignalR connection below will keep retrying and the UI simply shows no data yet.
        }

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception)
        {
            // Allow the page to render even if the Telemetry service isn't reachable yet.
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }
}
