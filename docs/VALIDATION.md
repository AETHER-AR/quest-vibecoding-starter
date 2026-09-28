# Validation status

Development preview, 2026-09-23. This publication candidate targets **Unity 2022.3.62f3**. It is not yet a verified tutorial release. Do not use historical results from the earlier editor version as evidence that this version has passed.

## Current security-patched candidate

- [x] Restore packages and build Starter and Finished with the pinned 2022.3.62f3 editor and Android modules.
- [x] Verify the manifest and scene wiring for both current builds.
- [x] Install the exact current Finished APK over USB; both build receipts retained locally. Starter hardware acceptance is still pending.
- [ ] Check room visibility and sculpture appearance in both eyes on Quest 3.
- [ ] Check scale, initial placement and right-controller B reset while looking ahead.
- [ ] Check aiming, grip without an initial jump, movement and release leaving the sculpture still.
- [ ] Check A color cycling while targeted/held and no cycling while off target.
- [ ] Check thumbstick reach adjustment while held.
- [ ] Check tracking loss and system-menu recovery without a stuck grab.
- [ ] Relaunch disconnected from the PC; check that no bridge, network or AI account is required by the app.
- [ ] Review performance and recording on the actual headset.
- [ ] Complete the tutorial review and final public-repository identity audit after account setup.

The version guards and ProjectVersion were updated together. Runtime interactions and scenes have not been changed by the version update. See [security patch review](SECURITY_PATCH_REVIEW.md).

## Historical observations: Unity 2022.3.62f1

The previous development checkpoint restored dependencies and built both Android ARM64 APKs in a fresh source checkout. Existing machine-wide package and Gradle caches were available; this was not a clean-OS installation test. Scene wiring checks passed, and two actual GPU-rendered sculpture angles were reviewed for composition.

The exact Finished APK was installed over USB on Quest 3. Native OpenXR and passthrough initialized. AETHER confirmed that the room and sculpture appeared correctly in both eyes and that basic grab/release and reset worked. An initial report that A did not change color was followed by a recording visibly showing cyan, orange/red, violet and cyan. The cause of the earlier failure was not established, and no software fix is claimed. That recording alone does not prove which physical button was pressed or verify off-target input gating.

These observations do not establish comfort, no-jump behavior, thumbstick reach, tracking-loss recovery, system-menu recovery, disconnected relaunch or sustained performance. Short startup frame-rate samples are not a performance acceptance test.

## Evidence and privacy

Keep build receipts, source/APK hashes, build logs and hardware observations locally. Match each hardware test to the exact APK installed. Installation alone does not demonstrate correct behavior. An editor render does not demonstrate headset stereo or passthrough.

Private device identifiers, room recordings, APKs, editor caches and local diagnostic logs are excluded from this source package. Redact identifiers before sharing logs publicly.

The authored example makes no network requests and uses no runtime AI credentials. The development APK can declare INTERNET for included SDK/development facilities; disconnected operation still needs the explicit hardware check above.
