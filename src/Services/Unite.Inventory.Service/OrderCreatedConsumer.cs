using MassTransit;
using Unite.Shared.Events.Inventory;
using Unite.Shared.Events.Orders;

namespace Unite.Inventory.Service;

/// <summary>
/// Reacts to new orders by simulating a stock reservation check and publishing the
/// outcome as <see cref="InventoryReserved"/>, demonstrating decoupled, event-driven
/// collaboration between microservices (no direct service-to-service calls).
/// </summary>
public sealed class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly Random _random = new();

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var order = context.Message;

        // Simulate stock availability: ~90% of orders can be fulfilled.
        var success = _random.NextDouble() > 0.1;

        _logger.LogInformation(
            "Processed reservation for order {OrderId} ({CustomerName}): {Outcome}",
            order.OrderId, order.CustomerName, success ? "reserved" : "out of stock");

        await context.Publish(new InventoryReserved(
            order.OrderId,
            success,
            success ? null : "Insufficient stock"));
    }
}
