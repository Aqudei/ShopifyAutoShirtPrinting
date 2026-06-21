## Progress details for task 02.04 — Iterate builds and fix API issues

Timestamp: 2026-06-21T00:00:00Z

Summary

- Goal: iterate builds on the WPF app (net10.0-windows) and fix API/compile errors so app can smoke-launch.
- Action taken: attempted diagnostic MSBuild run and captured a build log to .github/.../build-wpf.log.

Findings

- Build failed immediately with MSBuild SDK resolution errors. Key messages (from build log):
  - MSB4276: The default SDK resolver failed to resolve SDK "Microsoft.NET.SDK.WorkloadAutoImportPropsLocator" because directory "C:\Program Files\dotnet\sdk\10.0.301\Sdks\Microsoft.NET.SDK.WorkloadAutoImportPropsLocator\Sdk" did not exist.
  - MSB4276: The default SDK resolver failed to resolve SDK "Microsoft.NET.SDK.WorkloadManifestTargetsLocator" because directory "C:\Program Files\dotnet\sdk\10.0.301\Sdks\Microsoft.NET.SDK.WorkloadManifestTargetsLocator\Sdk" did not exist.

Diagnosis

- These errors indicate the local .NET SDK installation is missing workload-related SDK components required by the .NET 10 SDK (10.0.301) that MSBuild attempted to use. In practice this means either:
  1. The required .NET 10 SDK (matching the project's target) is not installed on the machine, or
  2. The SDK is installed but some workload components (Windows Desktop / workload locator pieces) are not present or the SDK installation is corrupted.

Why this blocks code work

- The build fails during SDK resolution before any C# compile/XAML/resgen steps run, so further source-level fixes (API replacement, XAML changes, resource adjustments) cannot be exercised until the SDK/workload problem is resolved.

Recommended remediation steps (local developer machine)

1. Inspect the local SDKs and workloads:
   - dotnet --info
   - dotnet --list-sdks
   - dotnet workload list

2. If .NET 10 SDK is not installed, install it (recommended: the matching patch version shown in the log, e.g., 10.0.301) via the official installer from Microsoft.

3. If the .NET 10 SDK is installed but workloads are missing or the SDK appears corrupted, reinstall the SDK or install the missing workloads. Example commands (run in an elevated developer shell):
   - dotnet workload install windowsdesktop
   - dotnet workload repair
   - If those commands fail or are not applicable, reinstall the .NET 10 SDK using the official installer.

4. After installing/repairing the SDK and workloads, re-run from repository root:
   - dotnet restore ShopifyAutoShirtPrinting.sln
   - dotnet build ShopifyAutoShirtPrinting/ShopifyEasyShirtPrinting.csproj -c Debug

5. If issues persist, capture and attach the new build log (msbuild /v:diag /fileLogger) and share it for the next iteration.

Files changed during this attempt

- none (read-only diagnostic + logs). Progress artifacts created:
  - .github/upgrades/scenarios/dotnet-version-upgrade/build-wpf.log (diagnostic msbuild log)
  - This progress-details.md

Status

- BLOCKED by local SDK/workload installation issue. No source-code edits were made because the failure occurs before compile.

Next actions for the agent once environment is fixed

1. Re-run the build to capture source compile/XAML/resgen errors.
2. Enter the build-fix loop: fix the first error, rebuild, iterate until clean or escalate.
3. If resource/designer conflicts remain, follow the previously recommended fixes (GenerateResourceUsePreserializedResources, exclude checked-in designer files, add System.Resources.Extensions where needed).

Notes

- This is an environment blocker, not a code blocker. Addressing local SDK/workload installation will unblock task progress.


