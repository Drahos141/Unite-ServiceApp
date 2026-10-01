using Unite.Inventory.Service;
using Unite.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddUniteObservability(serviceName: "Inventory");
builder.AddUniteMessaging(x => x.AddConsumer<OrderCreatedConsumer>());

var host = builder.Build();
host.Run();
