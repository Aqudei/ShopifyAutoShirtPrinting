# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Strategy
- Per-project sequential upgrade: convert libraries and tests first, then migrate the WPF app, update packages, fix compile/runtime issues, validate with tests.

## Source Control
- **Source Branch**: dotnet-version-upgrade-ShopifyEasyShirtPrinting-net10
- **Working Branch**: dotnet-version-upgrade-ShopifyEasyShirtPrinting-net10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Notes
- Initialized per user confirmation on upgrade to .NET 10 (net10.0).