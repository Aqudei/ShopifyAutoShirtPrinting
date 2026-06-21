# 01.03-locate-or-create-unit-tests-and-convert: Locate UnitTestProject1 or create/convert test project and target net10.0

## Objective
Ensure unit tests exist and target modern frameworks. If UnitTestProject1 is missing, locate its source or create a new test project that covers Common. Convert the test project to SDK-style and target net10.0.

## Steps
1. Search repository for test project files; if not present, ask user or create a new test project in UnitTestProject1 targeting net10.0 using MSTest/NUnit as existing tests use.
2. Convert csproj to SDK-style and update test framework packages to net10-compatible versions if necessary.
3. Run tests and confirm they pass against Common (net10 target) where applicable.

## Done when
- Tests are present, converted to SDK-style, target net10.0, and run successfully for the modified code.
