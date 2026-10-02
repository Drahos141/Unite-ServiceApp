# Unite ServiceApp

A modular, domain-driven, event-based platform built on .NET 8 and Blazor. A
Blazor Server **Dashboard** gives a live overview of the backend, which is
composed of small, independent **microservices** that collaborate purely
through **events** published on a message bus. A **YARP** API Gateway fronts
the services, and every component is instrumented with **OpenTelemetry**.

## Architecture

```
                         ┌─────────────────────┐
                         │   Unite.Dashboard    │  Blazor Server UI
                         │  (SignalR client)    │  "Connected services" + live
                         └──────────┬───────────┘  event feed
                                    │ HTTP / SignalR (direct, or via Gateway)
                         ┌──────────▼───────────┐
                         │   Unite.ApiGateway    │  YARP reverse proxy
                         └──────────┬───────────┘
                                    │
                         ┌──────────▼────────────┐
                         │ Unite.Telemetry.Service│  Audits & streams every
                         │ (SignalR hub + REST)   │  integration event
                         └──────────▲────────────┘
                                    │ consumes all events
                      ┌─────────────┴─────────────┐
                      │        RabbitMQ bus        │  (MassTransit)
                      └──┬───────────────────┬─────┘
                         │                   │
             ┌───────────▼─────────┐ ┌───────▼──────────────┐
             │ Unite.Orders.Service │ │ Unite.Inventory.Service│
             │ publishes OrderCreated,│ consumes OrderCreated, │
             │ consumes InventoryReserved│ publishes InventoryReserved│
             └──────────────────────┘ └───────────────────────┘
```

Each backend microservice is fully decoupled: it only knows about the events
it publishes and consumes, never about the other services directly. This is
the "mastermind/gateway" pattern requested — the Telemetry service and
Dashboard observe the system purely by watching the event stream.

### Projects

| Project | Description |
|---|---|
| `src/Shared/Unite.Shared` | Shared integration event contracts (`IIntegrationEvent`, `OrderCreated`, `InventoryReserved`, ...) and telemetry DTOs used across services and the Dashboard. |
| `src/Shared/Unite.ServiceDefaults` | Cross-cutting bootstrapping shared by every service: OpenTelemetry tracing/metrics and MassTransit messaging (RabbitMQ, with an in-memory fallback for standalone/dev use). |
| `src/Services/Unite.Orders.Service` | Sample domain microservice. Publishes `OrderCreated` events and reacts to `InventoryReserved`. |
| `src/Services/Unite.Inventory.Service` | Sample domain microservice. Reacts to `OrderCreated` and publishes `InventoryReserved`. |
| `src/Telemetry/Unite.Telemetry.Service` | Consumes every integration event type, records it, and streams activity + connected-service status to clients over a SignalR hub (`/hubs/telemetry`) and a small REST API (`/api/activity`, `/api/services`). |
| `src/Gateway/Unite.ApiGateway` | YARP reverse proxy routing requests (including WebSocket/SignalR traffic) to backend services. |
| `src/Dashboard/Unite.Dashboard` | Blazor Server dashboard. Connects to the Telemetry service's SignalR hub and REST API to show connected services and a live event feed. |
| `tests/Unite.Shared.Tests` | xUnit tests for the shared event contracts and the Telemetry service's in-memory store. |

## Messaging and observability

* **Messaging**: [MassTransit](https://masstransit.io/) configured for
  **RabbitMQ**. Set `MessageBroker:Host` (environment variable
  `MessageBroker__Host`) to point each service at a broker; if unset, the
  service falls back to an in-memory transport, so each service can also be
  run and tested standalone without a broker.
* **API Gateway**: [YARP](https://microsoft.github.io/reverse-proxy/) routes
  configured in `src/Gateway/Unite.ApiGateway/appsettings.json`.
* **Dashboard**: Blazor Server + a typed `HubConnection` (SignalR client) that
  streams live telemetry from the Telemetry service.
* **Observability**: [OpenTelemetry](https://opentelemetry.io/) tracing and
  metrics (ASP.NET Core, HTTP client, runtime, and MassTransit instrumentation)
  exported to the console by default, and additionally to an OTLP
  endpoint (e.g. an OpenTelemetry Collector feeding Grafana, or Seq's OTLP
  ingestion) when `Observability:OtlpEndpoint` is configured.

## Running locally

1. Start a RabbitMQ broker, e.g.:
   ```bash
   docker run -d --name unite-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```
2. Run each service with the broker host configured:
   ```bash
   export MessageBroker__Host=localhost

   dotnet run --project src/Telemetry/Unite.Telemetry.Service --urls http://localhost:5311
   dotnet run --project src/Services/Unite.Orders.Service
   dotnet run --project src/Services/Unite.Inventory.Service
   dotnet run --project src/Gateway/Unite.ApiGateway --urls http://localhost:5200
   dotnet run --project src/Dashboard/Unite.Dashboard --urls http://localhost:5280
   ```
3. Open the Dashboard at `http://localhost:5280` to see connected services and
   the live event feed as the Orders/Inventory services exchange events.

## Tests

```bash
dotnet test
```
