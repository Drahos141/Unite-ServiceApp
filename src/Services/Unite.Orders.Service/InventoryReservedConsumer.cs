using MassTransit;
using Unite.Shared.Events.Inventory;
using Unite.Shared.Events.Orders;

namespace Unite.Orders.Service;

/// <summary>
/// Reacts to the Inventory service's reservation outcome and, on success, confirms
/// the order by publishing <see cref="OrderConfirmed"/>.
/// </summary>
public sealed class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly ILogger<InventoryReservedConsumer> _logger;

    public InventoryReservedConsumer(ILogger<InventoryReservedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        var message = context.Message;

        if (!message.Success)
        {
            _logger.LogWarning("Inventory reservation failed for order {OrderId}: {Reason}", message.OrderId, message.Reason);
            return;
        }

        await context.Publish(new OrderConfirmed(message.OrderId));
        _logger.LogInformation("Order {OrderId} confirmed", message.OrderId);
    }
}
