namespace Unite.Shared.Events.Orders;

/// <summary>Raised by the Orders service when a new order has been placed.</summary>
public sealed record OrderCreated(
    Guid OrderId,
    string CustomerName,
    decimal TotalAmount) : IntegrationEvent(SourceService: "Orders");

/// <summary>Raised by the Orders service once an order has been fully confirmed.</summary>
public sealed record OrderConfirmed(
    Guid OrderId) : IntegrationEvent(SourceService: "Orders");
