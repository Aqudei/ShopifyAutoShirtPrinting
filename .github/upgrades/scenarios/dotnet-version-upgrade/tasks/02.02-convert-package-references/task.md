# 02.02-convert-package-references: Convert References/HintPath to PackageReference where possible and condition incompatible packages

## Objective
Replace legacy HintPath-based Reference elements with PackageReference entries or conditional references. For packages without net10 support, add conditional entries or plan replacements.

## Steps
1. Inspect existing Reference and packages.config (if any).
2. For each package present in assessment with net10-compatible versions, add PackageReference with appropriate version.
3. For packages with no compatible version (e.g., PdfiumViewer, some MahApps.IconPacks), add conditional references for net472 only or document alternatives.
4. Run dotnet restore and note unresolved packages.

## Done when
- Project uses PackageReference for modern packages and conditionalizes/removes net472-only packages; restore succeeds or blockers are documented.
