using Unite.Dashboard.Components;
using Unite.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var telemetryBaseUrl = builder.Configuration["TelemetryService:BaseUrl"] ?? "http://localhost:5311";

builder.Services.AddHttpClient<DashboardTelemetryService>(client =>
{
    client.BaseAddress = new Uri(telemetryBaseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
