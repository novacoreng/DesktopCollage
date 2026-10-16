using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopCollage;

/// <summary>An independent, movable collection window. References never move original files.</summary>
internal sealed class CollectionWindow : Window
{
    private readonly CollectionRecord _record;
    private readonly Action _save;
    private readonly ListBox _items = new();
    private readonly TextBlock _heading = new();

    public CollectionWindow(CollectionRecord record, Action save)
    {
        _record = record;
        _save = save;
        Title = record.Name;
        Width = Math.Clamp(record.Width, 240, 1600);
        Height = Math.Clamp(record.Height, 160, 1100);
        Left = record.Left;
        Top = record.Top;
        MinWidth = 240;
        MinHeight = 160;
        WindowStartupLocation = WindowStartupLocation.Manual;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.CanResize;
        AllowDrop = true;

        var border = new Border
        {
            CornerRadius = new CornerRadius(14),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromArgb(32, 30, 40, 60)),
            Padding = new Thickness(12)
        };
        var root = new DockPanel();
        var header = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(195, 55, 75, 100)),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12, 8, 12, 8),
            Cursor = Cursors.SizeAll
        };
        header.MouseLeftButtonDown += (_, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        };
        _heading.Text = record.Name;
        _heading.Foreground = Brushes.White;
        _heading.FontWeight = FontWeights.SemiBold;
        header.Child = _heading;
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var close = new Button { Content = "Close", Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(4) };
        close.Click += (_, _) => Close();
        footer.Children.Add(close);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        _items.Background = Brushes.Transparent;
        _items.Foreground = Brushes.White;
        _items.BorderThickness = new Thickness(0);
        _items.MouseDoubleClick += OpenItem;
        root.Children.Add(_items);
        border.Child = root;
        Content = border;
        Refresh();

        DragOver += OnDragOver;
        Drop += OnDrop;
        LocationChanged += (_, _) => SaveGeometry();
        SizeChanged += (_, _) => SaveGeometry();
        Closing += OnClosing;
    }

    public void UpdateTitle()
    {
        Title = _record.Name;
        _heading.Text = _record.Name;
    }

    private void Refresh() => _items.ItemsSource = _record.References
        .Select(p => new ItemEntry(p, Path.GetFileName(p.TrimEnd(Path.DirectorySeparatorChar))))
        .ToArray();

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;
        foreach (var path in paths.Where(p => File.Exists(p) || Directory.Exists(p)))
            if (!_record.References.Contains(path, StringComparer.OrdinalIgnoreCase))
                _record.References.Add(path);
        Refresh();
        _save();
        e.Handled = true;
    }

    private void OpenItem(object sender, MouseButtonEventArgs e)
    {
        if (_items.SelectedItem is not ItemEntry item) return;
        if (!File.Exists(item.Path) && !Directory.Exists(item.Path))
        {
            MessageBox.Show("Original file or folder not found.", "DesktopCollage");
            return;
        }
        try { Process.Start(new ProcessStartInfo(item.Path) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or System.Security.SecurityException)
        { MessageBox.Show(ex.Message, "Could not open item"); }
    }

    private void SaveGeometry()
    {
        if (!IsLoaded || WindowState != WindowState.Normal) return;
        if (!double.IsFinite(Left) || !double.IsFinite(Top)) return;
        _record.Left = Left;
        _record.Top = Top;
        _record.Width = ActualWidth;
        _record.Height = ActualHeight;
        _save();
    }

    private void OnClosing(object? sender, CancelEventArgs e) => SaveGeometry();

    private sealed record ItemEntry(string Path, string Name)
    {
        public override string ToString() => Name;
    }
}
