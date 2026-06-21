# 01.01-convert-common-to-sdk: Convert Common project to SDK-style without changing TFM

## Objective
Convert Common\Common.csproj to a clean SDK-style csproj while preserving net472 compatibility.

## Steps
1. Ensure project uses Sdk attribute and globbing for Compile items.
2. Keep TargetFramework as net472 (no net10 target in this subtask).
3. Replace packages.config with PackageReference if present (not applicable here).
4. Save task.md research findings and run a targeted build for Common (net472) to verify no regressions.

## Done when
- Common.csproj is a cleaned SDK-style project, builds for net472 without errors or warnings in touched projects.
