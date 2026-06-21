# 02.03-fix-xaml-resources-and-manifest: Address XAML compilation, resources, and manifest differences for SDK-style WPF

## Objective
Resolve build-time XAML compilation and resource generation differences introduced by SDK-style and newer SDKs. Update app.manifest, resource build actions, and any required System.Resources.Extensions references.

## Steps
1. Build project and capture XAML/ResX errors.
2. If non-string resources cause MSB3822/MSB3823, add System.Resources.Extensions package or adjust GenerateResourceUsePreserializedResources per TFM.
3. Ensure app.manifest and signing settings are valid or temporarily disable signing during migration.
4. Verify that primary windows and App.xaml compile.

## Done when
- Project builds past XAML/resource compilation stage for net10.0 (errors resolved or documented).
