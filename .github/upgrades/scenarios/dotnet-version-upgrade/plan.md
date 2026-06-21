# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade solution projects to net10.0 (.NET 10, LTS).
**Scope**: 3 projects (Common, ShopifyEasyShirtPrinting WPF app, UnitTestProject1). Large WPF app has extensive package and API incompatibilities; expect most work concentrated there.

## Tasks

### 01-convert-libraries-and-tests-to-sdk-style
Convert library and test projects (Common, UnitTestProject1) from old CSProj format to SDK-style projects and update TargetFramework to net10.0.

**Done when**: Both projects use SDK-style csproj, target net10.0, and compile without errors.

---

### 02-migrate-wpf-app-to-sdk-style-and-tfm
Migrate ShopifyEasyShirtPrinting (WPF) to SDK-style project with <UseWPF>true</UseWPF>, set TargetFramework to net10.0, and apply necessary project-system changes (PackageReference, framework references, RID settings as needed).

**Done when**: ShopifyEasyShirtPrinting.csproj is SDK-style, targets net10.0, and the project loads in the IDE (no csproj load errors).

---

### 03-update-nuget-packages-and-replace-incompatible-packages
Update NuGet references to versions that support net10.0 where available. For packages without net10 support, document alternatives or isolate them (e.g., native Pdfium bindings, MahApps.IconPacks compatibility). Key packages called out in assessment: MahApps.Metro/IconPacks family, PdfiumViewer, ServiceStack.Text, Netco, Magick.NET (security update), Microsoft.Extensions.* packages, System.Drawing.Common (platform changes).

**Done when**: All packages that offer net10-compatible versions are updated and the projects restore successfully. Any packages with no compatible version are documented as blockers with recommended alternatives.

---

### 04-address-API-breaking-changes-and-compile-errors
Resolve source/binary incompatible APIs surfaced in the assessment (high-impact areas in the WPF app). Apply API replacements, refactors, and conditional compilation where appropriate.

**Done when**: Solution builds without compilation errors and there are no remaining CS errors introduced by the TFM change.

---

### 05-wpf-runtime-and-platform-fixes
Apply runtime adjustments specific to WPF and platform APIs (e.g., System.Drawing.Common usage, native dependencies, threading/Dispatch changes, XAML assembly binding changes). Verify major UI components compile and common startup paths do not throw on launch.

**Done when**: WPF project builds and a smoke-launch (manual run) starts the app without immediate runtime exceptions in primary startup path. If a full runtime validation cannot be automated, document manual smoke test steps in the task's progress-details.

---

### 06-run-tests-and-integration-validation
Execute unit tests and key integration scenarios. Fix failing tests and update test framework packages if needed.

**Done when**: All unit tests pass, and integration smoke tests complete (or any remaining failures are documented with root cause and remediation plan).

---

### 07-cleanup-and-documentation
Remove obsolete package references, tidy csproj files, update README and developer notes with new TFM and any environment setup instructions (e.g., required .NET SDK version), and create a short migration notes file describing known issues and workarounds.

**Done when**: Repo contains updated documentation and no leftover obsolete references; scenario artifacts updated (tasks.md, scenario-instructions.md) and a final progress-details.md is prepared.

---

### Execution Order
1. 01-convert-libraries-and-tests-to-sdk-style
2. 02-migrate-wpf-app-to-sdk-style-and-tfm
3. 03-update-nuget-packages-and-replace-incompatible-packages
4. 04-address-API-breaking-changes-and-compile-errors
5. 05-wpf-runtime-and-platform-fixes
6. 06-run-tests-and-integration-validation
7. 07-cleanup-and-documentation

---

Please review. In Automatic mode I will proceed to execution (start tasks) unless you say "pause". If you want changes to the plan (split tasks, reorder, or add blockers), specify adjustments now.