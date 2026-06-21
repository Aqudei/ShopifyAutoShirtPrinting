# Migration Progress

**Progress**: 4/12 tasks complete <progress value="33" max="100"></progress> 33%
**Status**: In Progress - Task 01-convert-libraries-and-tests-to-sdk-style

## Tasks

- 🔄 01-convert-libraries-and-tests-to-sdk-style: Convert Common and UnitTestProject1 to SDK-style and target net10.0 ([Content](tasks/01-convert-libraries-and-tests-to-sdk-style/task.md), [Progress](tasks/01-convert-libraries-and-tests-to-sdk-style/progress-details.md))
   - ✅ 01.01-convert-common-to-sdk: Convert Common project to SDK-style without changing TFM ([Content](tasks/01.01-convert-common-to-sdk/task.md), [Progress](tasks/01.01-convert-common-to-sdk/progress-details.md))
   - ✅ 01.02-add-multitarget-and-package-updates: Add net10.0 target and conditional package updates for Common ([Content](tasks/01.02-add-multitarget-and-package-updates/task.md), [Progress](tasks/01.02-add-multitarget-and-package-updates/progress-details.md))
   - 🔲 01.03-locate-or-create-unit-tests-and-convert: Locate UnitTestProject1 or create/convert test project and target net10.0 ([Content](tasks/01.03-locate-or-create-unit-tests-and-convert/task.md))
- 🔄 02-migrate-wpf-app-to-sdk-style-and-tfm: Migrate WPF app to SDK-style and set TargetFramework to net10.0 ([Content](tasks/02-migrate-wpf-app-to-sdk-style-and-tfm/task.md))
   - ✅ 02.01-scaffold-sdk-project: Create new SDK-style WPF csproj that targets net10.0-windows and references existing source ([Content](tasks/02.01-scaffold-sdk-project/task.md), [Progress](tasks/02.01-scaffold-sdk-project/progress-details.md))
   - ✅ 02.02-convert-package-references: Convert References/HintPath to PackageReference where possible and condition incompatible packages ([Content](tasks/02.02-convert-package-references/task.md), [Progress](tasks/02.02-convert-package-references/progress-details.md))
   - ❌ 02.03-fix-xaml-resources-and-manifest: Address XAML compilation, resources, and manifest differences for SDK-style WPF ([Content](tasks/02.03-fix-xaml-resources-and-manifest/task.md), [Progress](tasks/02.03-fix-xaml-resources-and-manifest/progress-details.md))
   - ❌ 02.04-iterate-build-and-fix-api-issues: Iterate builds, fix API-breaking issues, and validate runtime smoke-launch ([Content](tasks/02.04-iterate-build-and-fix-api-issues/task.md), [Progress](tasks/02.04-iterate-build-and-fix-api-issues/progress-details.md))
- 🔲 03-update-nuget-packages-and-replace-incompatible-packages: Update packages and document blockers
- 🔲 04-address-API-breaking-changes-and-compile-errors: Fix compile errors from API changes
- 🔲 05-wpf-runtime-and-platform-fixes: Apply runtime fixes and smoke-launch
- 🔲 06-run-tests-and-integration-validation: Run tests and fix failures
- 🔲 07-cleanup-and-documentation: Final cleanup and docs

**Legend**: ✅ Complete | 🔄 In Progress | 🔲 Pending | ⚠️ Blocked | ❌ Failed
