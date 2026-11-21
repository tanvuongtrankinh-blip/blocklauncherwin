namespace BlockLauncher.Core;

/// <summary>
/// Everything a ModPE script can reach in the game. Implement this against a real game
/// integration; <see cref="SimulatedGame"/> is the built-in in-memory implementation.
/// </summary>
public interface IGameBridge
{
    bool InWorld { get; }
    bool IsRemote { get; }
    string WorldName { get; }
    string MinecraftVersion { get; }

    void ShowMessage(string text);
    string ExecuteCommand(string command, bool silent);

    // World
    int GetTile(int x, int y, int z);
    int GetData(int x, int y, int z);
    void SetTile(int x, int y, int z, int id, int data);
    void DestroyBlock(int x, int y, int z, bool drop);
    long Time { get; set; }
    int GameMode { get; set; }
    int Difficulty { get; set; }
    void SetSpawn(int x, int y, int z);
    void Explode(double x, double y, double z, double radius);
    ItemStack GetSlot(int x, int y, int z, int slot);
    void SetSlot(int x, int y, int z, int slot, ItemStack stack);
    string? GetSignText(int x, int y, int z, int line);
    void SetSignText(int x, int y, int z, int line, string text);

    // Entities
    long PlayerEntity { get; }
    IReadOnlyList<long> AllEntities { get; }
    bool EntityExists(long id);
    long SpawnEntity(double x, double y, double z, int typeId);
    void RemoveEntity(long id);
    int GetEntityType(long id);
    Vec3 GetPosition(long id);
    void SetPosition(long id, double x, double y, double z);
    Vec3 GetVelocity(long id);
    void SetVelocity(long id, double x, double y, double z);
    (double Yaw, double Pitch) GetRotation(long id);
    void SetRotation(long id, double yaw, double pitch);
    int GetHealth(long id);
    void SetHealth(long id, int halfHearts);
    int GetMaxHealth(long id);
    void SetMaxHealth(long id, int halfHearts);
    string GetNameTag(long id);
    void SetNameTag(long id, string name);
    bool IsSneaking(long id);
    void SetSneaking(long id, bool sneaking);
    void SetFireTicks(long id, int ticks);
    string? GetExtra(long id, string key);
    void SetExtra(long id, string key, string? value);
    ItemStack GetEntityItem(long id, int slot);      // slot 0 = hand, 1..4 armor, 5 = offhand
    void SetEntityItem(long id, int slot, ItemStack stack);
    void AddEffect(long id, int effectId, int durationTicks, int amplifier);
    void RemoveEffect(long id, int effectId);

    // Player-specific
    ItemStack GetInventory(int slot);
    void SetInventory(int slot, ItemStack stack);
    int SelectedSlot { get; set; }
    bool IsFlying { get; set; }
    bool CanFly { get; set; }
    double Hunger { get; set; }
    double Saturation { get; set; }
    double Exhaustion { get; set; }
    int XpLevel { get; set; }
    double XpProgress { get; set; }
    int Score { get; }
    (int X, int Y, int Z, int Id, int Data, int Side)? PointedBlock { get; }
    long PointedEntity { get; }

    // Content registration (items/blocks defined by scripts)
    void RegisterItem(ItemDefinition def);
    void RegisterBlock(BlockDefinition def);
    ItemDefinition? GetItemDefinition(int id);
    BlockDefinition? GetBlockDefinition(int id);
    IEnumerable<int> AllBlockIds { get; }
    void AddRecipe(string description);
}
