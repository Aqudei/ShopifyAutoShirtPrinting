## Files Modified
- ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj — replaced legacy csproj with minimal SDK-style WPF project targeting net10.0-windows, UseWPF=true, and a ProjectReference to ..\Common\Common.csproj

## Restore/Build Result
- dotnet restore: succeeded (warnings)
- dotnet build: failed for net10.0-windows (1 error, 4 warnings). The error occurred during compilation of many source files; root cause to be investigated during package and code migration (02.02+).

## Changes Summary
- Created an SDK-style project file that glob-includes .cs and .xaml sources so the project loads in the IDE.
- Deferred package conversion and per-file fixes to subsequent subtasks.

## Issues Encountered
- Build failed with a single compilation error after scaffold. This is expected until package references, conditionalization, and API fixes are applied.
- WPF app contains many references and hint-paths to packages that are not yet converted to PackageReference; 02.02 will address those.

## Next Steps
1. Execute subtask 02.02 to convert HintPath references to PackageReference and condition incompatible packages.
2. Resolve compile-time errors surfaced by the build in 02.03/02.04.

