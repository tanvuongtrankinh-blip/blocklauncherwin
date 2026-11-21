using System.Text.Json;

namespace BlockLauncher.Core;

/// <summary>JSON-file backed storage for ModPE.saveData / readData.</summary>
public sealed class FileDataStore : IDataStore
{
    private readonly string _path;
    private readonly Dictionary<string, string> _d;

    public FileDataStore(string path)
    {
        _path = path;
        try
        {
            _d = File.Exists(path)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? new()
                : new();
        }
        catch { _d = new(); }
    }

    public string? Read(string key) => _d.TryGetValue(key, out var v) ? v : null;

    public void Write(string key, string? value)
    {
        if (value is null) _d.Remove(key); else _d[key] = value;
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_d));
    }
}
