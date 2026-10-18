using DesktopCollage.Companion;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Safe initial baseline: no network exposure and no remote command execution.
builder.WebHost.UseUrls("http://127.0.0.1:47831");
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<PairingStore>();
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

app.MapPost("/local/pairing/start", (StartPairRequest request, PairingStore store) =>
    string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 80
        ? Results.BadRequest(new { error = "A device name of 1 to 80 characters is required." })
        : Results.Ok(store.Start(request.Name.Trim())));

app.MapPost("/local/pairing/approve", (ApprovePairRequest request, PairingStore store) =>
{
    var result = store.Approve(request.RequestId, request.Code);
    return result is null ? Results.BadRequest(new { error = "Invalid or expired request." }) : Results.Ok(result);
});

app.MapGet("/local/pairing/pending", (PairingStore store) => Results.Ok(store.Pending()));
app.MapDelete("/local/pairing/pending/{id}", (string id, PairingStore store) =>
    store.Reject(id) ? Results.NoContent() : Results.NotFound());

app.MapGet("/local/devices", (PairingStore store) => Results.Ok(store.List()));
app.MapDelete("/local/devices/{id}", (string id, PairingStore store) =>
    store.Revoke(id) ? Results.NoContent() : Results.NotFound());

app.Run();

internal sealed record StartPairRequest(string Name);
internal sealed record ApprovePairRequest(string RequestId, string Code);
