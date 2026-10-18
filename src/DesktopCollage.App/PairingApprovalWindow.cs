using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DesktopCollage;

/// <summary>Local-only developer pairing review UI. Not a production security boundary.</summary>
internal sealed class PairingApprovalWindow : Window
{
    private readonly HttpClient _client = new() { BaseAddress = new Uri("http://127.0.0.1:47831") };
    private readonly ListBox _requests = new();
    private readonly TextBox _code = new();
    private readonly TextBlock _status = new();
    private string[] _ids = Array.Empty<string>();

    public PairingApprovalWindow()
    {
        Title = "DesktopCollage | Pairing review";
        Width = 510;
        Height = 390;
        MinWidth = 430;
        MinHeight = 340;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(16, 24, 32));
        Foreground = Brushes.White;

        var root = new StackPanel { Margin = new Thickness(22) };
        root.Children.Add(new TextBlock
        {
            Text = "PAIR A DEVICE", FontSize = 20, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(147, 229, 182)),
            Margin = new Thickness(0, 0, 0, 8)
        });
        root.Children.Add(new TextBlock
        {
            Text = "Review local enrollment requests. Remote Android pairing is not yet enabled.",
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 14)
        });
        _requests.Height = 100;
        root.Children.Add(_requests);
        root.Children.Add(new TextBlock
        {
            Text = "One-time code", Margin = new Thickness(0, 12, 0, 4)
        });
        _code.MaxLength = 8;
        root.Children.Add(_code);
        var actions = new WrapPanel { Margin = new Thickness(0, 14, 0, 8) };
        var refresh = MakeButton("Refresh");
        refresh.Click += async (_, _) => await RefreshAsync();
        actions.Children.Add(refresh);
        var approve = MakeButton("Approve");
        approve.Click += async (_, _) => await DecideAsync(true);
        actions.Children.Add(approve);
        var reject = MakeButton("Reject");
        reject.Click += async (_, _) => await DecideAsync(false);
        actions.Children.Add(reject);
        root.Children.Add(actions);
        _status.TextWrapping = TextWrapping.Wrap;
        root.Children.Add(_status);
        Content = root;
        Loaded += async (_, _) => await RefreshAsync();
        Closed += (_, _) => _client.Dispose();
    }

    private static Button MakeButton(string title) => new()
    {
        Content = title, Padding = new Thickness(12, 7, 12, 7),
        Margin = new Thickness(0, 0, 9, 0),
        Background = new SolidColorBrush(Color.FromRgb(38, 79, 72)),
        Foreground = Brushes.White
    };

    private async System.Threading.Tasks.Task RefreshAsync()
    {
        try
        {
            using var response = await _client.GetAsync("/local/pairing/pending");
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var requests = json.RootElement.EnumerateArray().ToArray();
            _ids = requests.Select(x => x.GetProperty("requestId").GetString() ?? "").ToArray();
            _requests.ItemsSource = requests.Select(x => x.GetProperty("name").GetString() ?? "Unnamed device").ToArray();
            _status.Text = requests.Length == 0 ? "No pending requests." : "Select a request to review.";
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            _status.Text = "Companion service unavailable: " + ex.Message;
        }
    }

    private async System.Threading.Tasks.Task DecideAsync(bool approve)
    {
        var index = _requests.SelectedIndex;
        if (index < 0 || index >= _ids.Length) { _status.Text = "Select a request first."; return; }
        if (approve && _code.Text.Length != 8) { _status.Text = "Enter the eight-character code."; return; }
        if (approve && MessageBox.Show(
                "Approve this local development device? This does not enable remote control.",
                "Confirm enrollment", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        try
        {
            if (approve)
            {
                using var response = await _client.PostAsJsonAsync("/local/pairing/approve",
                    new { requestId = _ids[index], code = _code.Text });
                if (!response.IsSuccessStatusCode) { _status.Text = "Invalid or expired pairing code."; return; }
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var deviceId = json.RootElement.GetProperty("deviceId").GetString();
                _status.Text = "Device enrolled: " + deviceId + ". Token is not displayed here. Android pairing is not active.";
            }
            else
            {
                using var response = await _client.DeleteAsync("/local/pairing/pending/" + Uri.EscapeDataString(_ids[index]));
                _status.Text = response.IsSuccessStatusCode ? "Request rejected." : "Unable to reject request.";
            }
            _code.Clear();
            await RefreshAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            _status.Text = ex.Message;
        }
    }
}
