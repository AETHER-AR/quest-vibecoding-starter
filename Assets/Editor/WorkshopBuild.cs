using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace QuestWorkshop.Editor
{
    public static class WorkshopBuild
    {
        public const string StarterScene = "Assets/Scenes/01_Passthrough.unity";
        public const string FinishedScene = "Assets/Scenes/02_GrabAndChange.unity";
        public const string PackageId = "com.aether.questworkshop";

        [MenuItem("Workshop/Prepare project (first setup)")]
        public static void Prepare()
        {
            Configure();
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Materials");
            Directory.CreateDirectory("Assets/Meshes");
            AssetDatabase.Refresh();
            if (!File.Exists(StarterScene)) CreateScene(StarterScene, false);
            if (!File.Exists(FinishedScene)) CreateScene(FinishedScene, true);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(StarterScene, true) };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(StarterScene);
            Debug.Log("QUEST_WORKSHOP: prepared. Open the finished scene for the comparison.");
        }

        [MenuItem("Workshop/Build starter APK")]
        public static void BuildStarter() => Build(StarterScene, "Starter");

        [MenuItem("Workshop/Build finished APK")]
        public static void BuildFinished() => Build(FinishedScene, "Finished");

        private static void Build(string scene, string label)
        {
            Configure();
            if (!File.Exists(scene)) throw new FileNotFoundException("Run Workshop/Prepare project first.", scene);
            Directory.CreateDirectory("Builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scene }, target = BuildTarget.Android,
                locationPathName = "Builds/QuestWorkshop-" + label + ".apk",
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed: " + report.summary.result);
            Debug.Log("QUEST_WORKSHOP: " + label + " APK built successfully");
        }

        private static void Configure()
        {
            if (Application.unityVersion != "2022.3.62f3")
                throw new InvalidOperationException("Use Unity 2022.3.62f3 for this tutorial release.");
            PlayerSettings.companyName = "AE.T.HE.R";
            PlayerSettings.productName = "Quest Workshop";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.antiAliasing = 4;
            QualitySettings.vSyncCount = 0;
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = player.FindProperty("activeInputHandler");
            if (input == null) throw new InvalidOperationException("Input system setting was not found.");
            input.intValue = 1;
            player.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory("Assets/XR");
            AssetDatabase.Refresh();
            if (!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var all))
            {
                all = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(all, "Assets/XR/Settings.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, all, true);
            }
            if (!all.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
                all.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            var settings = all.SettingsForBuildTarget(BuildTargetGroup.Android);
            settings.InitManagerOnStart = true;
            if (!XRPackageMetadataStore.AssignLoader(settings.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android))
                throw new InvalidOperationException("Could not assign the Android OpenXR loader.");
            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
            foreach (var id in new[] { Meta.XR.MetaXRFeature.featureId, "com.unity.openxr.feature.metaquest", "com.unity.openxr.feature.input.oculustouch" })
            {
                var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, id);
                if (!feature) throw new InvalidOperationException("Required OpenXR feature missing: " + id);
                feature.enabled = true;
                EditorUtility.SetDirty(feature);
            }
            var xr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            xr.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            EditorUtility.SetDirty(xr);
            EditorUtility.SetDirty(settings.Manager);
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(all);
            var config = OVRProjectConfig.CachedProjectConfig;
            config.targetDeviceTypes = new System.Collections.Generic.List<OVRProjectConfig.DeviceType> { OVRProjectConfig.DeviceType.Quest3 };
            config.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Required;
            config.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersOnly;
            config.sceneSupport = OVRProjectConfig.FeatureSupport.None;
            config.anchorSupport = OVRProjectConfig.AnchorSupport.Disabled;
            config.isPassthroughCameraAccessEnabled = false;
            config.systemLoadingScreenBackground = OVRProjectConfig.SystemLoadingScreenBackground.ContextualPassthrough;
            OVRProjectConfig.CommitProjectConfig(config);
            AssetDatabase.SaveAssets();
        }

        private static void CreateScene(string path, bool interactive)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab");
            if (!prefab) throw new FileNotFoundException("Meta XR camera rig package is not resolved.");
            var rigObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var rig = rigObject.GetComponent<OVRCameraRig>();
            var manager = rigObject.GetComponent<OVRManager>();
            manager.isInsightPassthroughEnabled = true;
            manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            // Basic passthrough does not require raw camera, microphone, or room-mesh access.
            var passthrough = rigObject.AddComponent<OVRPassthroughLayer>();
            passthrough.overlayType = OVROverlay.OverlayType.Underlay;
            passthrough.projectionSurfaceType = OVRPassthroughLayer.ProjectionSurfaceType.Reconstructed;
            passthrough.textureOpacity = 1;
            foreach (var camera in rigObject.GetComponentsInChildren<Camera>(true))
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 30f;
                camera.allowHDR = false;
                camera.allowMSAA = true;
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(manager);
            foreach (var camera in rigObject.GetComponentsInChildren<Camera>(true)) PrefabUtility.RecordPrefabInstancePropertyModifications(camera);

            Material solid = MaterialAsset("Solid", "QuestWorkshop/Solid", new Color(0.12f, 0.85f, 1));
            Material holo = MaterialAsset("Glass", "QuestWorkshop/Hologram", new Color(0.12f, 0.85f, 1));
            Material dark = MaterialAsset("Graphite", "QuestWorkshop/Solid", new Color(0.025f, 0.045f, 0.07f));
            Material pointerMaterial = MaterialAsset("Pointer", "QuestWorkshop/Solid", new Color(0.65f, 0.9f, 1));
            var root = new GameObject("Floating sculpture");
            root.transform.position = new Vector3(0, 1.35f, 0.85f);
            var sculpture = root.AddComponent<FloatingSculpture>();
            sculpture.cameraRig = rig;
            root.AddComponent<SphereCollider>().radius = 0.18f;
            var core = MeshObject("Crystal", Octahedron(), holo, root.transform);
            core.transform.localScale = new Vector3(0.092f, 0.15f, 0.092f);
            sculpture.animatedCore = core.transform;
            var inner = MeshObject("Inner crystal", Octahedron(), solid, core.transform);
            inner.transform.localScale = Vector3.one * 0.42f;
            var ringTransforms = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var ring = MeshObject("Orbit " + (i + 1), Torus(), i == 1 ? dark : solid, root.transform);
                ring.transform.localScale = Vector3.one * (0.115f + i * 0.02f);
                ring.transform.localRotation = Quaternion.Euler(28 + 45 * i, i * 60, 20);
                ringTransforms[i] = ring.transform;
            }
            sculpture.rings = ringTransforms;
            sculpture.tintedSurfaces = root.GetComponentsInChildren<Renderer>().Where(r => r.sharedMaterial != dark).ToArray();
            if (interactive)
            {
                var interaction = new GameObject("Right controller interaction").AddComponent<ControllerGrab>();
                interaction.cameraRig = rig;
                interaction.sculpture = sculpture;
                var line = new GameObject("Aim line").AddComponent<LineRenderer>();
                line.sharedMaterial = pointerMaterial;
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.numCapVertices = 4;
                line.enabled = false;
                interaction.pointer = line;
                var cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cursor.name = "Aim point";
                UnityEngine.Object.DestroyImmediate(cursor.GetComponent<Collider>());
                cursor.GetComponent<Renderer>().sharedMaterial = pointerMaterial;
                cursor.transform.localScale = Vector3.one * 0.007f;
                cursor.SetActive(false);
                interaction.cursor = cursor.transform;
            }
            EditorSceneManager.SaveScene(scene, path);
        }

        private static Material MaterialAsset(string name, string shaderName, Color color)
        {
            string path = "Assets/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material) return material;
            var shader = Shader.Find(shaderName);
            if (!shader) throw new InvalidOperationException("Required shader missing: " + shaderName);
            material = new Material(shader) { color = color, enableInstancing = true };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject MeshObject(string name, Mesh mesh, Material material, Transform parent)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private static Mesh Octahedron()
        {
            const string path = "Assets/Meshes/Crystal.asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved) return saved;
            Vector3[] points = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            int[] faces = { 0,4,3, 0,2,4, 0,5,2, 0,3,5, 1,3,4, 1,4,2, 1,2,5, 1,5,3 };
            var mesh = new Mesh { name = "Faceted crystal", vertices = faces.Select(i => points[i]).ToArray(), triangles = Enumerable.Range(0,24).ToArray() };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Mesh Torus()
        {
            const string path = "Assets/Meshes/Orbit.asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved) return saved;
            const int segments = 96, sides = 8;
            var vertices = new Vector3[segments * sides];
            var triangles = new int[segments * sides * 6];
            for (int a = 0; a < segments; a++) for (int b = 0; b < sides; b++)
            {
                float u = a * Mathf.PI * 2 / segments, v = b * Mathf.PI * 2 / sides;
                vertices[a*sides+b] = new Vector3((1 + 0.018f*Mathf.Cos(v))*Mathf.Cos(u), (1 + 0.018f*Mathf.Cos(v))*Mathf.Sin(u), 0.018f*Mathf.Sin(v));
                int index = (a*sides+b)*6, n = (a+1)%segments, s = (b+1)%sides;
                int[] quad = { a*sides+b, n*sides+b, n*sides+s, a*sides+b, n*sides+s, a*sides+s };
                Array.Copy(quad, 0, triangles, index, 6);
            }
            var mesh = new Mesh { name = "Continuous orbit", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
