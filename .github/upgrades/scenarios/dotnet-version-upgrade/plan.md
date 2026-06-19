# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the solution projects from .NET Framework 4.7.2 to .NET 10.0 (net10.0)
**Scope**: 3 projects (~13k LOC). Major effort focused on the WPF app (ShopifyEasyShirtPrinting.csproj) which requires SDK-style conversion and Windows-specific target (net10.0-windows).

## Tasks

### 01-convert-common-to-sdk
Convert Common\Common.csproj to SDK-style and target net10.0.

**Done when**: Common project uses SDK-style csproj, targets net10.0, builds without errors and all unit tests referencing it compile.

---

### 02-convert-shopify-wpf-to-sdk-and-net
Convert ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj to SDK-style, target net10.0-windows, and update WPF-specific settings.

**Done when**: Project file is SDK-style targeting net10.0-windows with <UseWindowsForms> or <UseWPF> as required, builds without errors, and UI launches locally.

---

### 03-convert-tests-and-update-packages
Convert UnitTestProject1 to SDK-style, target net10.0, update test framework packages, and ensure tests run.

**Done when**: Unit test project targets net10.0, tests run successfully on local machine.

---

### 04-update-nuget-packages
Update incompatible and recommended NuGet packages across projects to versions compatible with net10.0. Document any packages without compatible versions.

**Done when**: All package references are updated or documented as blockers; solution restores successfully.

---

### 05-code-fixes-for-api-breaks
Apply necessary code changes to address binary and source-incompatible APIs identified in assessment (WPF and System.Drawing usage, Dispatcher calls, etc.).

**Done when**: Solution builds without errors and all modified projects compile warning-free.

---

### 06-integration-and-local-validation
Build solution, run unit tests, launch WPF app to validate major scenarios, and fix runtime behavioral issues found during testing.

**Done when**: Solution builds, tests pass, and main application workflows validated manually.

---

### 07-finalize-and-cleanup
Run code cleanup, ensure no warnings, update scenario-instructions.md with any decisions, and commit final changes.

**Done when**: No compiler warnings, scenario-instructions.md updated, and changes committed per commit strategy.

