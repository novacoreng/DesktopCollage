using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        var workspace = JsonSerializer.Deserialize<WorkspaceRecord>(File.ReadAllText(_path))
            ?? new WorkspaceRecord();
        if (workspace.Version != 1) throw new InvalidDataException("Unsupported workspace version.");
        workspace.Collections ??= new();
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
        File.Move(temporary, _path, true);
    }
}
