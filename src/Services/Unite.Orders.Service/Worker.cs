using MassTransit;
using Unite.Shared.Events.Orders;

namespace Unite.Orders.Service;

/// <summary>
/// Simulates incoming customer orders by periodically publishing <see cref="OrderCreated"/>
/// integration events onto the bus, demonstrating the "mastermind/gateway" event flow:
/// other services (Inventory, Telemetry) react to these events independently.
/// </summary>
public class Worker : BackgroundService
{
    private static readonly string[] SampleCustomers = ["Ada Lovelace", "Grace Hopper", "Alan Turing", "Margaret Hamilton"];

    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;
    private readonly Random _random = new();

    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var orderCreated = new OrderCreated(
                OrderId: Guid.NewGuid(),
                CustomerName: SampleCustomers[_random.Next(SampleCustomers.Length)],
                TotalAmount: Math.Round((decimal)(_random.NextDouble() * 500), 2));

            await _bus.Publish(orderCreated, stoppingToken);

            _logger.LogInformation(
                "Published OrderCreated {OrderId} for {CustomerName} (${TotalAmount})",
                orderCreated.OrderId, orderCreated.CustomerName, orderCreated.TotalAmount);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
