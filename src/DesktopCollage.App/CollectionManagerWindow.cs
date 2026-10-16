using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DesktopCollage;

internal sealed class CollectionManagerWindow : Window
{
    private readonly WorkspaceStore _store = new();
    private readonly WorkspaceRecord _workspace;
    private readonly ListBox _list = new();
    private readonly Dictionary<Guid, CollectionWindow> _windows = new();

    public CollectionManagerWindow()
    {
        Title = "DesktopCollage Collections";
        Width = 520;
        Height = 390;
        MinWidth = 420;
        MinHeight = 280;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        _workspace = LoadSafe();

        var layout = new DockPanel { Margin = new Thickness(16) };
        var actions = new WrapPanel();
        DockPanel.SetDock(actions, Dock.Bottom);
        actions.Children.Add(Button("New", (_, _) => CreateCollection()));
        actions.Children.Add(Button("Rename", (_, _) => RenameCollection()));
        actions.Children.Add(Button("Delete", (_, _) => DeleteCollection()));
        actions.Children.Add(Button("Open", (_, _) => OpenCollection()));
        layout.Children.Add(actions);
        _list.DisplayMemberPath = nameof(CollectionRecord.Name);
        _list.MouseDoubleClick += (_, _) => OpenCollection();
        layout.Children.Add(_list);
        Content = layout;
        Refresh();
    }

    private WorkspaceRecord LoadSafe()
    {
        try { return _store.Load(); }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            MessageBox.Show(ex.Message, "Workspace could not be loaded");
            return new WorkspaceRecord();
        }
    }

    private static Button Button(string title, RoutedEventHandler handler)
    {
        var button = new Button { Content = title, Margin = new Thickness(0, 12, 8, 0), Padding = new Thickness(9, 6) };
        button.Click += handler;
        return button;
    }

    private void Refresh()
    {
        var selectedId = (_list.SelectedItem as CollectionRecord)?.Id;
        _list.ItemsSource = null;
        _list.ItemsSource = _workspace.Collections.ToArray();
        _list.SelectedItem = _list.Items.Cast<CollectionRecord>().FirstOrDefault(x => x.Id == selectedId);
    }

    private void Persist()
    {
        try { _store.Save(_workspace); }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        { MessageBox.Show(ex.Message, "Workspace save failed"); }
    }

    private void CreateCollection()
    {
        var name = PromptName("Collection name", "New collection");
        if (name is null) return;
        var collection = new CollectionRecord { Name = name };
        _workspace.Collections.Add(collection);
        Persist();
        Refresh();
        _list.SelectedItem = _list.Items.Cast<CollectionRecord>().FirstOrDefault(x => x.Id == collection.Id);
        OpenCollection();
    }

    private void RenameCollection()
    {
        if (_list.SelectedItem is not CollectionRecord collection) return;
        var name = PromptName("Rename collection", collection.Name);
        if (name is null) return;
        collection.Name = name;
        if (_windows.TryGetValue(collection.Id, out var window)) window.UpdateTitle();
        Persist();
        Refresh();
    }

    private void DeleteCollection()
    {
        if (_list.SelectedItem is not CollectionRecord collection) return;
        if (MessageBox.Show($"Remove collection '{collection.Name}'? Original files will not be deleted.",
            "Confirm removal", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        if (_windows.Remove(collection.Id, out var window)) window.Close();
        _workspace.Collections.RemoveAll(x => x.Id == collection.Id);
        Persist();
        Refresh();
    }

    private void OpenCollection()
    {
        if (_list.SelectedItem is not CollectionRecord collection) return;
        if (_windows.TryGetValue(collection.Id, out var existing))
        {
            existing.Activate();
            return;
        }
        var window = new CollectionWindow(collection, Persist);
        _windows[collection.Id] = window;
        window.Closed += (_, _) => _windows.Remove(collection.Id);
        window.Show();
    }

    private static string? PromptName(string title, string initial)
    {
        var dialog = new Window
        {
            Title = title, Width = 360, Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };
        var panel = new StackPanel { Margin = new Thickness(14) };
        var field = new TextBox { Text = initial, Margin = new Thickness(0, 0, 0, 12) };
        var confirm = new Button { Content = "Save", IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right, Padding = new Thickness(18, 5) };
        confirm.Click += (_, _) => dialog.DialogResult = true;
        panel.Children.Add(field);
        panel.Children.Add(confirm);
        dialog.Content = panel;
        if (dialog.ShowDialog() != true) return null;
        var result = field.Text.Trim();
        return result.Length is > 0 and <= 80 ? result : null;
    }
}
