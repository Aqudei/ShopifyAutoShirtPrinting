# 02.01-scaffold-sdk-project: Create new SDK-style WPF csproj that targets net10.0-windows and references existing source

## Objective
Create a new SDK-style project file for ShopifyEasyShirtPrinting that targets net10.0-windows and enables WPF (UseWPF=true). Do not modify source code yet; ensure project file loads in the IDE.

## Steps
1. Create SDK-style csproj (Project Sdk="Microsoft.NET.Sdk.WindowsDesktop").
2. Set TargetFramework to net10.0-windows and UseWPF to true.
3. Add ProjectReference to ..\Common\Common.csproj and include existing source file globs.
4. Preserve important properties: AssemblyName, RootNamespace, StartupObject, LangVersion.
5. Open project in IDE (or run dotnet msbuild /t:Restore) to validate load.

## Done when
- New SDK-style project file exists and restores without fatal errors; project loads in the IDE (no csproj load failure).
