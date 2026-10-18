using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DesktopCollage;

internal sealed class CollectionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New collection";
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public double Width { get; set; } = 460;
    public double Height { get; set; } = 320;
    public bool PositionLocked { get; set; }
    public bool SizeLocked { get; set; }
    public bool RestoreOnStartup { get; set; } = true;
    public List<string> References { get; set; } = new();
}

internal sealed class WorkspaceRecord
{
    public int Version { get; set; } = 1;
    public List<CollectionRecord> Collections { get; set; } = new();
}

internal sealed class WorkspaceStore
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopCollage", "workspace-v1.json");

    public WorkspaceRecord Load()
    {
        if (!File.Exists(_path)) return new WorkspaceRecord();
        try { return Read(_path); }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException)
        {
            var backup = _path + ".bak";
            if (!File.Exists(backup)) throw;
            // Recovery never overwrites the original file. The user can inspect it later.
            var recovered = Read(backup);
            System.Windows.MessageBox.Show(
                "The workspace could not be loaded. A backup was recovered in memory. " +
                "The original file remains untouched until you make changes.\n\n" + ex.Message,
                "DesktopCollage workspace recovery");
            return recovered;
        }
    }

    private static WorkspaceRecord Read(string path)
    {
        var workspace = JsonSerializer.Deserialize<WorkspaceRecord>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Workspace JSON was empty.");
        if (workspace.Version != 1) throw new InvalidDataException("Unsupported workspace version.");
        workspace.Collections ??= new();
        foreach (var collection in workspace.Collections)
        {
            collection.References ??= new();
            if (collection.Id == Guid.Empty) throw new InvalidDataException("Collection ID missing.");
        }
        return workspace;
    }

    public void Save(WorkspaceRecord workspace)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporary = _path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(workspace, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
        if (File.Exists(_path))
        {
            // Atomic replacement on NTFS, preserving the previous good workspace.
            File.Replace(temporary, _path, _path + ".bak");
        }
        else
        {
            File.Move(temporary, _path);
        }
    }
}
