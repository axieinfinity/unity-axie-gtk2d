using AxieMixer.Unity;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Game
{
    public class AxieFigure : MonoBehaviour
    {
        public string id;
        public string genes;
        private SkeletonAnimation skeletonAnimation;

        [SerializeField] private bool _flipX = false;
        public bool flipX
        {
            get
            {
                return _flipX;
            }
            set
            {
                _flipX = value;
                if (skeletonAnimation != null)
                {
                    skeletonAnimation.skeleton.ScaleX = (_flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
                }
            }
        }

        private void Awake()
        {
            skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
            
            // Shouldn't be here, but it's useful
            Mixer.Init();
            Mixer.SpawnSkeletonAnimation(skeletonAnimation, id, genes);

            skeletonAnimation.transform.localPosition = new Vector3(0f, -0.32f, 0f);
            skeletonAnimation.transform.SetParent(transform, false);
            skeletonAnimation.transform.localScale = new Vector3(1, 1, 1);
            skeletonAnimation.skeleton.ScaleX = (_flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
            skeletonAnimation.timeScale = 0.5f;
            skeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
            skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
            skeletonAnimation.state.End += SpineEndHandler;
        }

        private void OnDisable()
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.state.End -= SpineEndHandler;
            }
        }

        public void DoJumpAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "action/move-forward", false);
        }

        public void DoAttackMeleeAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "attack/melee/tail-roll", false);
        }


        public void DoAttackRangedAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "attack/ranged/tail-roll", false);
        }

        public void DoBuffAnim()
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, "attack/ranged/tail-roll", false);
        }


        private void SpineEndHandler(TrackEntry trackEntry)
        {
            string animation = trackEntry.Animation.Name;
            if (animation == "action/move-forward")
            {
                skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
                skeletonAnimation.timeScale = 0.5f;
            }
        }
    }
}
