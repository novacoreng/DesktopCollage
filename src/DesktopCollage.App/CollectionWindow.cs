using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopCollage;

/// <summary>Movable glass collection. Only references files; never relocates originals.</summary>
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
        Width = Math.Clamp(record.Width, 280, 1600);
        Height = Math.Clamp(record.Height, 200, 1100);
        Left = record.Left;
        Top = record.Top;
        MinWidth = 280;
        MinHeight = 200;
        WindowStartupLocation = WindowStartupLocation.Manual;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.CanResize;
        AllowDrop = true;

        var border = new Border
        {
            CornerRadius = new CornerRadius(20),
            BorderBrush = new SolidColorBrush(Color.FromArgb(115, 160, 220, 205)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromArgb(215, 16, 24, 32)),
            Padding = new Thickness(14)
        };
        var root = new DockPanel();
        var header = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(105, 38, 77, 74)),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14, 10, 14, 10),
            Cursor = Cursors.SizeAll
        };
        header.MouseLeftButtonDown += (_, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        };
        _heading.Text = record.Name;
        _heading.FontSize = 15;
        _heading.Foreground = Brushes.White;
        _heading.FontWeight = FontWeights.SemiBold;
        header.Child = _heading;
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var footer = new DockPanel { Margin = new Thickness(0, 10, 0, 0), LastChildFill = false };
        var hint = new TextBlock
        {
            Text = "Drop shortcuts, files or folders",
            Foreground = new SolidColorBrush(Color.FromRgb(147, 229, 182)),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11
        };
        DockPanel.SetDock(hint, Dock.Left);
        footer.Children.Add(hint);
        var close = new Button { Content = "Close", Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(5, 0, 5, 0) };
        close.Click += (_, _) => Close();
        DockPanel.SetDock(close, Dock.Right);
        footer.Children.Add(close);
        var grip = new Thumb
        {
            Width = 22, Height = 22, Cursor = Cursors.SizeNWSE,
            Background = new SolidColorBrush(Color.FromArgb(130, 120, 169, 244)),
            ToolTip = "Resize collection"
        };
        grip.DragDelta += (_, e) =>
        {
            Width = Math.Max(MinWidth, Width + e.HorizontalChange);
            Height = Math.Max(MinHeight, Height + e.VerticalChange);
        };
        DockPanel.SetDock(grip, Dock.Right);
        footer.Children.Add(grip);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var tileStyle = new Style(typeof(ListBoxItem));
        tileStyle.Setters.Add(new Setter(FrameworkElement.WidthProperty, 98.0));
        tileStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 104.0));
        tileStyle.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(5)));
        tileStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        tileStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        _items.ItemContainerStyle = tileStyle;

        var tile = new FrameworkElementFactory(typeof(StackPanel));
        tile.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
        tile.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);

        var icon = new FrameworkElementFactory(typeof(Image));
        icon.SetValue(FrameworkElement.WidthProperty, 48.0);
        icon.SetValue(FrameworkElement.HeightProperty, 48.0);
        icon.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 6, 0, 6));
        icon.SetBinding(Image.SourceProperty, new Binding(nameof(ItemEntry.Path))
        {
            Converter = new ShellIconConverter()
        });
        tile.AppendChild(icon);

        var label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemEntry.Name)));
        label.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
        label.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        label.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        label.SetValue(FrameworkElement.MaxHeightProperty, 40.0);
        label.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        label.SetValue(TextBlock.FontSizeProperty, 11.0);
        tile.AppendChild(label);
        _items.ItemTemplate = new DataTemplate { VisualTree = tile };

        var wrap = new FrameworkElementFactory(typeof(WrapPanel));
        wrap.SetValue(WrapPanel.IsItemsHostProperty, true);
        _items.ItemsPanel = new ItemsPanelTemplate(wrap);
        _items.HorizontalContentAlignment = HorizontalAlignment.Center;
        _items.Background = Brushes.Transparent;
        _items.Foreground = Brushes.White;
        _items.BorderThickness = new Thickness(0);
        _items.Margin = new Thickness(0, 10, 0, 0);
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
