# Your first Quest app with AI

Build a small mixed reality app on a **Meta Quest 3**. Start with a floating sculpture in your room, then ask Codex to add controller interaction. The finished reference lets you grab it, place it and change its color.

**Development preview — this copy targets Unity 2022.3.62f3. Both builds, manifest checks and scene-wiring checks have passed. Headset acceptance for this security-patched version is pending. Earlier Quest observations were made with 2022.3.62f1.** See [validation](docs/VALIDATION.md) before using this as a finished tutorial release.

This is a standalone Unity Android app. Your computer builds it; your Quest runs it. There is no PC streaming, runtime AI assistant, API key or API bill in this example. Codex is the development tool. Its account/access requirements are separate.

## Start with Codex

Open a new, empty project folder in Codex and paste the [starter setup prompt](docs/STARTER_PROMPT.txt). It asks Codex to download this repository, inspect the installed tools, guide the required account/headset confirmations, then build and install the baseline before adding features. The steps below explain that process and also serve as a manual reference.

The prompt has not yet been validated end to end on a clean Windows installation. Check [validation](docs/VALIDATION.md) for the current tested scope.

## 1. What you need

- Quest 3 and its Touch controllers, charged.
- A Windows computer with enough storage for Unity, Android tools and the project. This repository's build helper is for Windows.
- A USB cable that transfers data. A charging-only cable will not work.
- Internet for editor installation and package downloads.
- Unity Hub, a Unity account and a valid editor license for your use.
- **Unity 2022.3.62f3**, including **Android Build Support**, **Android SDK & NDK Tools** and **OpenJDK**. Install that exact version through Unity Hub / Unity's archive. This security patch replaces the earlier 2022.3.62f1 development setup; its build and headset validation are tracked in `docs/VALIDATION.md`.
- Android SDK platform 34 in that editor's SDK. The preflight reports if it is missing.
- A Meta developer account and developer mode enabled on your headset.
- Codex installed and signed in if following the AI-assisted steps.

For the current desktop setup, follow the [official quickstart](https://learn.chatgpt.com/docs/quickstart): install the ChatGPT desktop app, sign in, select Codex for development, and open this local project folder. Check access and model availability in your account; no subscription tier is guaranteed to cover every project's usage.

The Unity version is deliberately pinned to the build environment used for this episode, not advertised as the newest release. Package versions are in `Packages/manifest.json` and `Packages/packages-lock.json`.

If preflight reports that Android SDK platform 34 is missing, ask Codex to locate the SDK Manager inside this Unity editor's Android tools and install `platforms;android-34` there. Read and accept any SDK license prompt yourself. Installing it into an unrelated Android Studio SDK will not satisfy this project's check.

## 2. Enable development on the headset

Follow [Meta's device setup guide](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/) for the current developer-account and developer-mode steps. These account confirmations need your input; an AI cannot complete them for you.

Connect the Quest to your PC by USB. Put it on, unlock it, and accept **Allow USB debugging** for your own computer. If the connection is missing, check the cable, developer mode and the in-headset prompt. Do not confuse the USB file-transfer prompt with USB debugging.

## 3. Open this project

Download the repository ZIP and extract it to a normal folder such as `Documents/quest-vibecoding-starter`, or clone it with Git. Do not open files inside the ZIP. A repository is simply the project's files plus their saved history.

Open the extracted folder containing `Assets`, `Packages` and `ProjectSettings` in Codex. The AI-led tutorial does not require manually arranging objects or editing scripts inside Unity. Codex uses the installed **2022.3.62f3** editor and the project helper to prepare and build the app. The first import can take several minutes; let it finish before requesting the next change. You handle sign-ins, license agreements and permission prompts.

Ask Codex:

> Read README.md and AGENTS.md. Inspect this computer and this project. Check the pinned Unity editor, Android modules and USB connection to my Quest. Give me a short to-do list, then run the preflight. Tell me exactly which account or headset confirmations I need to do myself. Don't change versions or build features yet.

The generated scenes are included in the repository. If working from a source-only scaffold, use **Workshop → Prepare project (first setup)** once. That command creates missing scenes; it does not replace existing scenes.

## 4. Build the starting point

The starter scene is `Assets/Scenes/01_Passthrough.unity`. It contains the prepared camera rig, passthrough configuration and original sculpture. Those are already provided; this tutorial does not pretend they appeared from an empty folder after one prompt.

Ask Codex to use that scene; there is no need to open and edit it manually:

> Build and install the starter scene on my Quest over USB. Use the repository's build and install commands. Do not install an old APK if the build fails. Tell me what I should see before I put on the headset.

For manual use, close this Unity project, open PowerShell in the repository folder, then run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/quest.ps1 -Action Preflight
powershell -ExecutionPolicy Bypass -File tools/quest.ps1 -Action BuildStarter
powershell -ExecutionPolicy Bypass -File tools/quest.ps1 -Action InstallStarter
```

The execution-policy override applies to that process, not a permanent machine-wide policy change. Read downloaded scripts before running them.

In the headset you should see your room with a roughly 30 cm sculpture about 85 cm ahead and slightly below eye level. Look ahead and press **B** on the right controller to bring it back. At this starting point you cannot grab it yet. Placement is session-local, not saved to your room after restarting.

## 5. Ask for one change

> Add right-controller interaction to the starter scene. Let me point at the sculpture, hold the grip button to move it, and release to leave it in place. It should not jump when I grab it. A should cycle three colors when I point at or hold it. Keep B as reset. Keep passthrough and the existing camera rig. Explain what you changed, build it, and install it so I can test.

The finished answer is already available in `02_GrabAndChange.unity` and `ControllerGrab.cs` for comparison. You can ask Codex to explain/reuse that implementation or attempt your own. The reference is a learning aid, not evidence that the AI independently generated it during your session.

## 6. Test, describe, revise

Check the actual behavior in the headset. Say what you see, what you expected and what should stay unchanged. For example, only if it happens:

> When I release the grip, the sculpture keeps following my controller. It should stay where I released it. Keep the color change and reset behavior. Find the cause, rebuild and reinstall, then give me one clear test for the fix.

Don't paste that as if the bug is guaranteed. Use your real observation. Installation proves that an APK reached the headset; it does not prove that interactions work.

## Finished reference controls

Build/install `BuildFinished` / `InstallFinished` with the same helper.

| Right controller action | Result |
| --- | --- |
| Point at the sculpture | Aim point grows to show the target |
| Hold grip while pointing | Grab; move and rotate the controller |
| Release grip | Leave the sculpture in that pose |
| Thumbstick up/down while holding | Bring it closer / move it farther |
| A while pointing or holding | Cycle cyan, amber and violet |
| B while looking ahead | Reset in front of you |

Hands-only input, room collisions, persistent anchors, portals and AI voice are outside this first episode. A released object floats; it does not snap to a physical table. Stay within a clear play area.

## Learn / troubleshoot

- [Chapter prompts](docs/PROMPTS.md)
- [Build and headset acceptance](docs/VALIDATION.md)
- [Third-party dependencies and sources](THIRD_PARTY.md)
- Creator: [YouTube](https://www.youtube.com/@AE.T.HE.R) · [Instagram](https://www.instagram.com/ae.t.he.r/)

If the room is black, input is missing, or the build fails, stop and share the exact symptom and `artifacts/BuildStarter.log` or `BuildFinished.log` with Codex. Do not replace passthrough with a fake background or treat editor playback as a headset test.
