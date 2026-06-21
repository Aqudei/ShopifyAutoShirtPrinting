## Files Modified
- ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj — added PackageReference entries for common packages and globbed source/XAML includes.

## Restore/Build Result
- dotnet restore: succeeded with warnings (vulnerabilities noted for some package versions)
- dotnet build: failed (1 error, 17 warnings) — build error indicates additional code or API issues to resolve; many files attempted to compile.

## Changes Summary
- Replaced many legacy HintPath-based references by adding PackageReference entries for commonly used packages (Newtonsoft.Json, RestSharp, NLog, MahApps.Metro, Prism, EPPlus, Magick.NET, AutoMapper, AngleSharp, RabbitMQ.Client, SharpZipLib, Sprache, Microsoft.Xaml.Behaviors.Wpf).
- Noted PdfiumViewer as net472-only; left for later conditionalization or replacement.

## Issues Encountered
- A single compilation error remains (details shown during build). This is expected; subsequent subtasks will fix API mismatches, missing using directives, and resource/XAML compilation problems.

## Next Steps
- Run 02.03 to address XAML/resources and manifest differences.
- Iterate on package versions where vulnerabilities were reported.
