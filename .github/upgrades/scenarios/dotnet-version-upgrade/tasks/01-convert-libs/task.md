# 01-convert-libs: Convert Common library to SDK-style and multi-target

Convert Common\Common.csproj to multi-target (net48;net10.0), update PackageReference entries where framework-provided, and ensure net10.0 build is clean.

## Research Findings

### Projects Affected
- Common\Common.csproj — convert to multi-targeting (net48;net10.0). Project is already SDK-style.

### Files to Modify
- Common\Common.csproj — replace TargetFramework with TargetFrameworks and conditionally include framework-only references and packages for net48.

### Packages to Update / Conditionalize
| Package | Current | Decision |
|---------|---------|----------|
| PdfiumViewer | 2.13.0 | Keep for net48 only (incompatible with net10.0) — Condition PackageReference to net48
| PdfiumViewer.Native.x86_64.v8-xfa | 2018.4.8.256 | Keep for net48 only
| System.Buffers, System.Memory, System.Numerics.Vectors, System.Threading.Tasks.Extensions, System.ValueTuple | various | Remove for net10.0 (framework provides) — condition to net48

### API Changes / Migration Patterns
- No code-level API changes made in this task. This step focuses on project file multi-targeting and package scoping. Subsequent tasks will address API fixes.

### Dependencies & Risks
- Multi-targeting to net10.0 may expose binary incompatible APIs; this task avoids code changes to minimize immediate breakage and scopes legacy references to net48.
- Some PackageReferences may still be incompatible with net10.0 and will be addressed in later tasks.

### Decisions Made
- Use TargetFrameworks="net48;net10.0" (prefer net48 to maintain compatibility with existing consumers per plan).
- Condition legacy .NET Framework-only references and packages on net48 to prevent adding incompatible dependencies to net10.0 target.

## Next Steps
1. Build Common project for net10.0 and fix any compilation issues introduced by multi-targeting.
2. If build issues are extensive, recommend decomposition into finer-grained subtasks (package-by-package updates).

## Done when
- Common builds for both target frameworks and unit tests (if present) pass.
