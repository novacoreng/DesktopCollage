using System.Net.Http.Json;
using System.Text.Json;

using var http = CompanionTls.Create();
Console.WriteLine("DesktopCollage local pairing review");
Console.WriteLine("This tool runs on the Windows computer only.");
try
{
    using var response = await http.GetAsync("/local/pairing/pending");
    response.EnsureSuccessStatusCode();
    using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    var requests = doc.RootElement.EnumerateArray().ToArray();
    if (requests.Length == 0) { Console.WriteLine("No pending requests."); return; }
    for (var i = 0; i < requests.Length; i++)
        Console.WriteLine($"{i + 1}. {requests[i].GetProperty("name").GetString()} | expires {requests[i].GetProperty("expiresAt").GetString()}");
    Console.Write("Choose request number (or press Enter to cancel): ");
    if (!int.TryParse(Console.ReadLine(), out var index) || index < 1 || index > requests.Length) return;
    var request = requests[index - 1];
    var id = request.GetProperty("requestId").GetString();
    Console.Write("Type APPROVE or REJECT: ");
    var decision = Console.ReadLine()?.Trim();
    if (decision == "REJECT")
    {
        using var reject = await http.DeleteAsync("/local/pairing/pending/" + Uri.EscapeDataString(id!));
        Console.WriteLine(reject.IsSuccessStatusCode ? "Request rejected." : "Rejection failed.");
        return;
    }
    if (decision != "APPROVE") return;
    Console.Write("Enter the one-time code displayed during enrollment: ");
    var code = Console.ReadLine() ?? "";
    using var approve = await http.PostAsJsonAsync("/local/pairing/approve", new { requestId = id, code });
    if (!approve.IsSuccessStatusCode) { Console.WriteLine("Invalid or expired pairing code."); return; }
    using var token = JsonDocument.Parse(await approve.Content.ReadAsStringAsync());
    Console.WriteLine("Device enrolled. Keep the token secret. This local development tool prints it once:");
    Console.WriteLine(token.RootElement.GetProperty("token").GetString());
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine("Companion service unavailable: " + ex.Message);
}
