using System;
using System.Windows;
using System.Windows.Input;

namespace DesktopCollage;

public partial class MainWindow
{
    private bool _companionEnabled;

    private void ToggleCompanion(object sender, RoutedEventArgs e)
    {
        if (_companionEnabled) { RestoreNormalMode(); return; }

        // Keep the taskbar entry available so the user can always recover the window.
        if (!DesktopCompanionMode.SendToBottom(this))
        {
            MessageBox.Show("Could not place the prototype at the bottom of the window stack.",
                "DesktopCollage");
            return;
        }

        _companionEnabled = true;
        CompanionButton.Content = "Normal window";
    }

    private void RestoreNormalMode()
    {
        _companionEnabled = false;
        CompanionButton.Content = "Desktop mode (test)";
        ShowInTaskbar = true;
        WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private void OnCompanionActivated(object? sender, EventArgs e)
    {
        // Never immediately send to bottom on activation: doing so would
        // prevent users from accessing the Restore button via the taskbar.
    }

    private void RestoreShortcut(object sender, ExecutedRoutedEventArgs e) => RestoreNormalMode();
}
