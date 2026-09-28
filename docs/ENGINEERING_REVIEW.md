# Initial source review

2026-09-23. Independent project; no Aether source files were edited.

GitNexus indexed this repository. Initial impact queries for FloatingSculpture and WorkshopBuild returned UNKNOWN/no resolved callers. That was not treated as an unused-code verdict: source references confirmed scene construction through WorkshopBuild, runtime references in ControllerGrab, Unity lifecycle invocation and the helper's executeMethod calls. PowerShell functions were not resolved by the graph; all Invoke-Adb call sites were inspected in tools/quest.ps1.

The initial repository had no HEAD yet, so `detect-changes --scope all` could not diff against HEAD. The full initial addition was staged and `detect-changes --scope staged` analyzed it: 94 files, 62 symbols, 7 flows, overall **high** risk. This covers the whole new application, including both build entry points, scene/material/mesh generation and controller color changes. It is not an assertion that native behavior is already validated. Subsequent commits use the regular all-changes analysis against HEAD.

Build, artifact-manifest and scene-reference checks are recorded in VALIDATION.md. Hardware acceptance is still required. Generated Unity YAML preserves Unity's empty-value trailing spaces; .gitattributes exempts those serialized formats from whitespace-only errors, while authored code and documentation remain checked.

The Windows USB-selection follow-up uses `adb -d get-state` and `adb -d get-serialno` rather than relying on a `usb:` field that is not always printed on Windows. The all-changes graph check sees the changed PowerShell file but reports no indexed symbols for its hunks; that is a coverage gap, not a clean result. The full diff and all callers were inspected manually, the PowerShell parser reports no syntax errors, and a real no-device install attempt stops with the intended actionable error. Positive installation remains pending a connected headset.
