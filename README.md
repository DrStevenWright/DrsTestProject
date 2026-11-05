# DrsTestProject

The development and test environment for [DrsFramework](https://github.com/DrStevenWright/DrsFramework),
on Unreal Engine 5.8. It holds test content and dev-only plugins (in `Plugins/Dev/`) that the
framework itself must never depend on. DrsFramework development happens in this project's
`Plugins/DrsFramework` submodule.

## Getting started

Requires Unreal Engine 5.8 (launcher install, or a registered source build such as 5.8-drs).

```
git clone --recurse-submodules https://github.com/DrStevenWright/DrsTestProject.git
cd DrsTestProject
powershell -ExecutionPolicy Bypass -File Plugins/DrsFramework/Tools/Bootstrap.ps1 -Build
```

Then open `DrsTestProject.uproject`.

## Running the framework tests

In the editor: Tools > Session Frontend > Automation, filter on `Drs`. Or from the command line:

```
UnrealEditor-Cmd.exe <path>/DrsTestProject.uproject -ExecCmds="Automation RunTests Drs; Quit" -unattended -nullrhi -nosplash
```

## Licence

MIT. See [LICENSE](LICENSE).
