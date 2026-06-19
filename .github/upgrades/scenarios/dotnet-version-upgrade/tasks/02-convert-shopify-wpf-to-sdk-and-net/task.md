# 02-convert-shopify-wpf-to-sdk-and-net: Convert ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj to SDK-style, target net10.0-windows, and update WPF-specific settings.

Convert ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj to SDK-style, target net10.0-windows, and update WPF-specific settings.

**Done when**: Project file is SDK-style targeting net10.0-windows with <UseWindowsForms> or <UseWPF> as required, builds without errors, and UI launches locally.

## Research / Findings

- Project path: ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj
- Current project format: legacy (non-SDK) MSBuild with TargetFrameworkVersion v4.7.2
- packages.config present at ShopifyAutoShirtPrinting\packages.config — packages will need migration to PackageReference later (out of scope for format conversion, but recorded)
- Project contains WPF/XAML, app.manifest, signed manifests, embedded resources, and many .resx/references which typically require msbuild.exe (full VS) for reliable build after conversion
- Many references and packages target .NET Framework TFMs (net47x). Expect to keep functionality and then retarget to net10.0-windows in a follow-up step after format conversion
- Notable dependencies that may need attention when retargeting: MahApps.Metro, Prism.Wpf, PdfiumViewer, Magick.NET, System.Drawing.Common, various Microsoft.* packages

## Plan (per-skill constraints)

1. Convert project to SDK-style format only (do NOT change TargetFramework during conversion) using the SDK-style conversion tool.
2. Build the converted project (use msbuild.exe because of WPF/XAML and embedded resources) and fix conversion-caused issues if any.
3. Once format conversion is validated, update TargetFramework to net10.0-windows and resolve new compile/runtime issues.
4. Record all changes in progress-details.md and commit after successful validation.

## Affected files (initial)

- ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj
- ShopifyAutoShirtPrinting\packages.config

-- End of research
