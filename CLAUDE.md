# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

DrsTestProject is the UE 5.8 host project for developing and testing **DrsFramework**, an XR framework (Meta Quest over Link and standalone Android). The framework lives in a separate repo, included as a git submodule at `Plugins/DrsFramework`. Almost all real code is there; this repo holds test content (`Content/Maps/DrsTestMap`), config and an empty game module.

**For the framework's architecture, layer rules and conventions, see `Plugins/DrsFramework/CLAUDE.md`.**

- `Plugins/Dev/` is for dev-only plugins, such as MCP tooling. **DrsFramework must never depend on anything in `Plugins/Dev/` or on this project.**
- The project enables only `DrsCore`; it enables the rest of the framework through its `.uplugin` dependencies. The `.uproject` also enables the editor plugins for MCP (`ModelContextProtocol`, `AllToolsets`, `Terminal`). These bring in GameFeatures, which is why `DefaultGame.ini` scans `GameFeatureData` assets.
- `Config/DefaultEngine.ini` sets `/DrsCore/BP_DrsGameMode` as the global game mode and `/DrsCore/BP_DrsGameInstance` as the game instance (both Blueprints live in DrsCore's plugin content), and sets up XR rendering: forward shading, MSAA, instanced stereo / mobile multi-view, no Lumen, no virtual shadow maps. It also has the Android/Quest packaging settings (arm64, Vulkan, SDK 32).

## Commands

Run these from the project root in PowerShell. Bootstrap fetches submodules, finds the engine from `EngineAssociation` (launcher install or a registered source build), generates project files and builds `DrsTestProjectEditor` Win64 Development:

```
powershell -ExecutionPolicy Bypass -File Plugins/DrsFramework/Tools/Bootstrap.ps1 -Build
```

Options: `-EngineDir <path>`, `-SkipSubmodules`, `-ProjectFile <path>`. To rebuild without regenerating project files:

```
& "<EngineDir>\Engine\Build\BatchFiles\Build.bat" DrsTestProjectEditor Win64 Development -Project="<root>\DrsTestProject.uproject" -WaitMutex
```

Run the framework tests. They are UE Automation tests named `Drs.*`. `RunTests` takes a name prefix, so pass a full test name to run a single test (e.g. `Drs.Globals.Version`):

```
UnrealEditor-Cmd.exe <root>/DrsTestProject.uproject -ExecCmds="Automation RunTests Drs; Quit" -unattended -nullrhi -nosplash
```

In the editor: Tools > Session Frontend > Automation, filter on `Drs`.

Check the framework's layer rules (run inside `Plugins/DrsFramework`; exits 1 on a violation):

```
powershell -ExecutionPolicy Bypass -File Tools/CheckLayers.ps1
```

## Code style

All code in this project and in DrsFramework follows [`Plugins/DrsFramework/Docs/CodingConventions.md`](Plugins/DrsFramework/Docs/CodingConventions.md). In short: Epic's standard with the `Drs` prefix, except that arguments and locals are camelCase with no `b`, and every code file starts with the MIT copyright line.

## Git workflow

Follow [`Plugins/DrsFramework/Docs/GitWorkflow.md`](Plugins/DrsFramework/Docs/GitWorkflow.md) for branch names, commit subjects, scopes, PR titles and descriptions, and squash merging. In short:

- Conventional Commits everywhere (`feat(core): summary`), with no breaking-change markers while the framework's major version is 0.
- **Framework changes go in the submodule first,** on a branch from `origin/main` (the submodule is often on a detached HEAD), with their own PR against DrsFramework `main`.
- **Update the submodule pointer here only after that PR merges,** to the squash commit on DrsFramework `main`, on its own branch and PR.

## Tooling

The editor's MCP server (`http://127.0.0.1:8000/mcp`) comes from the `unreal-mcp-status` plugin, which `.claude/settings.json` enables from the `claude-mods` marketplace. It is only reachable while the editor is running. As a plugin server it shows as `plugin:unreal-mcp-status:unreal-mcp`, its tools are `mcp__plugin_unreal-mcp-status_unreal-mcp__*`, and the plugin connects it by itself when the editor starts after the session (or run `/unreal-connect`). Don't add `unreal-mcp` back to a project `.mcp.json`: a project server of the same name hides the plugin's and can't be reconnected in a running session.
