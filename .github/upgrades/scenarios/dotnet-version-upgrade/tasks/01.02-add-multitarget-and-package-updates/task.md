# 01.02-add-multitarget-and-package-updates: Add net10.0 target and conditional package updates for Common

## Objective
Add net10.0 target to Common project (TargetFrameworks) and update or conditionalize PackageReference entries so the project can build for both net472 and net10.0 without causing conflicts for downstream .NET Framework projects.

## Steps
1. Change TargetFramework to TargetFrameworks: net472;net10.0.
2. Add conditional PackageReference elements with Condition on '$(TargetFramework)' where package versions differ between frameworks.
3. Remove or mark framework-provided packages for net10.0 using Condition attribute.
4. Add System.Resources.Extensions package or set GenerateResourceUsePreserializedResources as needed for non-string resources when building net10.0.
5. Run targeted builds for both TFM targets and iterate until build warnings/errors are resolved per the build-fix loop.

## Done when
- Common builds cleanly for net472 and net10.0 (0 errors, warnings addressed for touched projects).
