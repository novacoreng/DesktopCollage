using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace DesktopCollage;

/// <summary>One workspace and window registry per application process.</summary>
internal sealed class WorkspaceController
{
    public static WorkspaceController Current { get; } = new();

    private readonly WorkspaceStore _store = new();
    private readonly Dictionary<Guid, CollectionWindow> _openWindows = new();
    private CollectionManagerWindow? _manager;

    public WorkspaceRecord Workspace { get; }

    private WorkspaceController()
    {
        try { Workspace = _store.Load(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            MessageBox.Show($"Could not load workspace. Existing file was not overwritten: {ex.Message}",
                "DesktopCollage");
            Workspace = new WorkspaceRecord();
        }
    }

    private bool _restored;

    public void RestoreCollections()
    {
        if (_restored) return;
        _restored = true;
        foreach (var collection in Workspace.Collections)
        {
            if (!collection.RestoreOnStartup) continue;
            try { Open(collection, true); }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Could not restore collection");
            }
        }
    }

    public void ShowManager(Window owner)
    {
        if (_manager is not null)
        {
            _manager.Activate();
            return;
        }
        _manager = new CollectionManagerWindow(this) { Owner = owner };
        _manager.Closed += (_, _) => _manager = null;
        _manager.Show();
    }

    public void Open(CollectionRecord collection) => Open(collection, false);

    private void Open(CollectionRecord collection, bool restoring)
    {
        if (_openWindows.TryGetValue(collection.Id, out var existing))
        {
            existing.Activate();
            return;
        }
        var window = new CollectionWindow(collection, Save);
        _openWindows.Add(collection.Id, window);
        window.Closed += (_, _) =>
        {
            _openWindows.Remove(collection.Id);
            if (Application.Current.Dispatcher.HasShutdownStarted || Application.Current.Dispatcher.HasShutdownFinished) return;
            collection.RestoreOnStartup = false;
            Save();
        };
        window.Show();
        if (!restoring)
        {
            collection.RestoreOnStartup = true;
            Save();
        }
    }

    public void Rename(CollectionRecord collection)
    {
        if (_openWindows.TryGetValue(collection.Id, out var window)) window.UpdateTitle();
        Save();
    }

    public void Delete(CollectionRecord collection)
    {
        if (_openWindows.Remove(collection.Id, out var window)) window.Close();
        Workspace.Collections.RemoveAll(x => x.Id == collection.Id);
        Save();
    }

    public void Save()
    {
        try { _store.Save(Workspace); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { MessageBox.Show(ex.Message, "Workspace save failed"); }
    }
}
