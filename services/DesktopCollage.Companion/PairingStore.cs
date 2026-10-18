using System.Security.Cryptography;
using System.Text.Json;

namespace DesktopCollage.Companion;

/// <summary>Local, explicit device enrollment. Tokens are stored only as SHA-256 hashes.</summary>
internal sealed class PairingStore
{
    private readonly object _gate = new();
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopCollage", "paired-devices-v1.json");
    private readonly Dictionary<string, DeviceRecord> _devices = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PendingPair> _pending = new(StringComparer.Ordinal);

    public PairingStore()
    {
        if (!File.Exists(_path)) return;
        foreach (var item in JsonSerializer.Deserialize<List<DeviceRecord>>(File.ReadAllText(_path)) ?? new())
            _devices[item.Id] = item;
    }

    public object Start(string name)
    {
        lock (_gate)
        {
            Cleanup();
            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
            var id = Guid.NewGuid().ToString("N");
            _pending[id] = new PendingPair(name, Hash(code), DateTimeOffset.UtcNow.AddMinutes(3));
            return new { requestId = id, code, expiresInSeconds = 180,
                instruction = "The code must be confirmed by the user on the Windows computer." };
        }
    }

    public object? Approve(string requestId, string code)
    {
        lock (_gate)
        {
            Cleanup();
            if (!_pending.TryGetValue(requestId, out var pending)) return null;
            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(pending.CodeHash), Convert.FromHexString(Hash(code)))) return null;
            _pending.Remove(requestId);
            var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var device = new DeviceRecord(Guid.NewGuid().ToString("N"), pending.Name, Hash(secret), DateTimeOffset.UtcNow);
            _devices[device.Id] = device;
            Save();
            return new { deviceId = device.Id, token = secret,
                warning = "Store this token securely. It will not be shown again." };
        }
    }

    public IReadOnlyList<object> Pending()
    {
        lock (_gate)
        {
            Cleanup();
            return _pending.Select(p => (object)new
            {
                requestId = p.Key, name = p.Value.Name, expiresAt = p.Value.ExpiresAt
            }).ToArray();
        }
    }

    public bool Reject(string requestId)
    {
        lock (_gate)
        {
            Cleanup();
            return _pending.Remove(requestId);
        }
    }

    public IReadOnlyList<object> List()
    {
        lock (_gate) return _devices.Values.Select(d => (object)new
        {
            d.Id, d.Name, d.CreatedAt
        }).ToArray();
    }

    public bool Revoke(string id)
    {
        lock (_gate)
        {
            if (!_devices.Remove(id)) return false;
            Save();
            return true;
        }
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));

    private void Cleanup()
    {
        foreach (var id in _pending.Where(x => x.Value.ExpiresAt <= DateTimeOffset.UtcNow)
                     .Select(x => x.Key).ToArray()) _pending.Remove(id);
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(_devices.Values.ToArray()));
        File.Move(tmp, _path, true);
    }

    private sealed record PendingPair(string Name, string CodeHash, DateTimeOffset ExpiresAt);
    private sealed record DeviceRecord(string Id, string Name, string TokenHash, DateTimeOffset CreatedAt);
}
