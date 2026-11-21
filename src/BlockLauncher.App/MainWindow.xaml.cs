using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BlockLauncher.Core;
using Microsoft.Win32;

namespace BlockLauncher.App;

public partial class MainWindow : Window
{
    private readonly SimulatedGame _game = new();
    private readonly ScriptHost _host;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(50) };

    public MainWindow()
    {
        InitializeComponent();
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlockLauncherWin");
        _host = new ScriptHost(_game, new FileDataStore(Path.Combine(dataDir, "modpe-data.json")));
        _host.Log += (lvl, script, msg) => Dispatcher.Invoke(() => Print($"[{lvl}] {script}: {msg}"));
        _game.Message += m => Dispatcher.Invoke(() => Print("[chat] " + m));
        _timer.Tick += (_, _) => { _host.Tick(); UpdateStatus(); };

        Print("Simulator ready. Add a .js ModPE script, then use the controls below.");
        var sample = Path.Combine(AppContext.BaseDirectory, "Scripts", "sample.js");
        if (File.Exists(sample)) AddScript(sample);
        UpdateStatus();
    }

    private void Print(string line)
    {
        Console.AppendText(line + Environment.NewLine);
        Console.ScrollToEnd();
    }

    private void RefreshList()
    {
        ScriptList.Items.Clear();
        foreach (var s in _host.Scripts) ScriptList.Items.Add(s);
    }

    private void AddScript(string path)
    {
        _host.LoadFile(path);
        RefreshList();
    }

    private LoadedScript? Selected => ScriptList.SelectedItem as LoadedScript;

    private void AddScript_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "ModPE scripts (*.js)|*.js", Multiselect = true };
        if (dlg.ShowDialog() != true) return;
        foreach (var f in dlg.FileNames) AddScript(f);
    }

    private void ReloadScript_Click(object sender, RoutedEventArgs e)
    {
        if (Selected is { } s) { _host.Reload(s); RefreshList(); }
    }

    private void RemoveScript_Click(object sender, RoutedEventArgs e)
    {
        if (Selected is { } s) { _host.Unload(s); RefreshList(); }
    }

    private void ScriptList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void TickBox_Changed(object sender, RoutedEventArgs e)
    {
        if (TickBox.IsChecked == true) _timer.Start(); else _timer.Stop();
    }

    private void UseItem_Click(object sender, RoutedEventArgs e)
    {
        var p = _game.GetPosition(_game.PlayerEntity);
        int x = (int)Math.Floor(p.X), y = (int)Math.Floor(p.Y) - 1, z = (int)Math.Floor(p.Z);
        var held = _game.GetInventory(_game.SelectedSlot);
        var prevented = _host.UseItem(x, y, z, held.Id, _game.GetTile(x, y, z), 1, held.Damage, _game.GetData(x, y, z));
        Print($"[sim] useItem at {x},{y},{z} prevented={prevented}");
        UpdateStatus();
    }

    private void Spawn_Click(object sender, RoutedEventArgs e)
    {
        var p = _game.GetPosition(_game.PlayerEntity);
        var id = _game.SpawnEntity(p.X + 2, p.Y, p.Z, 32);
        _host.Dispatch("entityAddedHook", id);
        Print($"[sim] spawned zombie #{id}");
        UpdateStatus();
    }

    private void Attack_Click(object sender, RoutedEventArgs e)
    {
        var target = _game.AllEntities.FirstOrDefault(i => i != _game.PlayerEntity, -1L);
        if (target < 0) { Print("[sim] nothing to attack"); return; }
        var prevented = _host.Attack(_game.PlayerEntity, target);
        if (!prevented) _game.SetHealth(target, _game.GetHealth(target) - 2);
        Print($"[sim] attack #{target} prevented={prevented}");
    }

    private void NewLevel_Click(object sender, RoutedEventArgs e) => _host.NewLevel(true);

    private void Send_Click(object sender, RoutedEventArgs e)
    {
        var text = Input.Text.Trim();
        if (text.Length == 0) return;
        Input.Clear();
        if (text.StartsWith('/')) { _host.Command(text[1..]); return; }
        if (!_host.Chat(text)) Print("<Steve> " + text);
    }

    private void UpdateStatus()
    {
        var p = _game.GetPosition(_game.PlayerEntity);
        Status.Text = $"Player {p.X:0.0}, {p.Y:0.0}, {p.Z:0.0}   entities: {_game.AllEntities.Count}   time: {_game.Time}";
    }
}
