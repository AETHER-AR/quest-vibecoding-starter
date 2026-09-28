# Dependencies and provenance

The scripts, shaders and generated sculpture meshes in this repository were authored for this tutorial. No Aether runtime code, paid art, Pokemon, Yu-Gi-Oh, Cyberpunk content or vendor SDK source archives are included.

Unity downloads external packages from their configured registries. Their licenses are separate from this repository's MIT license. Inspect the licenses installed with each package; this project does not relicense them.

- Meta XR Core SDK 85.0.0 — [Meta SDK license](https://developers.meta.com/horizon/licenses/oculussdk/). The scene references the package's OVRCameraRig prefab; the prefab itself is fetched by Package Manager.
- Unity OpenXR 1.13.2, XR Management 4.4.0, Input System 1.7.0 and transitive dependencies — package license files distributed by Unity.
- Unity Editor — requires a valid Unity license for your use. No editor or Android toolchain binaries are redistributed here.

Implementation references, consulted 2026-09-23:

- [Meta passthrough tutorial](https://developers.meta.com/horizon/documentation/unity/unity-passthrough-tutorial/)
- [Meta passthrough sample overview](https://developers.meta.com/horizon/documentation/unity/unity-sample-starter-passthrough/)
- [Meta device setup](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/)
- Installed SDK API declarations for OVRCameraRig, OVRManager, OVRInput and OVRProjectConfig.

No sample artwork or sample application source was copied from the linked projects.

## Pinned-package manifest correction

Unity OpenXR 1.13.2's `MetaQuest/Editor/ModifyAndroidManifestMeta.cs` unconditionally adds the `oculus.software.eye_tracking` feature and `com.oculus.permission.EYE_TRACKING` permission. This app does not use eye tracking and Quest 3 does not have an eye tracker. `AndroidManifestPolicy.cs` adds explicit manifest-merger removal rules for those two declarations. The installed vendor package remains unchanged. Recheck this policy and the final APK manifest when upgrading the package.
