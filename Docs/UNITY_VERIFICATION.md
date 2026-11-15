# Unity Verification

## Environment

| Item | Verified value |
| --- | --- |
| Unity Editor | `6000.3.24f1` |
| Unity CLI | `1.0.0` |
| Unity Pipeline | `1.0.0` |
| Input System | `1.20.1` |
| Render pipeline | URP |
| Windows build support | Mono `windows-mono` |
| Host | macOS 26.4, Intel x86_64 |

## Completed Checks

- Unity Personal license is active.
- Runtime and Editor C# assemblies compile successfully.
- The generated setup and playable-project validators pass.
- Prologue, MainMenu, and Gameplay scenes exist and are enabled in the correct order.
- Starter survival configuration, three supply items, and six mission-objective assets exist.
- Official Unity MCP is connected through the project `opencode.json`.
- The live headless Editor exposes Pipeline on port `7800`.
- EditMode magnetic-routing tests pass: `3/3`.
- Play Mode starts with no game-code console errors.
- The Prologue runtime generates its cinematic UI, imported GLB worlds, first-person controller, post-processing, particles, and ambience successfully.
- Pipeline-driven story smoke flow reaches title, dinner, dialogue choice, bedroom door interaction, lab terminal, all nine puzzle tiles, protected B2 feedback, the authored solution, mission file, Alice's note, and the Enceladus ending.
- The `BeginMissionButton` is active and interactable and successfully loads the Gameplay scene.
- Runtime lit, unlit, transparent, particle, and panoramic skybox shader variants are bundled through Resources materials.
- Standalone macOS player build succeeds with `0` errors and `0` warnings.
- Built macOS player starts, loads assemblies and the first scene, then shuts down cleanly in the smoke run.
- Unity Windows Build Support (Mono) is installed for Editor `6000.3.24f1`.
- Standalone Windows 64-bit build succeeds with `0` errors. Its 18 warnings are unsupported ray-tracing shader variants that are not used by this URP game.
- Windows executable and `UnityPlayer.dll` are confirmed `PE32+` x86-64 binaries.
- Shareable Windows ZIP passes `unzip -t` integrity validation and contains all required runtime files.

## Build Results

| Target | Result | Output | Size |
| --- | --- | --- | --- |
| macOS Universal | Succeeded | `Builds/COSMIC RAYS.app` | 132,772,216 bytes |
| macOS ZIP | Valid | `Builds/COSMIC-RAYS-macOS.zip` | 51,110,245 bytes |
| Windows x64 | Succeeded | `Builds/Windows/COSMIC RAYS - Areeba's Promise.exe` | 114,301,799 bytes |
| Windows ZIP | Valid | `Builds/COSMIC-RAYS-Windows-x64.zip` | 42,263,483 bytes |

Package SHA-256:

```text
macOS:  3af7d5fc2e8fd88690c6d65101104bb9016145584525ee829026b6724d58b823
Windows: 8be51ecce9e694f81e00ac8f94315915d38196c1b6cde2d2ce7a45d32dcf05d2
```

Both targets use the same scene order: Prologue, MainMenu, Gameplay.

## Current Limitations

- Environments use imported GLB models plus generated lighting and effects; animated character avatars are not yet present.
- There is no final character animation, voice acting, composed soundtrack, or AudioMixer routing.
- The full Enceladus campaign systems exist, but its final 3D activities still need designer-owned integration.
- The Windows player was built and archive-validated on macOS; native launch testing still needs a Windows 10/11 machine.
- Headless automation verified behavior, but final visual framing still needs review from a user-recorded playthrough.
- On this Intel macOS 26 host, the GUI Editor stalls during startup. A persistent `-batchmode -nographics` Editor works correctly with Unity Pipeline and MCP.
