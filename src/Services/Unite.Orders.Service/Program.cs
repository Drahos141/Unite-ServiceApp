using Unite.Orders.Service;
using Unite.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddUniteObservability(serviceName: "Orders");
builder.AddUniteMessaging(x => x.AddConsumer<InventoryReservedConsumer>());

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
