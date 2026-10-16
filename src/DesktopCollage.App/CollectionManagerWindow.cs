using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DesktopCollage;

internal sealed class CollectionManagerWindow : Window
{
    private readonly WorkspaceController _controller;
    private readonly ListBox _list = new();

    public CollectionManagerWindow(WorkspaceController controller)
    {
        _controller = controller;
        Title = "DesktopCollage Collections";
        Width = 520;
        Height = 390;
        MinWidth = 420;
        MinHeight = 280;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

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
        _list.ItemsSource = _controller.Workspace.Collections.ToArray();
        _list.SelectedItem = _list.Items.Cast<CollectionRecord>().FirstOrDefault(x => x.Id == selectedId);
    }

    private void CreateCollection()
    {
        var name = PromptName("Collection name", "New collection");
        if (name is null) return;
        var collection = new CollectionRecord { Name = name };
        _controller.Workspace.Collections.Add(collection);
        _controller.Save();
        Refresh();
        _list.SelectedItem = _list.Items.Cast<CollectionRecord>().FirstOrDefault(x => x.Id == collection.Id);
        _controller.Open(collection);
    }

    private void RenameCollection()
    {
        if (_list.SelectedItem is not CollectionRecord collection) return;
        var name = PromptName("Rename collection", collection.Name);
        if (name is null) return;
        collection.Name = name;
        _controller.Rename(collection);
        Refresh();
    }

    private void DeleteCollection()
    {
        if (_list.SelectedItem is not CollectionRecord collection) return;
        if (MessageBox.Show($"Remove collection '{collection.Name}'? Original files will not be deleted.",
            "Confirm removal", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _controller.Delete(collection);
        Refresh();
    }

    private void OpenCollection()
    {
        if (_list.SelectedItem is CollectionRecord collection) _controller.Open(collection);
    }

    private string? PromptName(string title, string initial)
    {
        var dialog = new Window
        {
            Title = title, Width = 360, Height = 160, Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
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
