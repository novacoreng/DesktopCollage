using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace DesktopCollage;

public partial class MainWindow
{
    private void InspectExplorer(object sender, RoutedEventArgs e)
    {
        try { MessageBox.Show(ShellProbe.GetReport(), "Explorer inspection"); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Explorer inspection failed"); }
    }

    private void ResizeDelta(object sender, DragDeltaEventArgs e)
    {
        Width = Math.Max(MinWidth, Width + e.HorizontalChange);
        Height = Math.Max(MinHeight, Height + e.VerticalChange);
    }
}
