# COSMIC RAYS: Areeba's Promise - Unity

Unity production project for the COSMIC RAYS narrative science-fiction game.

## Project

- Unity Editor: `6000.3.24f1`
- Rendering: Universal Render Pipeline
- Input: Unity Input System
- UI: UGUI
- MCP: official Unity CLI `unity mcp`
- Build targets: macOS Universal, Windows 64-bit
- Cloud services: disabled

## Story

The game follows Areeba as she discovers her mother Alice's classified SPA-077 research, solves its magnetic-containment calibration, and follows Alice's disappearance toward Enceladus ten years later.

The current product requirements are in [`Docs/GAME_PRD.md`](./Docs/GAME_PRD.md).

## Imported Gameplay Systems

`Assets/CosmicRays` contains the supplied survival, inventory, objectives, save/load, settings, and runtime UI source package. The package was imported unchanged before project-specific prologue work.

## Unity MCP

The project-level [`opencode.json`](./opencode.json) starts the official Unity CLI MCP server for this project. OpenCode must be restarted from this directory after configuration changes.

Editor commands use:

1. Unity Personal, activated on this machine.
2. Unity Pipeline `1.0.0`, installed in this project.
3. The official Unity CLI `1.0.0` MCP server.

`opencode mcp list` reports the project-scoped `unity` server as connected. Restart OpenCode from this directory to load the Unity MCP tools into a new session.

## Generated Scenes

Build order:

1. `Assets/CosmicRays/Scenes/Prologue.unity`
2. `Assets/CosmicRays/Scenes/MainMenu.unity`
3. `Assets/CosmicRays/Scenes/Gameplay.unity`

Run **Tools > COSMIC RAYS > Build Playable Project** after changing generated scene setup.

## Playable Builds

The prologue now includes cinematic title/dinner/ending sequences, first-person
bedroom and laboratory exploration, imported GLB environments, the interactive
SPA-077 routing puzzle, mission file, Alice's note, and the transition into the
Gameplay scene.

### macOS

The verified macOS build is located at:

```text
Builds/COSMIC RAYS.app
Builds/COSMIC-RAYS-macOS.zip
```

Open it from Finder or run:

```bash
open "Builds/COSMIC RAYS.app"
```

### Windows

The shareable Windows 64-bit package is located at:

```text
Builds/COSMIC-RAYS-Windows-x64.zip
```

Extract the whole archive, keep all files together, then run `COSMIC RAYS - Areeba's Promise.exe`. The ZIP includes `HOW-TO-PLAY.txt`, the `_Data` folder, Mono runtime, D3D12 runtime, and `UnityPlayer.dll`.

Exploration controls are WASD, mouse look, Shift to run, E to interact, and
Escape to release or restore the cursor.

Rebuild either target with:

```bash
unity build --target StandaloneOSX --output "Builds/COSMIC RAYS.app"
unity build --target StandaloneWindows64 --output "Builds/Windows/COSMIC RAYS - Areeba's Promise.exe"
```

See [`Docs/UNITY_VERIFICATION.md`](./Docs/UNITY_VERIFICATION.md) for compile, test, Play Mode, MCP, and build results.
