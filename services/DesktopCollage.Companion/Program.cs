using System.Net;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Safe initial baseline: no network exposure and no remote command execution.
builder.WebHost.UseUrls("http://127.0.0.1:47831");
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    service = "DesktopCollage.Companion",
    status = "ready",
    protocolVersion = 1,
    remoteAccessEnabled = false
}));

app.MapGet("/api/v1/capabilities", () => Results.Ok(new
{
    pairing = false,
    applicationLaunch = false,
    keyboard = false,
    mediaControls = false,
    volumeControls = false,
    brightnessControls = false,
    note = "Capabilities are disabled until authenticated pairing is implemented."
}));

app.Run();
