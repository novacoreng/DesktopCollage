using System.Windows;

namespace DesktopCollage;

public partial class MainWindow
{
    private void OpenCollections(object sender, RoutedEventArgs e)
    {
        var manager = new CollectionManagerWindow { Owner = this };
        manager.ShowDialog();
    }
}
