# Upgrade Options

This file lists the upgrade options evaluated for the solution and the recommended defaults. Edit this file to change selections (move `**{value}** (selected)` to another row) or reply in chat with your desired changes. Confirmation is required to proceed.

## Projects Analyzed
- Common\Common.csproj
- ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj

## Target Framework
- **net10.0** (selected)
- net9.0
- net8.0
- net11.0 (preview)

## Upgrade Strategy
- All-at-once
- **Bottom-up** (selected)  # Recommended: convert libraries first, then apps
- Top-down

## Project Conversion Approach
- Convert libraries to SDK-style and target net10.0 directly
- **Convert libraries to SDK-style and multi-target (net48;net10.0)** (selected)  # Safer when libraries are consumed by unchanged apps during migration
- Use side-by-side / proxy for web projects where System.Web usage prevents direct porting

## Web Project Handling
- No special side-by-side migration (direct upgrade)  
- **Side-by-side web migration (YARP proxy + incremental migration)** (selected if needed)

## Package & Security Updates
- **Apply all security fixes and update incompatible NuGet packages where compatible versions exist** (selected)
- Defer security fixes (user must opt-out)

## Commit Strategy
- After Each Task (selected)
- After Each Phase
- Single Commit at End
- Manual

## Working Branch
- upgrade-dotnet-10 (selected)

## Rationale & Notes
- Assessment found API incompatibilities and NuGet incompatibilities in both projects. Bottom-up strategy reduces blast radius by upgrading libraries first (Common) and then the application project.
- Multi-targeting for libraries is recommended initially to keep the current .NET Framework consumers working while moving the application to net10.0.
- Security fixes will be applied by default.

---

Please review these options. Reply with `confirm` to accept as-is, or specify changes (e.g., "Use All-at-once", "Don't apply security fixes", "Use single commit").
