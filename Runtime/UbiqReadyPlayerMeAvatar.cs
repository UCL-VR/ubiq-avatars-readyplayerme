using UnityEngine;
using Ubiq.Avatars;
using ReadyPlayerMe.AvatarLoader;
using System.Collections.Generic;
using Ubiq.Voip;

namespace UbiqReadyPlayerMe
{
    public class UbiqReadyPlayerMeAvatar : MonoBehaviour
    {
        public bool useEyeAnimations;
        public bool useVoiceAnimations;

        public GameObject speechIndicatorPrefab;

        private Vector3 headPositionOffset = new Vector3(0.0f,-0.063000001f,0.0f);
        private Quaternion headRotationOffset = new Quaternion(0.0f,0.0f,0.0f,1.0f);
        private Vector3 leftHandPositionOffset = new Vector3(-0.0149999997f,-0.0299999993f,-0.0700000003f);
        private Quaternion leftHandRotationOffset = new Quaternion(0.488744467f,0.575827956f,0.42057842f,0.502657831f);
        private Vector3 rightHandPositionOffset = new Vector3(0.0149999997f,-0.0299999993f,-0.0700000003f);
        private Quaternion rightHandRotationOffset = new Quaternion(-0.488744438f,0.575827897f,0.42057842f,-0.502657771f);

        [SerializeField] [HideInInspector] private List<Quaternion> leftGripRotations = new List<Quaternion>();
        [SerializeField] [HideInInspector] private List<Quaternion> rightGripRotations = new List<Quaternion>();
        [SerializeField] [HideInInspector] private List<Quaternion> leftReleaseRotations = new List<Quaternion>();
        [SerializeField] [HideInInspector] private List<Quaternion> rightReleaseRotations = new List<Quaternion>();

        private Vector3 speechIndicatorPositionOffset = new Vector3(0.0f,0.0f,0.0f);

        private UbiqReadyPlayerMeLoader loader;
        private ThreePointTrackedAvatar trackedAvatar;

        private Transform head;
        private Transform leftHand;
        private Transform rightHand;
        private Transform armature;

        private List<Transform> leftHandBones = new List<Transform>();
        private List<Transform> rightHandBones = new List<Transform>();

        private float lastLeftGrip = -1;
        private float lastRightGrip = -1;

        private void Start()
        {
            trackedAvatar = GetComponentInParent<ThreePointTrackedAvatar>();
            Debug.Assert(trackedAvatar,"Requires ThreePointTrackedAvatar");
            trackedAvatar.OnHeadUpdate.AddListener(ThreePointTrackedAvatar_OnHeadUpdate);
            trackedAvatar.OnLeftHandUpdate.AddListener(ThreePointTrackedAvatar_OnLeftHandUpdate);
            trackedAvatar.OnRightHandUpdate.AddListener(ThreePointTrackedAvatar_OnRightHandUpdate);
            trackedAvatar.OnLeftGripUpdate.AddListener(ThreePointTrackedAvatar_OnLeftGripUpdate);
            trackedAvatar.OnRightGripUpdate.AddListener(ThreePointTrackedAvatar_OnRightGripUpdate);

            loader = GetComponent<UbiqReadyPlayerMeLoader>();
            Debug.Assert(loader,"Requires UbiqReadyPlayerMeLoader");

            loader.completed.AddListener(Loader_Completed);
            loader.failed.AddListener(Loader_Failed);

            if (loader.isLoaded)
            {
                Loader_Completed(loader.loadedArgs);
            }
        }

        private void OnDestroy()
        {
            if (trackedAvatar)
            {
                trackedAvatar.OnHeadUpdate.RemoveListener(ThreePointTrackedAvatar_OnHeadUpdate);
                trackedAvatar.OnLeftHandUpdate.RemoveListener(ThreePointTrackedAvatar_OnLeftHandUpdate);
                trackedAvatar.OnRightHandUpdate.RemoveListener(ThreePointTrackedAvatar_OnRightHandUpdate);
                trackedAvatar.OnLeftGripUpdate.RemoveListener(ThreePointTrackedAvatar_OnLeftGripUpdate);
                trackedAvatar.OnRightGripUpdate.RemoveListener(ThreePointTrackedAvatar_OnRightGripUpdate);
            }
            trackedAvatar = null;

            if (loader)
            {
                loader.completed.RemoveListener(Loader_Completed);
                loader.failed.RemoveListener(Loader_Failed);
            }
            loader = null;
        }

        private static Matrix4x4 TR(Vector3 pos, Quaternion rot)
        {
            return Matrix4x4.Translate(pos) * Matrix4x4.Rotate(rot);
        }

        private void ThreePointTrackedAvatar_OnHeadUpdate(Vector3 pos, Quaternion rot)
        {
            if (head && armature)
            {
                var headLocalPos = armature.InverseTransformPoint(head.position);
                var headLocalRot = Quaternion.Inverse(armature.rotation) * head.rotation;

                var localHeadTRS = TR(headLocalPos,headLocalRot);
                var armatureTRS = Matrix4x4.Translate(pos) * Matrix4x4.Rotate(rot) * localHeadTRS.inverse;
                armature.position = armatureTRS.GetPosition();
                armature.rotation = armatureTRS.rotation;

                armature.Translate(headPositionOffset,Space.Self);
                armature.localRotation *= headRotationOffset;

                var indicator = GetComponentInChildren<VoipSpeechIndicator>();
                indicator.transform.localPosition = speechIndicatorPositionOffset;
            }

        }

        private void ThreePointTrackedAvatar_OnLeftHandUpdate(Vector3 pos, Quaternion rot)
        {
            if (leftHand)
            {
                leftHand.position = pos;
                leftHand.rotation = rot;

                leftHand.Translate(leftHandPositionOffset,Space.Self);
                leftHand.localRotation *= leftHandRotationOffset;
            }
        }

        private void ThreePointTrackedAvatar_OnRightHandUpdate(Vector3 pos, Quaternion rot)
        {
            if (rightHand)
            {
                rightHand.position = pos;
                rightHand.rotation = rot;

                rightHand.Translate(rightHandPositionOffset,Space.Self);
                rightHand.localRotation *= rightHandRotationOffset;
            }
        }

        private void ThreePointTrackedAvatar_OnLeftGripUpdate(float grip)
        {
            // Avoid unecessary slerps
            if (lastLeftGrip == grip)
            {
                return;
            }

            for(int i = 0; i < leftHandBones.Count; i++)
            {
                var rot = Quaternion.Slerp(leftReleaseRotations[i],leftGripRotations[i],grip);
                leftHandBones[i].localRotation = rot;
            }
            lastLeftGrip = grip;
        }

        private void ThreePointTrackedAvatar_OnRightGripUpdate(float grip)
        {
            // Avoid unecessary slerps
            if (lastRightGrip == grip)
            {
                return;
            }

            for(int i = 0; i < rightHandBones.Count; i++)
            {
                var rot = Quaternion.Slerp(rightReleaseRotations[i],rightGripRotations[i],grip);
                rightHandBones[i].localRotation = rot;
            }
            lastRightGrip = grip;
        }

        private void Loader_Completed(CompletionEventArgs args)
        {
            if (useEyeAnimations) args.Avatar.AddComponent<EyeAnimationHandler>();
            if (useVoiceAnimations) args.Avatar.AddComponent<UbiqReadyPlayerMeVoiceHandler>();

            var t = args.Avatar.transform;
            if (args.Metadata.BodyType == BodyType.FullBody)
            {
                head = t.Find("Armature/Hips/Spine/Spine1/Spine2/Neck/Head");
                leftHand = t.Find("Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand");
                rightHand = t.Find("Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand");
            }
            else
            {
                armature = t.Find("Armature");
                head = t.Find("Armature/Hips/Spine/Neck/Head");
                leftHand = t.Find("Armature/Hips/Spine/LeftHand");
                rightHand = t.Find("Armature/Hips/Spine/RightHand");

                if (head && TryGetComponent<VoipAvatar>(out var voipAvatar))
                {
                    var go = Instantiate(speechIndicatorPrefab);
                    go.transform.parent = head;
                    go.transform.localPosition = speechIndicatorPositionOffset;
                    voipAvatar.audioSourcePosition = head;
                    voipAvatar.speechIndicator = go.GetComponentInChildren<VoipSpeechIndicator>();
                }

                // Animate manually. Animation component is added by the GLTF
                // loader and does not seem to be functional
                Destroy(GetComponentInChildren<Animation>());

                if (leftHand)
                {
                    leftHandBones.Clear();
                    leftHandBones.Add(leftHand.Find("LeftHandIndex1"));
                    leftHandBones.Add(leftHand.Find("LeftHandIndex1/LeftHandIndex2"));
                    leftHandBones.Add(leftHand.Find("LeftHandIndex1/LeftHandIndex2/LeftHandIndex3"));
                    leftHandBones.Add(leftHand.Find("LeftHandMiddle1"));
                    leftHandBones.Add(leftHand.Find("LeftHandMiddle1/LeftHandMiddle2"));
                    leftHandBones.Add(leftHand.Find("LeftHandMiddle1/LeftHandMiddle2/LeftHandMiddle3"));
                    leftHandBones.Add(leftHand.Find("LeftHandRing1"));
                    leftHandBones.Add(leftHand.Find("LeftHandRing1/LeftHandRing2"));
                    leftHandBones.Add(leftHand.Find("LeftHandRing1/LeftHandRing2/LeftHandRing3"));
                    leftHandBones.Add(leftHand.Find("LeftHandPinky1"));
                    leftHandBones.Add(leftHand.Find("LeftHandPinky1/LeftHandPinky2"));
                    leftHandBones.Add(leftHand.Find("LeftHandPinky1/LeftHandPinky2/LeftHandPinky3"));
                    leftHandBones.Add(leftHand.Find("LeftHandThumb1"));
                    leftHandBones.Add(leftHand.Find("LeftHandThumb1/LeftHandThumb2"));
                    leftHandBones.Add(leftHand.Find("LeftHandThumb1/LeftHandThumb2/LeftHandThumb3"));
                }
                if (rightHand)
                {
                    rightHandBones.Clear();
                    rightHandBones.Add(rightHand.Find("RightHandIndex1"));
                    rightHandBones.Add(rightHand.Find("RightHandIndex1/RightHandIndex2"));
                    rightHandBones.Add(rightHand.Find("RightHandIndex1/RightHandIndex2/RightHandIndex3"));
                    rightHandBones.Add(rightHand.Find("RightHandMiddle1"));
                    rightHandBones.Add(rightHand.Find("RightHandMiddle1/RightHandMiddle2"));
                    rightHandBones.Add(rightHand.Find("RightHandMiddle1/RightHandMiddle2/RightHandMiddle3"));
                    rightHandBones.Add(rightHand.Find("RightHandRing1"));
                    rightHandBones.Add(rightHand.Find("RightHandRing1/RightHandRing2"));
                    rightHandBones.Add(rightHand.Find("RightHandRing1/RightHandRing2/RightHandRing3"));
                    rightHandBones.Add(rightHand.Find("RightHandPinky1"));
                    rightHandBones.Add(rightHand.Find("RightHandPinky1/RightHandPinky2"));
                    rightHandBones.Add(rightHand.Find("RightHandPinky1/RightHandPinky2/RightHandPinky3"));
                    rightHandBones.Add(rightHand.Find("RightHandThumb1"));
                    rightHandBones.Add(rightHand.Find("RightHandThumb1/RightHandThumb2"));
                    rightHandBones.Add(rightHand.Find("RightHandThumb1/RightHandThumb2/RightHandThumb3"));
                }
                lastLeftGrip = lastRightGrip = -1; // Force update
                ThreePointTrackedAvatar_OnLeftGripUpdate(grip:0);
                ThreePointTrackedAvatar_OnRightGripUpdate(grip:0);
            }
        }

        private void Loader_Failed(FailureEventArgs args)
        {

        }
    }
}
