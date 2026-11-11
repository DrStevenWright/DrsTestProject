# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

DrsTestProject is the UE 5.8 host project for developing and testing **DrsFramework**, an XR framework (Meta Quest over Link and standalone Android; Pico as a cross-platform test case). The framework lives in a separate repo, included as a git submodule at `Plugins/DrsFramework`. Almost all real code is there; this repo holds test content (`Content/Maps/DrsTestMap`), config and an empty game module.

**For the framework's architecture, layer rules and conventions, see `Plugins/DrsFramework/CLAUDE.md`.**

- `Plugins/Dev/` is for dev-only plugins, such as MCP tooling. **DrsFramework must never depend on anything in `Plugins/Dev/` or on this project.**
- The project enables only `DrsCore`; it enables the rest of the framework through its `.uplugin` dependencies. The `.uproject` also enables the editor plugins for MCP (`ModelContextProtocol`, `AllToolsets`, `Terminal`). These bring in GameFeatures, which is why `DefaultGame.ini` scans `GameFeatureData` assets.
- `Config/DefaultEngine.ini` sets `DrsGameMode` as the global game mode and sets up XR rendering: forward shading, MSAA, instanced stereo / mobile multi-view, no Lumen, no virtual shadow maps. It also has the Android/Quest packaging settings (arm64, Vulkan, SDK 32).

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

## Git workflow

- Use Conventional Commits for branch names (`feat/…`, `fix/…`, `chore/…`, `docs/…`, `refactor/…`, `test/…`, `ci/…`), commit subjects and PR titles (`feat(scope): summary`).
- **Framework changes go in the submodule first.** The submodule is often on a detached HEAD, so check out a branch inside `Plugins/DrsFramework` before committing. Commit there, open a PR against DrsFramework `main`, then update the submodule pointer in this repo on its own branch and PR.

## Tooling

`.mcp.json` registers the `unreal-mcp` server at `http://127.0.0.1:8000/mcp`. It is only reachable while the editor is running.
