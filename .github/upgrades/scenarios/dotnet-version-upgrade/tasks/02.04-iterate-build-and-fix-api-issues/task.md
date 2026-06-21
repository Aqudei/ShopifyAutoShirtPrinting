# 02.04-iterate-build-and-fix-api-issues: Iterate builds, fix API-breaking issues, and validate runtime smoke-launch

## Objective
Fix remaining compile errors caused by API changes, update code to use modern APIs, and perform a smoke-launch to verify runtime behavior.

## Steps
1. Run build-fix loop targeting the project and its dependents.
2. Address missing namespace or API changes (wrap System.Web usages, replace System.Drawing usage where needed).
3. Run the app manually (smoke-launch) and note runtime exceptions in startup path.
4. Update code or document required native dependencies (e.g., Pdfium native bindings).

## Done when
- Project builds and a smoke-launch starts the app without immediate startup exceptions, or remaining runtime issues are documented as blockers.
