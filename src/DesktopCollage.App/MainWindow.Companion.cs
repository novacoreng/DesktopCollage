using System.Windows;

namespace DesktopCollage;

public partial class MainWindow
{
    private bool _companionEnabled;

    private void ToggleCompanion(object sender, RoutedEventArgs e)
    {
        _companionEnabled = !_companionEnabled;
        ShowInTaskbar = !_companionEnabled;
        if (_companionEnabled && !DesktopCompanionMode.SendToBottom(this))
        {
            _companionEnabled = false;
            ShowInTaskbar = true;
            MessageBox.Show("Could not place the prototype at the bottom of the window stack.",
                "DesktopCollage");
        }
        CompanionButton.Content = _companionEnabled ? "Normal window" : "Desktop mode (test)";
    }

    private void OnCompanionActivated(object? sender, System.EventArgs e)
    {
        if (_companionEnabled) DesktopCompanionMode.SendToBottom(this);
    }
}
