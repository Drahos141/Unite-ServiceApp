namespace Unite.Shared.Events.Inventory;

/// <summary>Raised by the Inventory service when stock has been reserved for an order.</summary>
public sealed record InventoryReserved(
    Guid OrderId,
    bool Success,
    string? Reason) : IntegrationEvent(SourceService: "Inventory");
