namespace BlockLauncher.Core;

/// <summary>In-memory game world so scripts can be run and tested without Minecraft.</summary>
public sealed class SimulatedGame : IGameBridge
{
    private sealed class Ent
    {
        public int Type;
        public Vec3 Pos, Vel;
        public double Yaw, Pitch;
        public int Health = 20, MaxHealth = 20;
        public string Name = "";
        public bool Sneaking;
        public int Fire;
        public Dictionary<string, string> Extra = new();
        public ItemStack[] Items = new ItemStack[6];
        public Dictionary<int, (int Duration, int Amp)> Effects = new();
    }

    private readonly Dictionary<(int, int, int), (int Id, int Data)> _blocks = new();
    private readonly Dictionary<(int, int, int, int), ItemStack> _slots = new();
    private readonly Dictionary<(int, int, int, int), string> _signs = new();
    private readonly Dictionary<long, Ent> _ents = new();
    private readonly ItemStack[] _inv = new ItemStack[36];
    private readonly Dictionary<int, ItemDefinition> _items = new();
    private readonly Dictionary<int, BlockDefinition> _blockDefs = new();
    private long _nextId = 1;

    public event Action<string>? Message;
    public List<string> Recipes { get; } = new();

    public SimulatedGame()
    {
        PlayerEntity = _nextId++;
        _ents[PlayerEntity] = new Ent { Type = 63, Pos = new Vec3(0.5, 65.62, 0.5), Name = "Steve" };
        // flat world: bedrock at y=0, stone to y=60, grass at y=61..62 (simple)
        for (var x = -16; x <= 16; x++)
            for (var z = -16; z <= 16; z++)
            {
                _blocks[(x, 0, z)] = (7, 0);
                for (var y = 1; y <= 60; y++) _blocks[(x, y, z)] = (1, 0);
                _blocks[(x, 61, z)] = (3, 0);
                _blocks[(x, 62, z)] = (2, 0);
            }
        InWorld = true;
    }

    public bool InWorld { get; set; }
    public bool IsRemote => false;
    public string WorldName => "Simulated World";
    public string MinecraftVersion => "simulated";

    public void ShowMessage(string text) => Message?.Invoke(text);
    public string ExecuteCommand(string command, bool silent)
    {
        ShowMessage("[cmd] /" + command.TrimStart('/'));
        return "";
    }

    // ---- world
    public int GetTile(int x, int y, int z) => _blocks.TryGetValue((x, y, z), out var b) ? b.Id : 0;
    public int GetData(int x, int y, int z) => _blocks.TryGetValue((x, y, z), out var b) ? b.Data : 0;
    public void SetTile(int x, int y, int z, int id, int data)
    {
        if (id == 0) _blocks.Remove((x, y, z)); else _blocks[(x, y, z)] = (id, data);
    }
    public void DestroyBlock(int x, int y, int z, bool drop) => _blocks.Remove((x, y, z));
    public long Time { get; set; }
    public int GameMode { get; set; }
    public int Difficulty { get; set; } = 2;
    public Vec3 Spawn { get; private set; }
    public void SetSpawn(int x, int y, int z) => Spawn = new Vec3(x, y, z);
    public void Explode(double x, double y, double z, double radius)
    {
        var r = (int)Math.Ceiling(radius);
        for (var dx = -r; dx <= r; dx++)
            for (var dy = -r; dy <= r; dy++)
                for (var dz = -r; dz <= r; dz++)
                    if (dx * dx + dy * dy + dz * dz <= radius * radius)
                    {
                        var key = ((int)Math.Floor(x) + dx, (int)Math.Floor(y) + dy, (int)Math.Floor(z) + dz);
                        if (_blocks.TryGetValue(key, out var b) && b.Id != 7) _blocks.Remove(key);
                    }
    }
    public ItemStack GetSlot(int x, int y, int z, int slot) => _slots.TryGetValue((x, y, z, slot), out var s) ? s : ItemStack.Empty;
    public void SetSlot(int x, int y, int z, int slot, ItemStack stack) => _slots[(x, y, z, slot)] = stack;
    public string? GetSignText(int x, int y, int z, int line) => _signs.TryGetValue((x, y, z, line), out var s) ? s : null;
    public void SetSignText(int x, int y, int z, int line, string text) => _signs[(x, y, z, line)] = text;

    // ---- entities
    public long PlayerEntity { get; }
    public IReadOnlyList<long> AllEntities => _ents.Keys.ToList();
    public bool EntityExists(long id) => _ents.ContainsKey(id);
    public long SpawnEntity(double x, double y, double z, int typeId)
    {
        var id = _nextId++;
        _ents[id] = new Ent { Type = typeId, Pos = new Vec3(x, y, z) };
        return id;
    }
    public void RemoveEntity(long id) { if (id != PlayerEntity) _ents.Remove(id); }
    private Ent E(long id) => _ents.TryGetValue(id, out var e) ? e : new Ent();
    public int GetEntityType(long id) => E(id).Type;
    public Vec3 GetPosition(long id) => E(id).Pos;
    public void SetPosition(long id, double x, double y, double z) { if (_ents.TryGetValue(id, out var e)) e.Pos = new Vec3(x, y, z); }
    public Vec3 GetVelocity(long id) => E(id).Vel;
    public void SetVelocity(long id, double x, double y, double z) { if (_ents.TryGetValue(id, out var e)) e.Vel = new Vec3(x, y, z); }
    public (double Yaw, double Pitch) GetRotation(long id) => (E(id).Yaw, E(id).Pitch);
    public void SetRotation(long id, double yaw, double pitch) { if (_ents.TryGetValue(id, out var e)) { e.Yaw = yaw; e.Pitch = pitch; } }
    public int GetHealth(long id) => E(id).Health;
    public void SetHealth(long id, int h) { if (_ents.TryGetValue(id, out var e)) e.Health = Math.Clamp(h, 0, e.MaxHealth); }
    public int GetMaxHealth(long id) => E(id).MaxHealth;
    public void SetMaxHealth(long id, int h) { if (_ents.TryGetValue(id, out var e)) e.MaxHealth = Math.Max(1, h); }
    public string GetNameTag(long id) => E(id).Name;
    public void SetNameTag(long id, string name) { if (_ents.TryGetValue(id, out var e)) e.Name = name; }
    public bool IsSneaking(long id) => E(id).Sneaking;
    public void SetSneaking(long id, bool s) { if (_ents.TryGetValue(id, out var e)) e.Sneaking = s; }
    public void SetFireTicks(long id, int t) { if (_ents.TryGetValue(id, out var e)) e.Fire = t; }
    public string? GetExtra(long id, string key) => E(id).Extra.TryGetValue(key, out var v) ? v : null;
    public void SetExtra(long id, string key, string? value)
    {
        if (!_ents.TryGetValue(id, out var e)) return;
        if (value is null) e.Extra.Remove(key); else e.Extra[key] = value;
    }
    public ItemStack GetEntityItem(long id, int slot) => slot is >= 0 and < 6 ? E(id).Items[slot] : ItemStack.Empty;
    public void SetEntityItem(long id, int slot, ItemStack s) { if (slot is >= 0 and < 6 && _ents.TryGetValue(id, out var e)) e.Items[slot] = s; }
    public void AddEffect(long id, int effectId, int duration, int amp) { if (_ents.TryGetValue(id, out var e)) e.Effects[effectId] = (duration, amp); }
    public void RemoveEffect(long id, int effectId) { if (_ents.TryGetValue(id, out var e)) e.Effects.Remove(effectId); }

    // ---- player
    public ItemStack GetInventory(int slot) => slot is >= 0 and < 36 ? _inv[slot] : ItemStack.Empty;
    public void SetInventory(int slot, ItemStack s) { if (slot is >= 0 and < 36) _inv[slot] = s; }
    public int SelectedSlot { get; set; }
    public bool IsFlying { get; set; }
    public bool CanFly { get; set; }
    public double Hunger { get; set; } = 20;
    public double Saturation { get; set; } = 5;
    public double Exhaustion { get; set; }
    public int XpLevel { get; set; }
    public double XpProgress { get; set; }
    public int Score => 0;
    public (int X, int Y, int Z, int Id, int Data, int Side)? PointedBlock { get; set; }
    public long PointedEntity { get; set; } = -1;

    // ---- content
    public void RegisterItem(ItemDefinition def) => _items[def.Id] = def;
    public void RegisterBlock(BlockDefinition def) => _blockDefs[def.Id] = def;
    public ItemDefinition? GetItemDefinition(int id) => _items.TryGetValue(id, out var d) ? d : null;
    public BlockDefinition? GetBlockDefinition(int id) => _blockDefs.TryGetValue(id, out var d) ? d : null;
    public IEnumerable<int> AllBlockIds => _blockDefs.Keys.OrderBy(i => i);
    public void AddRecipe(string description) => Recipes.Add(description);

    /// <summary>Snapshot of blocks near a point, for the simulator viewer.</summary>
    public IEnumerable<((int X, int Y, int Z) Pos, int Id)> BlocksNear(int cx, int cy, int cz, int r)
    {
        foreach (var (k, v) in _blocks)
            if (Math.Abs(k.Item1 - cx) <= r && Math.Abs(k.Item2 - cy) <= r && Math.Abs(k.Item3 - cz) <= r)
                yield return (k, v.Id);
    }
}
