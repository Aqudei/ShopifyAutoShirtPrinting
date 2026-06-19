# .NET Version Upgrade Progress

## Overview

Upgrading 3 projects from .NET Framework 4.7.2 to .NET 10.0. Strategy: convert projects to SDK-style, update packages, fix API breaks, and validate.

**Progress**: 1/7 tasks complete <progress value="14" max="100"></progress> 14%

## Tasks

- ✅ 01-convert-common-to-sdk: Convert Common\Common.csproj to SDK-style and target net10.0 ([Content](tasks/01-convert-common-to-sdk/task.md), [Progress](tasks/01-convert-common-to-sdk/progress-details.md))
- 🔲 02-convert-shopify-wpf-to-sdk-and-net: Convert ShopifyAutoShirtPrinting\ShopifyEasyShirtPrinting.csproj to SDK-style and target net10.0-windows
- 🔲 03-convert-tests-and-update-packages: Convert UnitTestProject1 to SDK-style and target net10.0
- 🔲 04-update-nuget-packages: Update incompatible and recommended NuGet packages
- 🔲 05-code-fixes-for-api-breaks: Apply code changes to address API incompatibilities
- 🔲 06-integration-and-local-validation: Build, run tests, and validate the app
- 🔲 07-finalize-and-cleanup: Cleanup, update instructions, and commit
