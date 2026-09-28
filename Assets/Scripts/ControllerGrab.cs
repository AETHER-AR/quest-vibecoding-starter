using UnityEngine;

namespace QuestWorkshop
{
    /// <summary>Right Touch grip holds the ray-selected sculpture; A changes its color.</summary>
    public sealed class ControllerGrab : MonoBehaviour
    {
        public OVRCameraRig cameraRig;
        public FloatingSculpture sculpture;
        public LineRenderer pointer;
        public Transform cursor;
        private float holdDistance;
        private Vector3 positionOffset;
        private Quaternion rotationOffset;
        private bool holding;
        private const OVRInput.Controller Hand = OVRInput.Controller.RTouch;

        private void LateUpdate()
        {
            bool tracked = OVRInput.IsControllerConnected(Hand) && OVRInput.GetControllerPositionTracked(Hand)
                && OVRManager.hasInputFocus && sculpture.IsPlaced;
            pointer.enabled = tracked;
            cursor.gameObject.SetActive(tracked);
            if (!tracked) { Release(); return; }

            var hand = cameraRig.rightHandAnchor;
            var ray = new Ray(hand.position, hand.forward);
            bool pointed = Physics.Raycast(ray, out var hit, 2f) && hit.collider.GetComponent<FloatingSculpture>() == sculpture;
            bool grip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Hand);
            if (OVRInput.GetDown(OVRInput.Button.Two, Hand)) Release();
            if (holding && !sculpture.IsHeld) Release();
            if (!holding && pointed && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, Hand))
            {
                holding = true;
                sculpture.IsHeld = true;
                holdDistance = Vector3.Distance(hand.position, hit.point);
                positionOffset = Quaternion.Inverse(hand.rotation) * (sculpture.transform.position - hit.point);
                rotationOffset = Quaternion.Inverse(hand.rotation) * sculpture.transform.rotation;
                Debug.Log("QUEST_WORKSHOP: grab started");
            }
            if (holding && !grip) Release();
            if (holding)
            {
                float axis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Hand).y;
                if (Mathf.Abs(axis) > 0.2f) holdDistance = Mathf.Clamp(holdDistance + axis * 0.4f * Time.deltaTime, 0.25f, 1.5f);
                sculpture.transform.SetPositionAndRotation(ray.GetPoint(holdDistance) + hand.rotation * positionOffset, hand.rotation * rotationOffset);
            }
            if ((pointed || holding) && OVRInput.GetDown(OVRInput.Button.One, Hand)) sculpture.NextColor();
            Vector3 end = holding ? ray.GetPoint(holdDistance) : pointed ? hit.point : ray.GetPoint(1.5f);
            pointer.SetPosition(0, ray.origin);
            pointer.SetPosition(1, end);
            pointer.startWidth = holding ? 0.003f : 0.0015f;
            pointer.endWidth = pointer.startWidth * 0.65f;
            cursor.position = end;
            cursor.localScale = Vector3.one * (pointed || holding ? 0.009f : 0.004f);
        }

        private void Release()
        {
            if (holding) Debug.Log("QUEST_WORKSHOP: grab released");
            holding = false;
            sculpture.IsHeld = false;
        }

        private void OnApplicationFocus(bool focused) { if (!focused) Release(); }
        private void OnDisable()
        {
            Release();
            if (pointer) pointer.enabled = false;
            if (cursor) cursor.gameObject.SetActive(false);
        }
    }
}
