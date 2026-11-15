# Unity Gameplay Package Mapping

## Source Reviewed

`/Users/macbook/Downloads/COSMIC_RAYS_FINAL_GAMEPLAY_PACKAGE`

The package targets Unity 6.3 LTS and contains 17 C# source files plus setup and handoff documentation. It does not contain a Unity project, generated scenes, models, character art, environment art, animation, or audio. Its own testing report states that it has not been compiled or run in Unity.

## Integration Decision

The current playable prototype remains browser-based because no Unity Editor is installed on this machine. The Unity C# files are not copied into the web project because they cannot execute there. Their system rules, stable IDs, and data boundaries are retained as the design source for later implementation or migration.

## Adopted Now

- SPA-077 collection and SEP-MPS-269 remain clearly marked experimental.
- The terminal now requires a magnetic-containment calibration puzzle before releasing the mission file.
- Gameplay rules live separately from rendering so they can be ported to Unity later.
- Automated tests verify puzzle connectivity independently from the interface.

## Planned System Mapping

| Unity package system | Browser equivalent | Planned milestone |
| --- | --- | --- |
| `SurvivalConfig` | Validated resource configuration object | Enceladus chapter |
| `SurvivalStatus` | Oxygen, water, and energy runtime model | Enceladus chapter |
| `InventoryState` | Stable item IDs and stack quantities | Enceladus chapter |
| `MissionProgress` | Stable objective IDs and completion set | Chapter framework |
| `CosmicRaysSaveData` | Versioned plain JSON snapshot | Save/resume milestone |
| `CosmicRaysSaveSystem` | Validated `localStorage` persistence with recovery copy | Save/resume milestone |
| Six month objectives | Six Enceladus chapter milestones | Full campaign |

## Stable Mission IDs

The following IDs from the Unity package are reserved unchanged:

1. `month_01_inspect_base`
2. `month_02_life_support`
3. `month_03_energy_heating`
4. `month_04_sep_technology`
5. `month_05_habitat_research`
6. `month_06_final_assessment`

Prototype-only objectives use a separate `prologue_` prefix so they cannot collide with campaign saves.

## Unity Migration Gate

Move the playable build into Unity only after all of the following are true:

- Unity 6.3 LTS and UGUI are installed.
- The package compiles in an initialized Unity project.
- The setup utility generates and validates its scenes and data assets.
- The team has final 3D environments, player controls, and animation assets.
- The browser vertical slice has been playtested and its core interaction is approved.

Until that gate is met, browser development gives the team a testable game instead of an unverified engine scaffold.
