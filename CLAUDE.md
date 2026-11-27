# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

DrsTestProject is the UE 5.8 host project for developing and testing **DrsFramework**, an XR framework (Meta Quest over Link and standalone Android). The framework lives in a separate repo, included as a git submodule at `Plugins/DrsFramework`. Almost all real code is there; this repo holds test content (`Content/Maps/DrsTestMap`), config and an empty game module.

**For the framework's architecture, layer rules and conventions, see `Plugins/DrsFramework/CLAUDE.md`.**

- `Plugins/MetaXR` is the private MetaXR fork, a submodule on its `drs` branch ([DrStevenWright/MetaXR](https://github.com/DrStevenWright/MetaXR)). Clones without access won't fetch it, and everything still builds and runs without it. The `.uproject` lists `OculusXR` as disabled by default; enable it there (or in the editor's Plugins window) to use the fork. It is also marked `Optional`, so an enabled entry is skipped when the fork is missing. The fork is integrated with DrsFramework's XR arbiter, so its modules load only when the arbiter picks Meta (see `Plugins/DrsFramework/Platform/DrsXrPlatform/CLAUDE.md`). DrsFramework must never depend on it.
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

Run the framework tests. They are UE Automation tests named `Drs.*`. `RunTests` takes a name prefix, so pass a full test name to run a single test (e.g. `Drs.Base.Version`):

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

## The MetaXR fork

The fork keeps two lines of history:

- `main` holds Meta's releases only, one commit per release, tagged with the version (`v1.209.0`). Binaries, Intermediate and source `.fbx` files are left out, and files are stored in plain git, not LFS.
- `drs` is the fork, and the submodule tracks it. New releases are merged into it from `main` with a merge commit, never squashed: a squash drops the link to the release, so the next upgrade would re-conflict on files already resolved. The repo allows both squash and merge commits on GitHub, so pick per PR (rebase merging is off).

Every change on `drs` is tagged with a `DRS:` comment (`// DRS:` in code, `<!-- DRS: -->` in XML, `; DRS:` in ini files, a `DrsComment` key in the `.uplugin`), so search for `DRS:` to list them. Changes to binary assets are tagged in their commit message instead. Patches go to `drs` through pull requests that are **squash merged**, one commit per patch. Only the release merge from `main` uses a merge commit. Don't reformat Meta's code.

To upgrade, import the new release onto `main` and tag it, merge the tag into `drs`, then update the submodule pointer here. Merge conflicts appear only where a `DRS:` patch meets Meta's changes. Re-check the `.uplugin` after a merge: every new module needs `LoadingPhase` `None` and its real phase in `ArbitratedLoadingPhase`.

## Tooling

The editor's MCP server (`http://127.0.0.1:8000/mcp`) comes from the `unreal-mcp` plugin, which `.claude/settings.json` enables from the `drs-claude-tools` marketplace ([DrStevenWright/DrsClaudeTools](https://github.com/DrStevenWright/DrsClaudeTools)). It is only reachable while the editor is running. As a plugin server it shows as `plugin:unreal-mcp:unreal-mcp`, its tools are `mcp__plugin_unreal-mcp_unreal-mcp__*`, and the plugin connects it by itself when the editor starts after the session. If it doesn't, press Reconnect on Unreal MCP under Connectors (in a terminal session, use `/mcp`). Don't add `unreal-mcp` back to a project `.mcp.json`: a project server of the same name hides the plugin's and can't be reconnected in a running session.
