# Neon Abyss 2 — Unlimited Resources Mod

A BepInEx plugin for Neon Abyss 2 that keeps your resources topped up during runs.

## What it does

| Resource | Effect |
|----------|--------|
| **Bombs** | Set to 99 (max also raised to 99) |
| **Keys** | Set to 99 (max also raised to 99) |
| **Crystals** | Auto-refills to the highest count you've had this run |
| **Fate Points** | Set to 999 |

- **Toggle:** Press **F9** to enable/disable mid-run
- **Coins are NOT modified** — unlimited coins would be too powerful
- Resources are checked every 2 seconds (lightweight polling, not per-frame)

## Quick Install (pre-built)

1. **Install BepInEx 6 bleeding edge** ([build 755](https://builds.bepinex.dev/projects/bepinex_be)) — download the Unity IL2CPP x64 build
2. Extract BepInEx into your Neon Abyss 2 game folder (`<Steam>/steamapps/common/Neon Abyss 2/`)
3. **Launch the game once** and close it — this generates the interop assemblies
4. Copy `release/Il2CppInterop.Runtime.dll` → `<game>/BepInEx/core/Il2CppInterop.Runtime.dll` (replace existing)
5. Copy `release/UnlimitedResources.dll` → `<game>/BepInEx/plugins/UnlimitedResources.dll`
6. Launch the game — done!

> **Why replace Il2CppInterop.Runtime.dll?**
> Stock Il2CppInterop has a [known bug](https://github.com/BepInEx/Il2CppInterop/issues/239) that crashes Unity 2022.3.x games on scene transitions. The included DLL has the crashing hook (`Class_GetFieldDefaultValue_Hook`) disabled.

## Building from Source

**Prerequisites:**
- .NET 6.0 SDK
- BepInEx installed and run at least once (to generate interop assemblies)

```bash
cd UnlimitedResources
dotnet build -p:GAME_DIR="D:\Program Files (x86)\Steam\steamapps\common\Neon Abyss 2"
```

The built DLL will be at `bin/Debug/net6.0/UnlimitedResources.dll`. Copy it to `<game>/BepInEx/plugins/`.

## How it Works

The mod runs as a BepInEx IL2CPP plugin with a MonoBehaviour that polls every 2 seconds:

- **Bombs & Keys** are stored in the game's `Attri` (attribute) system on `NEONPlayerState`. The mod sets `DefaultBomb`, `MaxBomb`, `Key`, and `MaxKey` attrs to 99.
- **Crystals** use a separate `CostType` resource system. The mod tracks the max crystal count seen during the run and calls `AddResource(CostType.Crystal, deficit)` when the count drops.
- **Fate Points** are stored on `SaveData.fatePointData.defaultPoint`, accessed via `GameState.CurrentSave`.

References are cached and reset on scene transitions.

## Compatibility

- **Game version:** Tested with Neon Abyss 2 on Unity 2022.3.62f3
- **Mod loader:** BepInEx 6.0.0-be.755 with patched Il2CppInterop
- **Platform:** Windows x64

## License

MIT
