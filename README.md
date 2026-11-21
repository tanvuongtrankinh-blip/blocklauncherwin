# BlockLauncher for Windows (C# / .NET 8 rewrite of the ModPE runtime)

A from-scratch rewrite of BlockLauncher's **ModPE scripting runtime** as a Windows `.exe`
(WPF + [Jint](https://github.com/sebastienros/jint) JavaScript engine).

## Status — read this first

| Part | State |
|---|---|
| ModPE script engine (one sandboxed JS engine per script, 5 s timeout, recursion limit) | implemented |
| Hook dispatch (`useItem`, `modTick`, `attackHook`, `chatHook`, `procCmd`, `newLevel`, …29 hooks), `preventDefault()` | implemented |
| `Level`, `Player`, `Entity`, `Item`, `Block`, `Server`, `Gui`, `ModPE` APIs | implemented against `IGameBridge`; functions that need rendering/audio/etc. log "not supported" once |
| Constants (`ChatColor`, `EntityType`, `ParticleType`, …) | generated from the original Java sources (`tools/gen_constants.py`) |
| Built-in **simulator** world to run and debug scripts | implemented |
| WPF window (script list, console, simulator controls) | implemented |
| **Real Minecraft integration** | **not implemented** — see below |
| **Not compiled or tested yet** | the code was written without a .NET SDK available; run the CI (or `dotnet test`) first and expect small compile fixes |

### Why there is no real game bridge
The original BlockLauncher works by patching Minecraft PE 1.1.x's Android `libminecraftpe.so`
(ARM, fixed offsets). None of that applies to the Windows edition. Attaching to the Windows game
means reverse-engineering and injecting into a specific Minecraft build, which is a separate project.
The app is designed for it: implement `IGameBridge` (src/BlockLauncher.Core/IGameBridge.cs) for your
integration and construct `ScriptHost` with it instead of `SimulatedGame`.

### Script compatibility
Plain ModPE JavaScript works. Scripts that use Java/Android classes (`android.*`, `java.*`,
`net.zhuoweizhang.*`, UI-thread runnables, custom renderers) will fail with a logged error.

## Build the .exe
Push this folder to GitHub: the workflow in `.github/workflows/build.yml` runs tests and uploads
`BlockLauncher-win-x64` (self-contained single-file `BlockLauncher.exe`).

Locally on Windows with the .NET 8 SDK:

    dotnet publish src/BlockLauncher.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o out

## Layout
- `src/BlockLauncher.Core` — runtime, APIs, simulator (no UI dependencies)
- `src/BlockLauncher.App` — WPF front end
- `src/BlockLauncher.Tests` — xunit tests
- `tools/gen_constants.py` — regenerates `ModPeConstants.js` from the Java sources

The original project is Apache-2.0 licensed; the constant tables are derived from it.
