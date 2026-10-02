using Unite.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddUniteObservability(serviceName: "ApiGateway");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapUniteDefaultEndpoints();

app.UseWebSockets();

app.MapGet("/", () => "Unite API Gateway");

app.MapReverseProxy();

app.Run();
