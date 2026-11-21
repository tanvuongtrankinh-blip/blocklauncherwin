namespace BlockLauncher.Core;

/// <summary>Shared state handed to every API object of one script.</summary>
public sealed class ApiContext
{
    public required IGameBridge Game { get; init; }
    public required IDataStore Store { get; init; }
    public required string ScriptName { get; init; }
    public required Action<LogLevel, string> Log { get; init; }
    public bool PreventRequested { get; set; }
    private readonly HashSet<string> _warned = new();

    public void Unsupported(string api)
    {
        if (_warned.Add(api)) Log(LogLevel.Warning, $"{api} is not supported on this platform (ignored).");
    }
    public string PrefKey(string k) => $"{ScriptName}:{k}";
}

// NOTE: method names are matched case-insensitively by Jint, so C# PascalCase == ModPE camelCase.

public sealed class LevelApi(ApiContext c)
{
    private IGameBridge G => c.Game;
    public int GetTile(int x, int y, int z) => G.GetTile(x, y, z);
    public int GetData(int x, int y, int z) => G.GetData(x, y, z);
    public void SetTile(int x, int y, int z, int id, int damage = 0) => G.SetTile(x, y, z, id, damage);
    public void DestroyBlock(int x, int y, int z, bool shouldDrop = false) => G.DestroyBlock(x, y, z, shouldDrop);
    public void Explode(double x, double y, double z, double radius, bool onFire = false, bool smoke = true, double other = 0) => G.Explode(x, y, z, radius);
    public long SpawnMob(double x, double y, double z, int typeId, string? tex = null) => G.SpawnEntity(x, y, z, typeId);
    public long SpawnChicken(double x, double y, double z, string? tex = null) => G.SpawnEntity(x, y, z, 10);
    public long SpawnCow(double x, double y, double z, string? tex = null) => G.SpawnEntity(x, y, z, 11);
    public long DropItem(double x, double y, double z, double range, int id, int count = 1, int damage = 0)
    {
        var e = G.SpawnEntity(x, y, z, 64);
        G.SetEntityItem(e, 0, new ItemStack(id, count, damage));
        return e;
    }
    public string GetWorldName() => G.WorldName;
    public string GetWorldDir() => G.WorldName;
    public int GetTime() => (int)G.Time;
    public void SetTime(int t) => G.Time = t;
    public int GetGameMode() => G.GameMode;
    public void SetGameMode(int m) => G.GameMode = m;
    public int GetDifficulty() => G.Difficulty;
    public void SetDifficulty(int d) => G.Difficulty = d;
    public void SetSpawn(int x, int y, int z) => G.SetSpawn(x, y, z);
    public bool IsRemote() => G.IsRemote;
    public string ExecuteCommand(string cmd, bool silent = false) => G.ExecuteCommand(cmd, silent);
    public string? GetSignText(int x, int y, int z, int line) => G.GetSignText(x, y, z, line);
    public void SetSignText(int x, int y, int z, int line, string text) => G.SetSignText(x, y, z, line, text);

    public void SetChestSlot(int x, int y, int z, int slot, int id, int damage, int count) => G.SetSlot(x, y, z, slot, new ItemStack(id, count, damage));
    public int GetChestSlot(int x, int y, int z, int slot) => G.GetSlot(x, y, z, slot).Id;
    public int GetChestSlotData(int x, int y, int z, int slot) => G.GetSlot(x, y, z, slot).Damage;
    public int GetChestSlotCount(int x, int y, int z, int slot) => G.GetSlot(x, y, z, slot).Count;
    public void SetFurnaceSlot(int x, int y, int z, int slot, int id, int damage, int count) => G.SetSlot(x, y, z, 100 + slot, new ItemStack(id, count, damage));
    public int GetFurnaceSlot(int x, int y, int z, int slot) => G.GetSlot(x, y, z, 100 + slot).Id;
    public int GetFurnaceSlotData(int x, int y, int z, int slot) => G.GetSlot(x, y, z, 100 + slot).Damage;
    public int GetFurnaceSlotCount(int x, int y, int z, int slot) => G.GetSlot(x, y, z, 100 + slot).Count;

    public void SetNightMode(bool night) => G.Time = night ? 14000 : 1000;
    public void PlaySound(double x, double y, double z, string sound, double volume = 1, double pitch = 1) => c.Unsupported("Level.playSound");
    public void PlaySoundEnt(object ent, string sound, double volume = 1, double pitch = 1) => c.Unsupported("Level.playSoundEnt");
    public void AddParticle(object type, double x, double y, double z, double xv, double yv, double zv, int size = 0) => c.Unsupported("Level.addParticle");
    public int GetBrightness(int x, int y, int z) => 15;
    public bool CanSeeSky(int x, int y, int z) { for (var yy = y + 1; yy < 256; yy++) if (G.GetTile(x, yy, z) != 0) return false; return true; }
    public int GetBiome(int x, int z) { c.Unsupported("Level.getBiome"); return 1; }
    public string GetBiomeName(int x, int z) { c.Unsupported("Level.getBiomeName"); return "Plains"; }
    public double GetRainLevel() => 0;
    public void SetRainLevel(double v) => c.Unsupported("Level.setRainLevel");
    public double GetLightningLevel() => 0;
    public void SetLightningLevel(double v) => c.Unsupported("Level.setLightningLevel");
}

public sealed class PlayerApi(ApiContext c)
{
    private IGameBridge G => c.Game;
    private long P => G.PlayerEntity;
    public double GetX() => G.GetPosition(P).X;
    public double GetY() => G.GetPosition(P).Y;
    public double GetZ() => G.GetPosition(P).Z;
    public long GetEntity() => P;
    public string GetName(object ent) => ent is null || Convert.ToInt64(ent) == P ? "Steve" : G.GetNameTag(Convert.ToInt64(ent));
    public bool IsPlayer(object ent) => Convert.ToInt64(ent) == P;
    public int GetCarriedItem() => G.GetInventory(G.SelectedSlot).Id;
    public int GetCarriedItemData() => G.GetInventory(G.SelectedSlot).Damage;
    public int GetCarriedItemCount() => G.GetInventory(G.SelectedSlot).Count;
    public void SetHealth(int v) => G.SetHealth(P, v);
    public int GetSelectedSlotId() => G.SelectedSlot;
    public void SetSelectedSlotId(int s) => G.SelectedSlot = s;
    public int GetInventorySlot(int s) => G.GetInventory(s).Id;
    public int GetInventorySlotData(int s) => G.GetInventory(s).Damage;
    public int GetInventorySlotCount(int s) => G.GetInventory(s).Count;
    public void SetInventorySlot(int slot, int id, int count, int damage = 0) => G.SetInventory(slot, new ItemStack(id, count, damage));
    public void ClearInventorySlot(int slot) => G.SetInventory(slot, ItemStack.Empty);
    public void AddItemInventory(int id, int amount, int damage = 0)
    {
        var max = G.GetItemDefinition(id)?.MaxStackSize ?? 64;
        for (var i = 0; i < 36 && amount > 0; i++)
        {
            var s = G.GetInventory(i);
            if (s.Id == id && s.Damage == damage && s.Count < max)
            {
                var add = Math.Min(max - s.Count, amount);
                G.SetInventory(i, s with { Count = s.Count + add });
                amount -= add;
            }
        }
        for (var i = 0; i < 36 && amount > 0; i++)
            if (G.GetInventory(i).IsEmpty)
            {
                var add = Math.Min(max, amount);
                G.SetInventory(i, new ItemStack(id, add, damage));
                amount -= add;
            }
    }
    public void AddItemCreativeInv(int id, int count = 1, int damage = 0) => c.Unsupported("Player.addItemCreativeInv");
    public int GetArmorSlot(int slot) => G.GetEntityItem(P, 1 + slot).Id;
    public int GetArmorSlotDamage(int slot) => G.GetEntityItem(P, 1 + slot).Damage;
    public void SetArmorSlot(int slot, int id, int damage = 0) => G.SetEntityItem(P, 1 + slot, new ItemStack(id, id == 0 ? 0 : 1, damage));
    public long GetPointedEntity() => G.PointedEntity;
    public int GetPointedBlockX() => G.PointedBlock?.X ?? 0;
    public int GetPointedBlockY() => G.PointedBlock?.Y ?? 0;
    public int GetPointedBlockZ() => G.PointedBlock?.Z ?? 0;
    public int GetPointedBlockId() => G.PointedBlock?.Id ?? 0;
    public int GetPointedBlockData() => G.PointedBlock?.Data ?? 0;
    public int GetPointedBlockSide() => G.PointedBlock?.Side ?? 0;
    public bool IsFlying() => G.IsFlying;
    public void SetFlying(bool v) => G.IsFlying = v;
    public bool CanFly() => G.CanFly;
    public void SetCanFly(bool v) => G.CanFly = v;
    public int GetDimension() => 0;
    public double GetHunger() => G.Hunger;
    public void SetHunger(double v) => G.Hunger = v;
    public double GetExhaustion() => G.Exhaustion;
    public void SetExhaustion(double v) => G.Exhaustion = v;
    public double GetSaturation() => G.Saturation;
    public void SetSaturation(double v) => G.Saturation = v;
    public int GetLevel() => G.XpLevel;
    public void SetLevel(int v) => G.XpLevel = v;
    public double GetExp() => G.XpProgress;
    public void SetExp(double v) => G.XpProgress = v;
    public void AddExp(int v) => G.XpProgress += v;
    public int GetScore() => G.Score;
    public bool Enchant(int slot, int enchantment, int level) { c.Unsupported("Player.enchant"); return false; }
    public string GetItemCustomName(int slot) => "";
    public void SetItemCustomName(int slot, string name) => c.Unsupported("Player.setItemCustomName");
}

public sealed class EntityApi(ApiContext c)
{
    private IGameBridge G => c.Game;
    private static long Id(object? o) => o is null ? -1 : Convert.ToInt64(o);
    public void SetVelX(object e, double v) { var o = G.GetVelocity(Id(e)); G.SetVelocity(Id(e), v, o.Y, o.Z); }
    public void SetVelY(object e, double v) { var o = G.GetVelocity(Id(e)); G.SetVelocity(Id(e), o.X, v, o.Z); }
    public void SetVelZ(object e, double v) { var o = G.GetVelocity(Id(e)); G.SetVelocity(Id(e), o.X, o.Y, v); }
    public double GetVelX(object e) => G.GetVelocity(Id(e)).X;
    public double GetVelY(object e) => G.GetVelocity(Id(e)).Y;
    public double GetVelZ(object e) => G.GetVelocity(Id(e)).Z;
    public void SetRot(object e, double yaw, double pitch) => G.SetRotation(Id(e), yaw, pitch);
    public double GetYaw(object e) => G.GetRotation(Id(e)).Yaw;
    public double GetPitch(object e) => G.GetRotation(Id(e)).Pitch;
    public void SetPosition(object e, double x, double y, double z) => G.SetPosition(Id(e), x, y, z);
    public void SetPositionRelative(object e, double x, double y, double z)
    {
        var p = G.GetPosition(Id(e)); G.SetPosition(Id(e), p.X + x, p.Y + y, p.Z + z);
    }
    public double GetX(object e) => G.GetPosition(Id(e)).X;
    public double GetY(object e) => G.GetPosition(Id(e)).Y;
    public double GetZ(object e) => G.GetPosition(Id(e)).Z;
    public void SetFireTicks(object e, int t) => G.SetFireTicks(Id(e), t);
    public int GetEntityTypeId(object e) => G.GetEntityType(Id(e));
    public long SpawnMob(double x, double y, double z, int type, string? tex = null) => G.SpawnEntity(x, y, z, type);
    public void Remove(object e) => G.RemoveEntity(Id(e));
    public int GetHealth(object e) => G.GetHealth(Id(e));
    public void SetHealth(object e, int h) => G.SetHealth(Id(e), h);
    public int GetMaxHealth(object e) => G.GetMaxHealth(Id(e));
    public void SetMaxHealth(object e, int h) => G.SetMaxHealth(Id(e), h);
    public string GetNameTag(object e) => G.GetNameTag(Id(e));
    public void SetNameTag(object e, string n) => G.SetNameTag(Id(e), n);
    public bool IsSneaking(object e) => G.IsSneaking(Id(e));
    public void SetSneaking(object e, bool s) => G.SetSneaking(Id(e), s);
    public long[] GetAll() => G.AllEntities.ToArray();
    public string GetUniqueId(object e) => "entity-" + Id(e);
    public string? GetExtraData(object e, string key) => G.GetExtra(Id(e), key);
    public bool SetExtraData(object e, string key, string? value) { G.SetExtra(Id(e), key, value); return true; }
    public void SetCarriedItem(object e, int id, int count = 1, int damage = 0) => G.SetEntityItem(Id(e), 0, new ItemStack(id, count, damage));
    public int GetCarriedItem(object e) => G.GetEntityItem(Id(e), 0).Id;
    public int GetCarriedItemData(object e) => G.GetEntityItem(Id(e), 0).Damage;
    public int GetCarriedItemCount(object e) => G.GetEntityItem(Id(e), 0).Count;
    public int GetOffhandSlot(object e) => G.GetEntityItem(Id(e), 5).Id;
    public int GetOffhandSlotData(object e) => G.GetEntityItem(Id(e), 5).Damage;
    public int GetOffhandSlotCount(object e) => G.GetEntityItem(Id(e), 5).Count;
    public void SetOffhandSlot(object e, int id, int count = 1, int damage = 0) => G.SetEntityItem(Id(e), 5, new ItemStack(id, count, damage));
    public int GetArmor(object e, int slot) => G.GetEntityItem(Id(e), 1 + slot).Id;
    public int GetArmorDamage(object e, int slot) => G.GetEntityItem(Id(e), 1 + slot).Damage;
    public void SetArmor(object e, int slot, int id, int damage = 0) => G.SetEntityItem(Id(e), 1 + slot, new ItemStack(id, id == 0 ? 0 : 1, damage));
    public int GetItemEntityId(object e) => G.GetEntityItem(Id(e), 0).Id;
    public int GetItemEntityData(object e) => G.GetEntityItem(Id(e), 0).Damage;
    public int GetItemEntityCount(object e) => G.GetEntityItem(Id(e), 0).Count;
    public void AddEffect(object e, int effect, int duration, int amp, bool ambient = false, bool particles = true) => G.AddEffect(Id(e), effect, duration * 20, amp);
    public void RemoveEffect(object e, int effect) => G.RemoveEffect(Id(e), effect);
    public void RemoveAllEffects(object e) { for (var i = 1; i <= 25; i++) G.RemoveEffect(Id(e), i); }
    public void RideAnimal(object rider, object mount) => c.Unsupported("Entity.rideAnimal");
    public int GetRiding(object e) { c.Unsupported("Entity.getRiding"); return -1; }
    public int GetRider(object e) { c.Unsupported("Entity.getRider"); return -1; }
    public void SetMobSkin(object e, string tex) => c.Unsupported("Entity.setMobSkin");
    public string GetMobSkin(object e) => "";
    public void SetRenderType(object e, object t) => c.Unsupported("Entity.setRenderType");
    public int GetRenderType(object e) => 0;
    public void SetCollisionSize(object e, double w, double h) => c.Unsupported("Entity.setCollisionSize");
    public void SetCape(object e, string loc) => c.Unsupported("Entity.setCape");
    public void SetImmobile(object e, bool v) => c.Unsupported("Entity.setImmobile");
    public void SetAnimalAge(object e, int a) => c.Unsupported("Entity.setAnimalAge");
    public int GetAnimalAge(object e) => 0;
    public long GetTarget(object e) => -1;
    public void SetTarget(object e, object t) => c.Unsupported("Entity.setTarget");
}

public sealed class ItemApi(ApiContext c)
{
    private IGameBridge G => c.Game;
    public void SetItem(int id, string icon, int idx, string name, int maxStack = 64) =>
        G.RegisterItem(new ItemDefinition { Id = id, IconName = icon, IconIndex = idx, Name = name, MaxStackSize = maxStack });
    public void SetFoodItem(int id, string icon, int idx, int halfHearts, string name, int maxStack = 64) =>
        G.RegisterItem(new ItemDefinition { Id = id, IconName = icon, IconIndex = idx, Name = name, MaxStackSize = maxStack, FoodHalfHearts = halfHearts, Kind = "food" });
    public void DefineArmor(int id, string icon, int idx, string name, string texture, int damageReduceAmount, int maxDamage, int armorType) =>
        G.RegisterItem(new ItemDefinition { Id = id, IconName = icon, IconIndex = idx, Name = name, MaxStackSize = 1, MaxDamage = maxDamage, Kind = "armor" });
    public void DefineThrowable(int id, string icon, int idx, string name, int maxStack = 64) =>
        G.RegisterItem(new ItemDefinition { Id = id, IconName = icon, IconIndex = idx, Name = name, MaxStackSize = maxStack, Kind = "throwable" });
    public string GetName(int id, int damage = 0, bool raw = false) => G.GetItemDefinition(id)?.Name ?? $"item.{id}";
    public int InternalNameToId(string name) => 0;
    public int TranslatedNameToId(string name) => 0;
    public bool IsValidItem(int id) => G.GetItemDefinition(id) is not null || G.GetBlockDefinition(id) is not null;
    public void SetMaxDamage(int id, int v) { if (G.GetItemDefinition(id) is { } d) d.MaxDamage = v; }
    public int GetMaxDamage(int id) => G.GetItemDefinition(id)?.MaxDamage ?? 0;
    public int GetMaxStackSize(int id) => G.GetItemDefinition(id)?.MaxStackSize ?? 64;
    public void SetCategory(int id, int cat) { if (G.GetItemDefinition(id) is { } d) d.Category = cat; }
    public void SetHandEquipped(int id, bool v) { if (G.GetItemDefinition(id) is { } d) d.HandEquipped = v; }
    public void SetUseAnimation(int id, int a) { if (G.GetItemDefinition(id) is { } d) d.UseAnimation = a; }
    public int GetUseAnimation(int id) => G.GetItemDefinition(id)?.UseAnimation ?? 0;
    public void AddCraftRecipe(int id, int count, int damage, object ingredients) => G.AddRecipe($"craft {count}x{id}:{damage}");
    public void AddFurnaceRecipe(int input, int output, int outDamage) => G.AddRecipe($"smelt {input} -> {output}:{outDamage}");
    public void AddShapedRecipe(int id, int count, int damage, object shape, object ingredients) => G.AddRecipe($"shaped {count}x{id}:{damage}");
    public void AddIcon(string url, string name, int index) => c.Unsupported("Item.addIcon");
    public void SetProperties(int id, object props) => c.Unsupported("Item.setProperties");
    public void SetStackedByData(int id, bool v) => c.Unsupported("Item.setStackedByData");
    public void SetAllowOffhand(int id, bool v) => c.Unsupported("Item.setAllowOffhand");
    public void SetRecipeIgnoreData(int id, bool v) => c.Unsupported("Item.setRecipeIgnoreData");
    public void SetEnchantType(int id, int flag, int value) => c.Unsupported("Item.setEnchantType");
}

public sealed class BlockApi(ApiContext c)
{
    private IGameBridge G => c.Game;
    private BlockDefinition? D(int id) => G.GetBlockDefinition(id);
    public void DefineBlock(int id, string name, object textures, int sourceId = 0, bool opaque = true, int renderType = 0) =>
        G.RegisterBlock(new BlockDefinition { Id = id, Name = name, RenderType = renderType });
    public int DefineLiquidBlock(int id, string name, object textures, object? flowing = null) =>
        Register(new BlockDefinition { Id = id, Name = name, Liquid = true });
    private int Register(BlockDefinition d) { G.RegisterBlock(d); return d.Id; }
    public void SetDestroyTime(int id, double t) { if (D(id) is { } d) d.DestroyTime = t; }
    public double GetDestroyTime(int id, int damage = 0) => D(id)?.DestroyTime ?? 1.0;
    public void SetExplosionResistance(int id, double r) { if (D(id) is { } d) d.ExplosionResistance = r; }
    public void SetLightLevel(int id, int l) { if (D(id) is { } d) d.LightLevel = l; }
    public void SetLightOpacity(int id, int l) { if (D(id) is { } d) d.LightOpacity = l; }
    public void SetRenderType(int id, int t) { if (D(id) is { } d) d.RenderType = t; }
    public int GetRenderType(int id) => D(id)?.RenderType ?? 0;
    public void SetRenderLayer(int id, int l) { if (D(id) is { } d) d.RenderLayer = l; }
    public int GetRenderLayer(int id) => D(id)?.RenderLayer ?? 0;
    public void SetCollisionEnabled(int id, bool v) { if (D(id) is { } d) d.CollisionEnabled = v; }
    public double GetFriction(int id, int damage = 0) => D(id)?.Friction ?? 0.6;
    public void SetFriction(int id, double f) { if (D(id) is { } d) d.Friction = f; }
    public int[] GetAllBlockIds() => G.AllBlockIds.ToArray();
    public void SetShape(int id, double a, double b, double c2, double d, double e, double f) => c.Unsupported("Block.setShape");
    public void SetColor(int id, object colors) => c.Unsupported("Block.setColor");
    public void AddIcon(string url, string name, int index) => c.Unsupported("Block.addIcon");
    public void SetRedstoneConsumer(int id, bool v) => c.Unsupported("Block.setRedstoneConsumer");
    public bool IsDouble(int x, int y, int z) => false;
}

public sealed class ServerApi(ApiContext c)
{
    public void JoinServer(string address, int port) => c.Unsupported("Server.joinServer");
    public void SendChat(string message) => c.Game.ShowMessage("<you> " + message);
    public string GetAddress() => "";
    public int GetPort() => 0;
    public long[] GetAllPlayers() => new[] { c.Game.PlayerEntity };
    public string[] GetAllPlayerNames() => new[] { "Steve" };
}

public sealed class GuiApi(ApiContext c)
{
    public int GetScreenWidth() => 1280;
    public int GetScreenHeight() => 720;
}

public sealed class ModPeApi(ApiContext c)
{
    private IGameBridge G => c.Game;
    public void Log(string s) => c.Log(LogLevel.Info, s);
    public string? ReadData(string name) => c.Store.Read(c.PrefKey(name)) ?? "";
    public void SaveData(string name, string value) => c.Store.Write(c.PrefKey(name), value);
    public void RemoveData(string name) => c.Store.Write(c.PrefKey(name), null);
    public string GetMinecraftVersion() => G.MinecraftVersion;
    public string GetOS() => "windows";
    public string GetLanguage() => "en_US";
    public string GetI18n(string key) => key;
    public void ShowTipMessage(string msg) => G.ShowMessage(msg);
    public void LeaveGame() => c.Unsupported("ModPE.leaveGame");
    public void SelectLevel(string dir) => c.Unsupported("ModPE.selectLevel");
    public void SetGameSpeed(double tps) => c.Unsupported("ModPE.setGameSpeed");
    public void TakeScreenshot(string file) => c.Unsupported("ModPE.takeScreenshot");
    public void LangEdit(string key, string value) => c.Unsupported("ModPE.langEdit");
    public void SetCamera(object entity) => c.Unsupported("ModPE.setCamera");
    public void SetFov(double fov) => c.Unsupported("ModPE.setFov");
    public void ResetFov() => c.Unsupported("ModPE.resetFov");
    public void SetTerrain(string url) => c.Unsupported("ModPE.setTerrain");
    public void SetItems(string url) => c.Unsupported("ModPE.setItems");
    public void SetGuiBlocks(string url) => c.Unsupported("ModPE.setGuiBlocks");
    public void OverrideTexture(string a, string b) => c.Unsupported("ModPE.overrideTexture");
    public void ResetImages() => c.Unsupported("ModPE.resetImages");
    public void SetUiRenderDebug(bool v) => c.Unsupported("ModPE.setUiRenderDebug");
}
