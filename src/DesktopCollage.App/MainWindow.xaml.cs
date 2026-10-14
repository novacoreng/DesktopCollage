using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace DesktopCollage;
public partial class MainWindow : Window
{
    private readonly List<string> _references = new();
    private readonly string _settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopCollage", "prototype-layout.json");
    public MainWindow() => InitializeComponent();
    private void DragHeader(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    private void CloseClick(object sender, RoutedEventArgs e) => Close();
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }
    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;
        foreach (var p in paths.Where(p => File.Exists(p) || Directory.Exists(p)))
            if (!_references.Contains(p, StringComparer.OrdinalIgnoreCase)) _references.Add(p);
        RefreshItems();
        Save();
        e.Handled = true;
    }
    private void OpenItem(object sender, MouseButtonEventArgs e)
    {
        if (Items.SelectedItem is not ItemEntry item) return;
        if (!File.Exists(item.Path) && !Directory.Exists(item.Path)) { MessageBox.Show("Original item not found."); return; }
        try { Process.Start(new ProcessStartInfo(item.Path) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or System.Security.SecurityException) { MessageBox.Show(ex.Message, "Could not open item"); }
    }
    private void RefreshItems() => Items.ItemsSource = _references.Select(p => new ItemEntry(p, Path.GetFileName(p.TrimEnd(Path.DirectorySeparatorChar)))).ToArray();
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try {
            if (!File.Exists(_settingsFile)) return;
            var s = JsonSerializer.Deserialize<PrototypeState>(File.ReadAllText(_settingsFile));
            if (s is null) return;
            Width = Math.Clamp(s.Width, MinWidth, 1400);
            Height = Math.Clamp(s.Height, MinHeight, 1000);
            var w = SystemParameters.WorkArea;
            Left = Math.Clamp(s.Left, w.Left, Math.Max(w.Left, w.Right - 80));
            Top = Math.Clamp(s.Top, w.Top, Math.Max(w.Top, w.Bottom - 50));
            foreach (var p in s.References ?? Array.Empty<string>()) if (!string.IsNullOrWhiteSpace(p) && !_references.Contains(p, StringComparer.OrdinalIgnoreCase)) _references.Add(p);
            RefreshItems();
        } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException) { MessageBox.Show(ex.Message, "Restore failed"); }
    }
    private void Save()
    {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFile)!);
            var temp = _settingsFile + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(new PrototypeState(Left, Top, Width, Height, _references.ToArray())));
            File.Move(temp, _settingsFile, true);
        } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { MessageBox.Show(ex.Message, "Save failed"); }
    }
    private void OnClosing(object? sender, CancelEventArgs e) => Save();
    private sealed record ItemEntry(string Path, string Name) { public override string ToString() => Name; }
    private sealed record PrototypeState(double Left, double Top, double Width, double Height, string[]? References);
}