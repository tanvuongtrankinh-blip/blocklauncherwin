namespace BlockLauncher.Core;

public readonly record struct ItemStack(int Id, int Count, int Damage)
{
    public static readonly ItemStack Empty = new(0, 0, 0);
    public bool IsEmpty => Id == 0 || Count <= 0;
}

public readonly record struct Vec3(double X, double Y, double Z);

public enum LogLevel { Debug, Info, Warning, Error }

public sealed class ItemDefinition
{
    public int Id { get; init; }
    public string IconName { get; init; } = "";
    public int IconIndex { get; init; }
    public string Name { get; init; } = "";
    public int MaxStackSize { get; set; } = 64;
    public int MaxDamage { get; set; }
    public int Category { get; set; }
    public int FoodHalfHearts { get; init; }
    public bool HandEquipped { get; set; }
    public int UseAnimation { get; set; }
    public string Kind { get; init; } = "item"; // item | food | armor | throwable
}

public sealed class BlockDefinition
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public bool Liquid { get; init; }
    public double DestroyTime { get; set; } = 1.0;
    public double ExplosionResistance { get; set; } = 1.0;
    public double Friction { get; set; } = 0.6;
    public int LightLevel { get; set; }
    public int LightOpacity { get; set; } = 15;
    public int RenderType { get; set; }
    public int RenderLayer { get; set; }
    public bool CollisionEnabled { get; set; } = true;
}

public interface IDataStore
{
    string? Read(string key);
    void Write(string key, string? value); // null removes
}

public sealed class MemoryDataStore : IDataStore
{
    private readonly Dictionary<string, string> _d = new();
    public string? Read(string key) => _d.TryGetValue(key, out var v) ? v : null;
    public void Write(string key, string? value)
    {
        if (value is null) _d.Remove(key); else _d[key] = value;
    }
}
