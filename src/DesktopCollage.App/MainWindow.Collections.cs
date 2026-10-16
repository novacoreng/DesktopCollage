using System.Windows;

namespace DesktopCollage;

public partial class MainWindow
{
    private void OpenCollections(object sender, RoutedEventArgs e)
    {
        WorkspaceController.Current.ShowManager(this);
    }
}
