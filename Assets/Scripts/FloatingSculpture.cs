using System.Collections;
using UnityEngine;

namespace QuestWorkshop
{
    /// <summary>Places a 30 cm sculpture in front of the viewer once tracking is ready.</summary>
    public sealed class FloatingSculpture : MonoBehaviour
    {
        public OVRCameraRig cameraRig;
        public Transform animatedCore;
        public Transform[] rings;
        public Renderer[] tintedSurfaces;
        [Range(0, 90)] public float rotationSpeed = 18f;
        public Color[] palette = { new Color(0.12f, 0.85f, 1f), new Color(1f, 0.32f, 0.12f), new Color(0.68f, 0.35f, 1f) };
        public bool IsHeld { get; set; }
        public bool IsPlaced { get; private set; }
        public int ColorIndex { get; private set; }
        private MaterialPropertyBlock properties;

        private IEnumerator Start()
        {
            properties = new MaterialPropertyBlock();
            ApplyColor();
            // The editor preview is composed separately; it is not a simulated Quest test.
            while (!OVRManager.isHmdPresent || !OVRManager.tracker.isPositionTracked)
                yield return null;
            yield return null;
            PlaceInFront();
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
                PlaceInFront();
            if (IsHeld) return;
            animatedCore.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            for (int i = 0; i < rings.Length; i++)
                rings[i].Rotate(Vector3.forward, rotationSpeed * (i % 2 == 0 ? 0.6f : -0.4f) * Time.deltaTime, Space.Self);
        }

        public void PlaceInFront()
        {
            if (!OVRManager.isHmdPresent || !OVRManager.tracker.isPositionTracked) return;
            var eye = cameraRig.centerEyeAnchor;
            var forward = Vector3.ProjectOnPlane(eye.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.05f) return; // Look ahead before resetting.
            transform.SetPositionAndRotation(eye.position + forward.normalized * 0.85f - Vector3.up * 0.2f, Quaternion.identity);
            IsHeld = false;
            IsPlaced = true;
            Debug.Log("QUEST_WORKSHOP: sculpture placed");
        }

        public void NextColor()
        {
            ColorIndex = (ColorIndex + 1) % palette.Length;
            ApplyColor();
            Debug.Log("QUEST_WORKSHOP: color changed " + ColorIndex);
        }

        private void ApplyColor()
        {
            properties ??= new MaterialPropertyBlock();
            properties.SetColor("_Color", palette[ColorIndex]);
            foreach (var surface in tintedSurfaces) surface.SetPropertyBlock(properties);
        }
    }
}
