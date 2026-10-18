using System.Windows;

namespace DesktopCollage;

public partial class MainWindow
{
    private PairingApprovalWindow? _pairingWindow;

    private void OpenPairing(object sender, RoutedEventArgs e)
    {
        if (_pairingWindow is not null)
        {
            _pairingWindow.Activate();
            return;
        }
        _pairingWindow = new PairingApprovalWindow { Owner = this };
        _pairingWindow.Closed += (_, _) => _pairingWindow = null;
        _pairingWindow.Show();
    }
}
