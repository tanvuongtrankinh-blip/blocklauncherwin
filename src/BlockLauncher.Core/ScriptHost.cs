using System.Reflection;
using Jint;
using Jint.Runtime.Interop;

namespace BlockLauncher.Core;

public sealed class LoadedScript
{
    public required string Name { get; init; }
    public string? Path { get; init; }
    public required Engine Engine { get; init; }
    public required ApiContext Context { get; init; }
    public bool Enabled { get; set; } = true;
    public string? LastError { get; set; }
}

/// <summary>Loads ModPE scripts, each in its own sandboxed JS engine, and dispatches game hooks to them.</summary>
public sealed class ScriptHost
{
    // Hooks and whether scripts may veto the default behaviour via preventDefault().
    public static readonly IReadOnlyList<string> Hooks = new[]
    {
        "newLevel", "leaveGame", "modTick", "selectLevelHook", "useItem", "attackHook", "deathHook",
        "destroyBlock", "startDestroyBlock", "continueDestroyBlock", "chatHook", "chatReceiveHook",
        "serverMessageReceiveHook", "procCmd", "eatHook", "entityAddedHook", "entityRemovedHook",
        "entityHurtHook", "explodeHook", "playerAddExpHook", "playerExpLevelChangeHook",
        "blockEventHook", "levelEventHook", "redstoneUpdateHook", "screenChangeHook",
        "projectileHitBlockHook", "projectileHitEntityHook",
        "customThrowableHitBlockHook", "customThrowableHitEntityHook",
    };

    private readonly List<LoadedScript> _scripts = new();
    private readonly IGameBridge _game;
    private readonly IDataStore _store;
    private readonly string _constants = ReadResource("ModPeConstants.js");
    private readonly string _prelude = ReadResource("ModPePrelude.js");

    public ScriptHost(IGameBridge game, IDataStore store)
    {
        _game = game;
        _store = store;
    }

    public event Action<LogLevel, string, string>? Log; // level, script, message
    public IReadOnlyList<LoadedScript> Scripts => _scripts;
    public TimeSpan ScriptTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public LoadedScript LoadFile(string path)
    {
        var script = LoadSource(System.IO.Path.GetFileName(path), File.ReadAllText(path), path);
        return script;
    }

    public LoadedScript LoadSource(string name, string source, string? path = null)
    {
        _scripts.RemoveAll(s => s.Name == name);
        var ctx = new ApiContext
        {
            Game = _game, Store = _store, ScriptName = name,
            Log = (lvl, msg) => Log?.Invoke(lvl, name, msg),
        };
        var engine = new Engine(o =>
        {
            o.Strict(false);
            o.TimeoutInterval(ScriptTimeout);
            o.LimitRecursion(256);
            o.CatchClrExceptions();
            // ModPE scripts use camelCase; our C# API uses PascalCase.
            o.SetTypeResolver(new TypeResolver { MemberNameComparer = StringComparer.OrdinalIgnoreCase });
        });

        var script = new LoadedScript { Name = name, Path = path, Engine = engine, Context = ctx };
        Install(engine, ctx);
        _scripts.Add(script);
        try
        {
            engine.Execute(_constants);
            engine.Execute(_prelude);
            engine.Execute(source, name);
            ctx.Log(LogLevel.Info, "Script loaded.");
        }
        catch (Exception ex)
        {
            Fail(script, "load", ex);
        }
        return script;
    }

    public void Unload(LoadedScript s) => _scripts.Remove(s);
    public void Reload(LoadedScript s)
    {
        if (s.Path is null) return;
        LoadFile(s.Path);
    }

    /// <summary>Calls <paramref name="hook"/> in every enabled script. Returns true if any script called preventDefault().</summary>
    public bool Dispatch(string hook, params object?[] args)
    {
        var prevented = false;
        foreach (var s in _scripts.ToArray())
        {
            if (!s.Enabled) continue;
            try
            {
                if (s.Engine.Evaluate($"typeof {hook}").AsString() != "function") continue;
                s.Context.PreventRequested = false;
                s.Engine.Invoke(hook, args!);
                prevented |= s.Context.PreventRequested;
            }
            catch (Exception ex)
            {
                Fail(s, hook, ex);
            }
        }
        return prevented;
    }

    // Convenience wrappers for the most common hooks.
    public void Tick() => Dispatch("modTick");
    public bool UseItem(int x, int y, int z, int itemId, int blockId, int side, int itemDamage, int blockDamage)
        => Dispatch("useItem", x, y, z, itemId, blockId, side, itemDamage, blockDamage);
    public bool Attack(long attacker, long victim) => Dispatch("attackHook", attacker, victim);
    public bool Chat(string text) => Dispatch("chatHook", text);
    public bool Command(string command) => Dispatch("procCmd", command);
    public void NewLevel(bool hasLevel) => Dispatch("newLevel", hasLevel);

    private void Fail(LoadedScript s, string where, Exception ex)
    {
        var msg = ex is Jint.Runtime.JavaScriptException js ? js.Message : ex.Message;
        s.LastError = $"{where}: {msg}";
        s.Context.Log(LogLevel.Error, s.LastError);
    }

    private static void Install(Engine e, ApiContext c)
    {
        e.SetValue("Level", new LevelApi(c));
        e.SetValue("Player", new PlayerApi(c));
        e.SetValue("Entity", new EntityApi(c));
        e.SetValue("Item", new ItemApi(c));
        e.SetValue("Block", new BlockApi(c));
        e.SetValue("Server", new ServerApi(c));
        e.SetValue("Gui", new GuiApi(c));
        e.SetValue("ModPE", new ModPeApi(c));
        e.SetValue("clientMessage", new Action<string>(m => c.Game.ShowMessage(m)));
        e.SetValue("preventDefault", new Action(() => c.PreventRequested = true));
        e.SetValue("getLevel", new Func<object>(() => c.Game.WorldName));
    }

    private static string ReadResource(string name)
    {
        using var s = typeof(ScriptHost).Assembly.GetManifestResourceStream(name)
                      ?? throw new InvalidOperationException("Missing resource " + name);
        using var r = new StreamReader(s);
        return r.ReadToEnd();
    }
}
