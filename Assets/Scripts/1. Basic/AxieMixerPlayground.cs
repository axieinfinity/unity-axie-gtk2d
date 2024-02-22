using System.Collections;
using System.Collections.Generic;
using AxieCore.AxieMixer;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Game
{
    public class AxieMixerPlayground : MonoBehaviour
    {
        [SerializeField] Button mixBtn;
        [SerializeField] Button leftAnimBtn;
        [SerializeField] Button rightAnimBtn;
        [SerializeField] Dropdown animationDropDown;
        [SerializeField] InputField axieIdInputField;
        [SerializeField] Dropdown bodyDropDown;
        [SerializeField] Toggle allAxieToggle;
        [SerializeField] Toggle customIdToggle;
        [SerializeField] Toggle customLVToggle;
        [SerializeField] Toggle[] customPartToggles;
        [SerializeField] RectTransform rootTF;

        Axie2dBuilder builder => Mixer.Builder;

        const bool USE_GRAPHIC = false;
        int accessoryIdx = 1;

        static string[] ACCESSORY_SLOTS = new[]
          {
                "accessory-air",
                "accessory-cheek",
                "accessory-ground",
                "accessory-hip",
                "accessory-neck",
            };

        private void OnEnable()
        {
            mixBtn.onClick.AddListener(OnMixButtonClicked);
            allAxieToggle.onValueChanged.AddListener((b) => { if (b) OnSwitch(); });
            customIdToggle.onValueChanged.AddListener((b) => { if (b) OnSwitch(); });
            customLVToggle.onValueChanged.AddListener((b) => { OnCustomLvSwitch(); });
            animationDropDown.onValueChanged.AddListener((_) => OnAnimationChanged());
            leftAnimBtn.onClick.AddListener(() => OnAnimationStep(-1));
            rightAnimBtn.onClick.AddListener(() => OnAnimationStep(1));
        }

        private void OnDisable()
        {
            mixBtn.onClick.RemoveListener(OnMixButtonClicked);
            allAxieToggle.onValueChanged.RemoveAllListeners();
            customIdToggle.onValueChanged.RemoveAllListeners();
            customLVToggle.onValueChanged.RemoveAllListeners();
            animationDropDown.onValueChanged.RemoveAllListeners();
            leftAnimBtn.onClick.RemoveAllListeners();
            rightAnimBtn.onClick.RemoveAllListeners();
        }

        void Start()
        {
            Mixer.Init();
            List<string> animationList = builder.axieMixerMaterials.GetMixerStuff(AxieFormType.Normal).GetAnimatioNames();
            animationDropDown.ClearOptions();
            animationDropDown.AddOptions(animationList);

            //TestCombo();
            //TestAll();
            TestSummer();
            //ProcessMixer("", "0x90000000000001000080e020c40c00000001001028a084080001000008404408000003000800440c0000039408a0450600000300300041020000048008004104", USE_GRAPHIC);
            //var adultCombo = new Dictionary<string, string> {
            //        {"back", "aquatic-back-04" }, //blue-moon
            //        {"ears", "beast-ears-02" }, //nyan
            //        {"eyes", "reptile-eyes-02.1" }, //Crimson Gecko
            //        {"horn", "bird-horn-12" }, //feather-spear
            //        {"mouth", "plant-mouth-10" }, //silence-whisper
            //        {"tail", "bird-tail-08" }, //Cloud
            //        {"body-class", "beast" },
            //    };
            //string genes = Genes.FakeAxie512.FakeAxie(adultCombo);
            //Debug.Log($"axiepart:{genes}/back");
            //Debug.Log($"axiepart:{genes}/ear");
            //Debug.Log($"axiepart:{genes}/eyes");
            //Debug.Log($"axiepart:{genes}/horn");
            //Debug.Log($"axiepart:{genes}/mouth");
            //Debug.Log($"axiepart:{genes}/back");
            //ProcessMixer("", genes, true);
        }

        void OnSwitch()
        {
            bodyDropDown.gameObject.SetActive(allAxieToggle.isOn);
            axieIdInputField.gameObject.SetActive(customIdToggle.isOn);
        }

        void OnCustomLvSwitch()
        {
            for(int i = 0; i < customPartToggles.Length; i++)
            {
                customPartToggles[i].interactable = customLVToggle.isOn;
            }
        }

        void OnAnimationChanged()
        {
            var animName = animationDropDown.options[animationDropDown.value].text;
            var skeletonAnimations = FindObjectsOfType<SkeletonAnimation>();
            foreach (var p in skeletonAnimations)
            {
                p.state.SetAnimation(0, animName, true);
            }

            var skeletonGraphics = FindObjectsOfType<SkeletonGraphic>();
            foreach (var p in skeletonGraphics)
            {
                p.AnimationState.SetAnimation(0, animName, true);
            }
        }

        void OnAnimationStep(int step)
        {
            animationDropDown.value = (animationDropDown.value + step + animationDropDown.options.Count) % animationDropDown.options.Count;
        }

        void TestAll()
        {
            ClearAll();
            List<(string, string, int, int)> bodies = new List<(string, string, int, int)>();
            string[] specialBodys = new[]
            {
                "body-normal",
                "body-bigyak",
                "body-curly",
                "body-fuzzy",
                "body-spiky",
                "body-sumo",
                "body-wetdog",
                //"body-normal-accessory",
            };

            int k = 0;
            string bodyMode = bodyDropDown.options[bodyDropDown.value].text.ToLower().Replace("body ", "");
            for (int classIdx = 0; classIdx < 6; classIdx++)
            {
                var characterClass = (CharacterClass)classIdx;
                for (int classValue = 2; classValue <= 12; classValue += 2)
                {
                    string key = $"{characterClass}-{classValue:00}";
                    //
                    if (bodyMode == "random")
                    {
                        bodies.Add((key, specialBodys[(k++) % specialBodys.Length], classIdx, classValue));
                    }
                    else
                    {
                        bodies.Add((key, $"body-{bodyMode}", classIdx, classValue));
                    }
                }
            }

            for (int classIdx = 0; classIdx < 6; classIdx++)
            {
                var characterClass = (CharacterClass)classIdx;
                string key = $"{characterClass}-mystic-02";
                bodies.Add((key, (classIdx % 2 == 0) ? "body-mystic-normal" : "body-mystic-fuzzy", classIdx, 2));
                //bodies.Add((key, (classIdx % 2 == 0) ? "body-normal" : "body-fuzzy", classIdx, 2));
            }

            {
                for (int classValue = 1; classValue <= 2; classValue += 1)
                {
                    string key = $"xmas-{classValue:00}";
                    bodies.Add((key, "body-frosty", 0, classValue));
                }
            }
            {
                for (int classValue = 1; classValue <= 3;classValue += 1)
                {
                    string key = $"japan-{classValue:00}";
                    bodies.Add((key, "body-normal", 0, classValue));
                }

            }
            {
                for (int classValue = 0;classValue <= 1;classValue += 1)
                {
                    string key = $"agamo-{classValue:00}";
                    bodies.Add((key, "body-agamo", 0, classValue));
                }
            }
            bodies.Add(("summer-as", "body-normal", 0, 0));
            bodies.Add(("summer-a", "body-normal", 0, 0));


          
            int total = 0;
            foreach (var (key, body, classIdx, classValue) in bodies)
            {
                var characterClass = (CharacterClass)classIdx;
                string finalBody = body;
                string keyAdjust = key.Replace("-06", "-02").Replace("-12", "-04");
                var adultCombo = new Dictionary<string, string> {
                    {"back", key },
                    {"body", finalBody },
                    {"ears", key },
                    {"ear", key },
                    {"eyes", keyAdjust },
                    {"horn", key },
                    {"mouth", keyAdjust },
                    {"tail", key },
                    {"body-class", characterClass.ToString() },
                    {"body-id", " 2727 " },
                };
                 
                //foreach(var accessorySlot in ACCESSORY_SLOTS)
                //{
                //    adultCombo.Add(accessorySlot, $"{accessorySlot}1{System.Char.ConvertFromUtf32((int)('a') + accessoryIdx - 1)}");
                //}
                float scale = 0.0018f;
                byte colorVariant = (byte)builder.GetSampleColorVariant(characterClass, 2);

                {
                    if (customLVToggle.isOn)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            AxiePartType partType = (AxiePartType)i;
                            if (customPartToggles[i].isOn)
                            {
                                adultCombo.Add($"{partType}.lv2", "y");
                            }
                        }
                    }
                    var builderResult = builder.BuildSpineAdultCombo(adultCombo, colorVariant, scale);

                    //Test
                    GameObject go = new GameObject("DemoAxie");
                    int row = total / 6;
                    int col = total % 6;
                    //go.transform.localPosition = new Vector3(row * 1.6f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);
                    go.transform.localPosition = new Vector3(row * 1.85f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);

                    SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
                    runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
                    runtimeSkeletonAnimation.transform.localScale = Vector3.one;
                    var meshRenderer = runtimeSkeletonAnimation.GetComponent<MeshRenderer>();
                    meshRenderer.sortingOrder = 10 * total;
               

                    runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
                    runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

                    runtimeSkeletonAnimation.state.TimeScale = 0.25f;
                    //runtimeSkeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
                    if (builderResult.adultCombo.ContainsKey("body") &&
                          builderResult.adultCombo["body"].Contains("mystic") &&
                          builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                          builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
                    {
                        runtimeSkeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
                    }
                }
                total++;
            }
            Debug.Log("Done");
        }

        void TestCombo()
        {

            CharacterClass characterClass = CharacterClass.beast;
            string finalBody = "body-normal";
            string key = "beast-02";
            string keyAdjust = key.Replace("-06", "-02").Replace("-12", "-04");
            var adultCombo = new Dictionary<string, string> {
                    {"back", key },
                    {"body", finalBody },
                    {"ears", key },
                    {"ear", key },
                    {"eyes", keyAdjust },
                    {"horn", key },
                    {"mouth", keyAdjust },
                    {"tail", key },
                    {"body-class", characterClass.ToString() },
                    {"body-id", " 2727 " },
                };

            float scale = 0.0032f;
            var builderResult = builder.BuildSpineAdultCombo(adultCombo, 0, scale);

            GameObject go = new GameObject("DemoAxie");
            int row = 0 / 3;
            int col = 0 % 3;
            //go.transform.localPosition = new Vector3(row * 1.6f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);
            go.transform.localPosition = new Vector3(row * 2.85f, col * 2.5f) - new Vector3(6.9f, 4.8f, 0);

            SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
            runtimeSkeletonAnimation.transform.localScale = Vector3.one;
            var meshRenderer = runtimeSkeletonAnimation.GetComponent<MeshRenderer>();
            
            runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

            runtimeSkeletonAnimation.state.TimeScale = 0.5f;
        }
        void TestSummer()
        {
            List<Dictionary<string, string>> lst = new List<Dictionary<string, string>>();

            var summer0 = new Dictionary<string, string> {
                    {"back", "summer-a-lv2" },
                    {"body", "body-summer" },
                    {"ears", "summer-a-lv2" },
                    {"ear", "summer-a-lv2" },
                    {"eyes", "summer-a-lv2" },
                    {"horn", "summer-tc-lv2" },
                    {"mouth", "summer-a-lv2" },
                    {"tail", "summer-a-lv2" },
                };
            lst.Add(summer0);

            var summer1 = new Dictionary<string, string> {
                    {"back", "summer-ta-lv2" },
                    {"body", "body-summer" },
                    {"ears", "summer-a-lv2" },
                    {"ear", "summer-a-lv2" },
                    {"eyes", "summer-a-lv2" },
                    {"horn", "summer-a-lv2" },
                    {"mouth", "summer-a-lv2" },
                    {"tail", "summer-a-lv2" },
                };
            lst.Add(summer1);

            var summer2 = new Dictionary<string, string> {
                    {"back", "summer-a-lv2" },
                    {"body", "body-summer" },
                    {"ears", "summer-a-lv2" },
                    {"ear", "summer-a-lv2" },
                    {"eyes", "summer-a-lv2" },
                    {"horn", "summer-td-lv2" },
                    {"mouth", "summer-a-lv2" },
                    {"tail", "summer-a-lv2" },
                };
            lst.Add(summer2);

            var summer3 = new Dictionary<string, string> {
                    {"back", "summer-tc-lv2" },
                    {"body", "body-summer" },
                    {"ears", "summer-as-lv2" },
                    {"ear", "summer-as-lv2" },
                    {"eyes", "summer-as-lv2" },
                    {"horn", "summer-ta-lv2" },
                    {"mouth", "summer-as-lv2" },
                    {"tail", "summer-as-lv2" },
                };
            lst.Add(summer3);

            var summer4 = new Dictionary<string, string> {
                    {"back", "summer-as-lv2" },
                    {"body", "body-summer" },
                    {"ears", "summer-as-lv2" },
                    {"ear", "summer-as-lv2" },
                    {"eyes", "summer-as-lv2" },
                    {"horn", "summer-as-lv2" },
                    {"mouth", "summer-as-lv2" },
                    {"tail", "summer-as-lv2" },
                };
            lst.Add(summer4);

            var summer5 = new Dictionary<string, string> {
                    {"back", "summer-tc-lv2" },
                    {"body", "body-summer" },
                    {"ears", "summer-as-lv2" },
                    {"ear", "summer-as-lv2" },
                    {"eyes", "summer-as-lv2" },
                    {"horn", "summer-tb-lv2" },
                    {"mouth", "summer-as-lv2" },
                    {"tail", "summer-as-lv2" },
                };
            lst.Add(summer5);

            var japan1 = new Dictionary<string, string> {
                    {"back", "japan-03" },
                    {"body", "body-fuzzy" },
                    {"ears", "japan-03" },
                    {"ear", "japan-03" },
                    {"eyes", "japan-03" },
                    {"horn", "japan-03" },
                    {"mouth", "japan-03" },
                    {"tail", "japan-03" },
                };
            lst.Add(japan1);

            float scale = 0.0032f;
            int total = 0;
            byte colorVariant = 49;

            foreach (var adultCombo in lst)
            {
                //var builderResult = builder.BuildSpineAdultCombo(adultCombo, (byte)(colorVariant + total), scale);
                var builderResult = builder.BuildSpineAdultCombo(adultCombo, (byte)(colorVariant + total), scale);

                GameObject go = new GameObject("DemoAxie");
                int row = total / 3;
                int col = total % 3;
                //go.transform.localPosition = new Vector3(row * 1.6f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);
                go.transform.localPosition = new Vector3(row * 2.85f, col * 2.5f) - new Vector3(6.9f, 4.8f, 0);

                SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
                runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
                runtimeSkeletonAnimation.transform.localScale = Vector3.one;
                var meshRenderer = runtimeSkeletonAnimation.GetComponent<MeshRenderer>();
                meshRenderer.sortingOrder = 10 * total;
                total++;

                runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
                runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

                runtimeSkeletonAnimation.state.TimeScale = 0.5f;
            }
        }

        void ProcessMixer(string axieId, string genesStr, bool isGraphic)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }
            float scale = 0.017f;

            var meta = new Dictionary<string, string>();
            //foreach (var accessorySlot in ACCESSORY_SLOTS)
            //{
            //    meta.Add(accessorySlot, $"{accessorySlot}1{System.Char.ConvertFromUtf32((int)('a') + accessoryIdx - 1)}");
            //}
            if (customLVToggle.isOn)
            {
                for (int i = 0; i < 6; i++)
                {
                    AxiePartType partType = (AxiePartType)i;
                    if (customPartToggles[i].isOn)
                    {
                        meta.Add($"{partType}.lv2", "y");
                    }
                }
            }
            var builderResult = builder.BuildSpineFromGene(axieId, genesStr, meta, scale, isGraphic);

            //Test
            if (isGraphic)
            {
                SpawnSkeletonGraphic(builderResult);
            }
            else
            {
                SpawnSkeletonAnimation(builderResult);
            }
        }

        void ClearAll()
        {
            var skeletonAnimations = FindObjectsOfType<SkeletonAnimation>();
            foreach (var p in skeletonAnimations)
            {
                Destroy(p.transform.parent.gameObject);
            }
            var skeletonGraphics = FindObjectsOfType<SkeletonGraphic>();
            foreach (var p in skeletonGraphics)
            {
                Destroy(p.transform.gameObject);
            }
        }

        void SpawnSkeletonAnimation(Axie2dBuilderResult builderResult)
        {
            ClearAll();
            GameObject go = new GameObject("DemoAxie");
            go.transform.localPosition = new Vector3(0f, -2.4f, 0f);
            SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
            runtimeSkeletonAnimation.transform.localScale = Vector3.one;

            runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

            if (builderResult.adultCombo.ContainsKey("body") &&
                builderResult.adultCombo["body"].Contains("mystic") &&
                builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                runtimeSkeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
            }
            runtimeSkeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
        }

        void SpawnSkeletonGraphic(Axie2dBuilderResult builderResult)
        {
            ClearAll();

            var skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(builderResult.skeletonDataAsset, rootTF, builderResult.sharedGraphicMaterial);
            skeletonGraphic.rectTransform.sizeDelta = new Vector2(1, 1);
            skeletonGraphic.rectTransform.localScale = Vector3.one;
            skeletonGraphic.rectTransform.anchoredPosition = new Vector2(0f, -260f);
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Skeleton.SetSkin("default");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            skeletonGraphic.gameObject.AddComponent<AutoBlendAnimGraphicController>();
            skeletonGraphic.AnimationState.SetAnimation(0, "action/idle/normal", true);

            if (builderResult.adultCombo.ContainsKey("body") &&
             builderResult.adultCombo["body"].Contains("mystic") &&
             builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
             builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                skeletonGraphic.gameObject.AddComponent<MysticIdGraphicController>().Init(bodyClass, bodyId);
            }
        }

        bool isFetchingGenes = false;
        public void OnMixButtonClicked()
        {
            if (allAxieToggle.isOn)
            {
                TestAll();
            }
            else
            {
                if (isFetchingGenes) return;
                StartCoroutine(GetAxiesGenes(axieIdInputField.text));
            }
        }

        public IEnumerator GetAxiesGenes(string axieId)
        {
            isFetchingGenes = true;
            string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes}}";
            JObject jPayload = new JObject();
            jPayload.Add(new JProperty("query", searchString));

            var wr = new UnityWebRequest("https://graphql-gateway.axieinfinity.com/graphql", "POST");
            //var wr = new UnityWebRequest("https://testnet-graphql.skymavis.one/graphql", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jPayload.ToString().ToCharArray());
            wr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            wr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            wr.SetRequestHeader("Content-Type", "application/json");
            wr.timeout = 10;
            yield return wr.SendWebRequest();
            if (wr.error == null)
            {
                var result = wr.downloadHandler != null ? wr.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jResult = JObject.Parse(result);
                    string genesStr = (string)jResult["data"]["axie"]["newGenes"];
                    Debug.Log(genesStr);
                    ProcessMixer(axieId, genesStr, USE_GRAPHIC);
                }
            }
            isFetchingGenes = false;
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown($"{i}"))
                {
                    accessoryIdx = i;
                    this.TestAll();
                }
            }
        }
    }
}
