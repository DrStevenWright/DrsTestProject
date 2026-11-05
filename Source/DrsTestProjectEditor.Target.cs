// Copyright (c) 2026 Steven Wright. Licensed under the MIT License.

using UnrealBuildTool;
using System.Collections.Generic;

public class DrsTestProjectEditorTarget : TargetRules
{
	public DrsTestProjectEditorTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V7;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("DrsTestProject");
	}
}
