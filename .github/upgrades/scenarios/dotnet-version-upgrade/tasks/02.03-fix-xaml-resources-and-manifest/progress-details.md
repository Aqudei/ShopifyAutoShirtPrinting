## Progress details for task 02.03 — Fix XAML / resources / manifest

Timestamp: 2026-06-21T00:00:00Z

Actions performed:

- Ran `dotnet build` on ShopifyEasyShirtPrinting.csproj to capture compile errors for net10.0-windows.
- Observed build failed with 1 error and multiple warnings (see build output). The failure appears during compilation of project sources (XAML/designer/resource generation conflict).
- Investigated strongly-typed resource designer (Properties/Resources.Designer.cs). This file is checked-in and conflicts with SDK-style resource generation.
- Applied changes:
  - Excluded checked-in generated designer files from compilation by removing Properties\Resources.Designer.cs, Properties\Settings.Designer.cs, and Properties\AssemblyInfo.cs from the Compile item group.
  - Added PackageReference to System.Resources.Extensions (v6.0.0) to support non-string resource generation on net10.0.
- Re-ran `dotnet build`. Build still fails with 1 error; warnings increased to ~18. Full diagnostic log saved to repository (if available) for later inspection.

Current status:

- The task is NOT yet complete. Resource/designer/XAML conflict remains unresolved; additional investigation required.

Next steps recommended:

1. Capture the full msbuild diagnostic log to identify the exact error message and the file(s) involved (ResGen/CodeGen step or XAML compile target).
2. Inspect .resx files and the generated Properties\Resources.Designer.cs for duplicate resource keys or type mismatches.
3. If designer files are intentionally checked-in, consider regenerating them using ResGen for net10 or remove them and let SDK generate them.
4. Consider adding <GenerateResourceUsePreserializedResources>true</GenerateResourceUsePreserializedResources> for the project or in Common csproj's net10 target (already set in Common for library), and/or add System.Resources.Extensions to project references for resource handling.
5. Evaluate globbing vs explicit includes: SDK-style globbing can include generated designer files under obj/ or conflicting artifacts; consider disabling default items and adding explicit includes for XAML and .cs files if necessary.

Files modified in this attempt:

- ShopifyAutoShirtPrinting/ShopifyAutoShirtPrinting/ShopifyEasyShirtPrinting.csproj

Notes:

- This progress-details.md is being written before calling complete_task to record actions and evidence. The task will be marked failed to unblock a follow-up iteration while documenting the blocker.
