## Files Modified
- Common\Common.csproj — normalized SDK-style and preserved net472 target (now multi-targets net472;net10.0)

## Build Result
- Built: Common (net472)
- Errors: 0
- Warnings: 1 (NU1701: PdfiumViewer restored for older frameworks; may be incompatible with net10.0)

## Test Result
- No tests run in this subtask

## Changes Summary
- Ensured Common.csproj is SDK-style and builds for net472. Left notes to add conditional package references and proceed with net10 multi-targeting in the next subtask.

## Issues Encountered
- PdfiumViewer package is not net10-compatible and will need replacement or isolation when targeting net10.0.

## Next Steps
- Start subtask 01.02 to add net10 target and perform conditional package updates.
