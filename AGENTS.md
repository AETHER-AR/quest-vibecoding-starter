# Quest tutorial contract

- Read README.md and docs/VALIDATION.md first. This is an independent standalone Quest 3 Unity starter, not the full Aether application.
- All user-facing content is English. Preserve the tutorial's pinned versions unless the user explicitly requests a migration.
- Inspect installed tools and find paths yourself. Ask the user only for account actions, headset confirmations, observations or an unavailable connection.
- Use tools/quest.ps1: Preflight, BuildStarter / BuildFinished, then the matching Install action. Never claim success from a stale APK. Unity must be closed for batch builds; the Workshop menu supports builds from the open editor.
- The prepared baseline includes the XR/passthrough setup and sculpture. Do not imply that the learner built those from nothing. The finished reference is explicitly available.
- Analyze callers and scene references before changing existing code. Use GitNexus if indexed; confirm Unity serialized/prefab/event links in the actual assets because the call graph alone cannot prove those links absent.
- Keep fixes scoped. Preserve user scene edits; Prepare creates only missing scenes. Never regenerate an existing scene to hide a regression.
- No silent fallbacks: no fake passthrough, desktop simulation presented as native validation, automatic USB/Wi-Fi switch, dummy successes or old-APK installs.
- Quest package ID is com.aether.questworkshop. Never modify or uninstall another app, including Aether.
- No account tokens, API keys, signing keys, downloaded vendor SDK sources or third-party entertainment assets in Git. Generated meshes and shaders in Assets are original to this example.
- Before completing runtime work: build, install that exact build, and perform/ask for the headset acceptance checklist. Record what is pending separately from what passed.
- An editor render checks composition only. Hardware acceptance needs actual passthrough, both eyes, tracking, input and comfort checks.
- Preserve a working checkpoint before adding the next feature. Never publish an untested build as a verified tutorial release.
