using AxieCore.AxieMixer;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace AxieMixer.Unity
{
    public class AutoBlendAnimGraphicController : MonoBehaviour
    {
        SkeletonGraphic skeletonGraphic;
        bool isIdle = false;

        private void Awake()
        {
            skeletonGraphic = gameObject.GetComponent<SkeletonGraphic>();
            skeletonGraphic.AnimationState.Start += HandleEvent;
        }

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0;i < (int)BoneComboType.count;i++)
            {
                BoneComboType boneType = (BoneComboType)i;
                string allTimeMixAnim = $"action/mix/{boneType}-animation";
                var anim = skeletonGraphic.Skeleton.Data.FindAnimation(allTimeMixAnim);
                if (anim == null) continue;
                skeletonGraphic.AnimationState.AddAnimation(1 + i, anim, true, 0f);
            }
        }

        void HandleEvent(TrackEntry trackEntry)
        {
            if (trackEntry.TrackIndex != 0) return;
            bool newIsIdle = trackEntry.Animation.Name == "action/idle/normal";
            if (isIdle == newIsIdle) return;
            isIdle = newIsIdle;

            for (int i = 0;i < (int)BoneComboType.count;i++)
            {
                BoneComboType boneType = (BoneComboType)i;
                string allTimeMixAnim = $"action/mix/normal-{boneType}-animation";
                var anim = skeletonGraphic.Skeleton.Data.FindAnimation(allTimeMixAnim);
                if (anim == null) continue;
                if (isIdle)
                {
                    skeletonGraphic.AnimationState.AddAnimation(1 + i, anim, true, 0f);
                }
                else
                {
                    skeletonGraphic.AnimationState.ClearTrack(i + i);
                }
            }
        }
    }
}
