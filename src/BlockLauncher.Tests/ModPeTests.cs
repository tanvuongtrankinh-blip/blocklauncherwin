using BlockLauncher.Core;
using Xunit;

public class ModPeTests
{
    private static (ScriptHost host, SimulatedGame game, List<string> msgs, List<string> logs) Make()
    {
        var game = new SimulatedGame();
        var host = new ScriptHost(game, new MemoryDataStore());
        var msgs = new List<string>();
        var logs = new List<string>();
        game.Message += msgs.Add;
        host.Log += (l, s, m) => logs.Add($"{l}:{m}");
        return (host, game, msgs, logs);
    }

    [Fact]
    public void SetTileAndGetTileWork()
    {
        var (host, game, _, _) = Make();
        host.LoadSource("t.js", "function useItem(x,y,z,i,b,s){ Level.setTile(x,y+1,z,20); }");
        host.UseItem(0, 70, 0, 1, 0, 1, 0, 0);
        Assert.Equal(20, game.GetTile(0, 71, 0));
    }

    [Fact]
    public void ClientMessageAndConstantsWork()
    {
        var (host, _, msgs, _) = Make();
        host.LoadSource("t.js", "function modTick(){ clientMessage(ChatColor.RED + 'hi ' + EntityType.ZOMBIE); }");
        host.Tick();
        Assert.Single(msgs);
        Assert.Contains("hi 32", msgs[0]);
    }

    [Fact]
    public void PreventDefaultIsReported()
    {
        var (host, _, _, _) = Make();
        host.LoadSource("t.js", "function chatHook(s){ preventDefault(); }");
        Assert.True(host.Chat("hello"));
    }

    [Fact]
    public void InfiniteLoopIsStoppedAndLogged()
    {
        var (host, _, _, logs) = Make();
        host.ScriptTimeout = TimeSpan.FromMilliseconds(200);
        host.LoadSource("t.js", "function modTick(){ while(true){} }");
        host.Tick();
        Assert.Contains(logs, l => l.StartsWith("Error"));
    }

    [Fact]
    public void SaveAndReadDataRoundTrip()
    {
        var (host, _, msgs, _) = Make();
        host.LoadSource("t.js", "ModPE.saveData('k','v'); clientMessage(ModPE.readData('k'));");
        Assert.Equal("v", msgs.Single());
    }
}
