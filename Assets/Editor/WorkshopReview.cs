using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuestWorkshop.Editor
{
    public static class WorkshopReview
    {
        [MenuItem("Workshop/Validate scene wiring")]
        public static void ValidateScenes()
        {
            foreach (string path in new[] { WorkshopBuild.StarterScene, WorkshopBuild.FinishedScene })
            {
                EditorSceneManager.OpenScene(path);
                var rigs = UnityEngine.Object.FindObjectsOfType<OVRCameraRig>();
                Require(rigs.Length == 1, path + ": exactly one camera rig required");
                var manager = rigs[0].GetComponent<OVRManager>();
                var layer = rigs[0].GetComponent<OVRPassthroughLayer>();
                Require(manager && manager.isInsightPassthroughEnabled, path + ": passthrough disabled");
                Require(layer && layer.overlayType == OVROverlay.OverlayType.Underlay && layer.textureOpacity == 1, path + ": underlay misconfigured");
                foreach (var camera in rigs[0].GetComponentsInChildren<Camera>(true))
                    Require(camera.clearFlags == CameraClearFlags.SolidColor && camera.backgroundColor.a == 0, path + ": camera would obscure the room");
                var sculpture = UnityEngine.Object.FindObjectOfType<FloatingSculpture>();
                Require(sculpture && sculpture.cameraRig == rigs[0] && sculpture.animatedCore && sculpture.rings.Length == 3, path + ": missing sculpture references");
                Require(sculpture.tintedSurfaces.Length > 0 && sculpture.tintedSurfaces.All(r => r && r.sharedMaterial && r.sharedMaterial.shader.isSupported), path + ": unsupported or missing material");
                Require(sculpture.GetComponent<SphereCollider>().radius >= 0.15f, path + ": selection volume does not enclose sculpture");
                foreach (var filter in sculpture.GetComponentsInChildren<MeshFilter>())
                    Require(filter.sharedMesh && filter.sharedMesh.vertexCount > 0 && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(filter.sharedMesh)), path + ": unsaved mesh would be lost after import");
                var grab = UnityEngine.Object.FindObjectOfType<ControllerGrab>();
                if (path == WorkshopBuild.FinishedScene)
                    Require(grab && grab.cameraRig == rigs[0] && grab.sculpture == sculpture && grab.pointer && grab.cursor, path + ": interaction references missing");
                else Require(!grab, path + ": starting checkpoint unexpectedly contains finished interaction");
                Debug.Log("QUEST_WORKSHOP: scene wiring PASS " + path);
            }
            EditorSceneManager.OpenScene(WorkshopBuild.StarterScene);
        }

        [MenuItem("Workshop/Render sculpture review")]
        public static void RenderReview()
        {
            EditorSceneManager.OpenScene(WorkshopBuild.FinishedScene);
            var rig = UnityEngine.Object.FindObjectOfType<OVRCameraRig>();
            rig.gameObject.SetActive(false);
            var sculpture = UnityEngine.Object.FindObjectOfType<FloatingSculpture>();
            var go = new GameObject("Review camera");
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.018f, 0.027f, 0.038f, 1);
            camera.fieldOfView = 34;
            camera.nearClipPlane = 0.02f;
            var target = new RenderTexture(1200, 1200, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            target.Create();
            camera.targetTexture = target;
            Directory.CreateDirectory("artifacts/review");
            for (int angle = 0; angle < 3; angle++)
            {
                camera.transform.position = sculpture.transform.position + Quaternion.Euler(10, angle * 40, 0) * new Vector3(0, 0, -0.8f);
                camera.transform.LookAt(sculpture.transform);
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(1200, 1200, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1200, 1200), 0, 0);
                image.Apply();
                File.WriteAllBytes("artifacts/review/sculpture-" + angle + ".png", image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            RenderTexture.active = null;
            camera.targetTexture = null;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(go);
            // Reload without saving the temporary review camera or disabled headset rig.
            EditorSceneManager.OpenScene(WorkshopBuild.StarterScene);
            Debug.Log("QUEST_WORKSHOP: GPU composition review rendered; not a headset acceptance test");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
