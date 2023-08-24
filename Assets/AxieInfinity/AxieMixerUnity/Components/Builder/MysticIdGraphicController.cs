using Spine.Unity;
using UnityEngine;

namespace AxieMixer.Unity
{
    public class MysticIdGraphicController : MonoBehaviour
    {
        SkeletonGraphic skeletonGraphic;
        string bodyId;
        string bodyClass;
        bool isStarted;
        bool isFlipped;
        private void Awake()
        {
            skeletonGraphic = gameObject.GetComponent<SkeletonGraphic>();
        }

        public void Init(string bodyClass, string bodyId)
        {
            this.bodyClass = bodyClass;
            this.bodyId = bodyId;
            isStarted = false;
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(bodyClass) || string.IsNullOrEmpty(bodyId)) return;
            if ((!isStarted || transform.lossyScale.x < 0) != isFlipped)
            {
                isStarted = true;
                SyncId(!isFlipped);
            }
        }

        void SyncId(bool flipped)
        {
            isFlipped = flipped;
            var bodyClassSlotIdx = skeletonGraphic.Skeleton.FindSlotIndex("body-class");
            if (bodyClassSlotIdx != -1)
            {
                skeletonGraphic.Skeleton.SetAttachment("body-class", $"body-class-{bodyClass}");
            }

            for (int i = 0;i < 6;i++)
            {
                char c = (char)('f' - i);
                if (flipped)
                {
                    c = (char)('a' + i);
                }
                string slotName = $"body-id-{c}";
                var bodyIdUnitSlotIdx = skeletonGraphic.Skeleton.FindSlotIndex(slotName);
                if (bodyIdUnitSlotIdx == -1) break;

                int val = -1;
                if (i >= 0 && i < bodyId.Length && bodyId[i] >= '0' && bodyId[i] <= '9')
                {
                    val = bodyId[i] - '0';
                }
                if (val != -1)
                {
                    string attachmentName = $"body-id-{val:00}-{bodyClass}";
                    if (flipped)
                    {
                        attachmentName += "-flipx";
                    }
                    skeletonGraphic.Skeleton.SetAttachment(slotName, attachmentName);
                }
                else
                {
                    skeletonGraphic.Skeleton.FindSlot(slotName).Attachment = null;
                }
            }
        }
    }
}
