## Files Modified
- Common\Common.csproj — added conditional package references per TFM and GenerateResourceUsePreserializedResources for net10.0
- Common\Models\Seo\SEOAudit.cs — wrapped System.Web.Configuration using in NETFRAMEWORK conditional

## Build Result
- Built: Common (net10.0)
- Errors: 0
- Warnings: 19 (mostly nullable warnings and package/assembly unification warnings)

## Test Result
- No tests run in this subtask

## Changes Summary
- Added conditional PackageReference elements to separate net472-only packages (PdfiumViewer, framework polyfills) from net10-only package versions (System.Text.Json, encodings, runtime unsafe, Microsoft.Bcl.* updates). Added System.Resources.Extensions for net10 to address resource generation requirements.
- Wrapped System.Web.Configuration using with #if NETFRAMEWORK to avoid net10 build referencing System.Web.

## Issues Encountered
- Several assembly unification warnings remain due to projects referencing older package assets; these are expected during transition and will be addressed when dependent projects (WPF) are migrated or packages are updated.

## Next Steps
- Proceed to convert or locate UnitTestProject1 (01.03) and migrate tests to net10.0, then migrate the WPF project.
- Alternatively, start WPF migration earlier to reduce cross-target interference.
