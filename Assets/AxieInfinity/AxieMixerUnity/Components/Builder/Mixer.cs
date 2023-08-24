using System.Collections.Generic;
using System.IO;
using System.Linq;
using AxieCore.AxieMixer;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;

namespace AxieMixer.Unity
{
    public static class Mixer
    {
        private const string StuffName = "axie-2d-v3-stuff";

        private static bool initialized;
        private static Axie2dBuilder builder;

        public static Axie2dBuilder Builder
        {
            get
            {
                if (!initialized)
                {
                    Debug.LogWarning("Mixer is not initialzed. Please call Mixer.Init() first!");
                    return null;
                }
                return builder;
            }
        }

        /// <summary>
        /// Init the AxieMixer, calling this function multiple times will do nothing
        /// </summary>
        public static void Init()
        {
            if (initialized)
                return;

            initialized = true;
            LoadMixer();
        }

        public static void SpawnSkeletonAnimation(SkeletonAnimation skeletonAnimation, string axieId, string genesStr, float scale = 0.0016f)
        {
            var result = Builder.BuildSpineFromGene(axieId, genesStr, scale);
            skeletonAnimation.skeletonDataAsset = result.skeletonDataAsset;
            skeletonAnimation.Initialize(true);
            if (result.adultCombo.ContainsKey("body") &&
                result.adultCombo["body"].Contains("mystic") &&
                result.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                result.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                skeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
            }
        }

        public static void SpawnSkeletonAnimation(SkeletonGraphic skeletonGraphic, string axieId, string genesStr, float scale = 0.0016f)
        {
            var result = Builder.BuildSpineFromGene(axieId, genesStr, scale);
            skeletonGraphic.skeletonDataAsset = result.skeletonDataAsset;
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Skeleton.SetSkin("default");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.gameObject.AddComponent<AutoBlendAnimGraphicController>();
            skeletonGraphic.material = Builder.axieMixerMaterials.GetSampleGraphicMaterial(AxieFormType.Normal);
            skeletonGraphic.SetMaterialDirty();
            //var skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(builderResult.skeletonDataAsset, rootTF, builderResult.sharedGraphicMaterial);
            if (result.adultCombo.ContainsKey("body") &&
             result.adultCombo["body"].Contains("mystic") &&
             result.adultCombo.TryGetValue("body-class", out var bodyClass) &&
             result.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                skeletonGraphic.gameObject.AddComponent<MysticIdGraphicController>().Init(bodyClass, bodyId);
            }
        }

        private static void LoadMixer()
        {
            string genesStuffJsonString = Resources.Load<TextAsset>($"{StuffName}/axie-2d-v3-stuff-genes").text;
            string stuffSamplesJsonString = Resources.Load<TextAsset>($"{StuffName}/axie-2d-v3-stuff-samples").text;
            string stuffAnimationsJsonString = Resources.Load<TextAsset>($"{StuffName}/axie-2d-v3-stuff-animations").text;
            string stuffMaterialsString = Resources.Load<TextAsset>($"{StuffName}/axie-2d-v3-stuff-materials").text;

            var baseMaterials = LoadAxieMaterials(stuffMaterialsString);
            var atlasStuffMap = LoadAxieAtlasStuff();

            atlasStuffMap.TryGetValue("atlas-single", out var singleAtlasAsset);

            var genesStuff = new AxieGenesStuff();
            genesStuff.Load(genesStuffJsonString);

            var axieMixerStuff = new AxieMixerStuff();
            axieMixerStuff.Load(stuffSamplesJsonString, stuffAnimationsJsonString);

            var axieMixerMaterials = new AxieMixerMaterials();
            axieMixerMaterials.InstallStuff(AxieFormType.Normal, singleAtlasAsset, null, genesStuff, axieMixerStuff, baseMaterials);

            builder = new Axie2dBuilder();
            builder.Init(axieMixerMaterials);
        }

        private static Dictionary<string, SplatAtlasStuff> LoadAxieAtlasStuff()
        {
            Dictionary<string, SplatAtlasStuff> atlasStuffMap = new Dictionary<string, SplatAtlasStuff>();
            List<string> srcList = new List<string>();
            srcList.Add("atlas-single");

            var swapTexture = Resources.Load<Texture2D>($"{StuffName}/axie-2d-v3-swap-tex");
            foreach (var quality in srcList)
            {
                Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
                var atlasAsset = Resources.Load<TextAsset>($"{StuffName}/{quality}/axie-2d-v3-stuff");
                string[] lines = atlasAsset.text.Split('\n').Where(x => x.Contains(".png")).Select(x => x.Replace(".png", "")).ToArray();
                foreach (var texName in lines)
                {
                    var colorTexture = Resources.Load<Texture2D>($"{StuffName}/{quality}/{texName}_color");
                    colorTexture.name = texName;
                    var lineTexture = Resources.Load<Texture2D>($"{StuffName}/{quality}/{texName}_line");
                    var splat0Texture = Resources.Load<Texture2D>($"{StuffName}/{quality}/{texName}_splat0");
                    var splat1Texture = Resources.Load<Texture2D>($"{StuffName}/{quality}/{texName}_splat1");
                    textures.Add($"[{texName}]_MainTex", colorTexture);
                    textures.Add($"[{texName}]_LineTex", lineTexture);
                    textures.Add($"[{texName}]_Splat0Tex", splat0Texture);
                    textures.Add($"[{texName}]_Splat1Tex", splat1Texture);
                    textures.Add($"[{texName}]_SwapTex", swapTexture);
                }

                SplatAtlasStuff splatAtlasStuff = new SplatAtlasStuff
                {
                    tag = quality,
                    atlasAssetText = atlasAsset.text,
                    textures = textures
                };
                atlasStuffMap.Add(quality, splatAtlasStuff);
            }
            return atlasStuffMap;
        }

        private static Dictionary<string, Material> LoadAxieMaterials(string stuffMaterialsString)
        {
            Dictionary<string, Material> materialGroups = new Dictionary<string, Material>();
            JObject jData = JObject.Parse(stuffMaterialsString);
            int version = 0;
            if (jData != null && jData["version"] != null)
            {
                version = (int)jData["version"];
            }
            if (version == 2)
            {
                var jItems = jData["items"] as JArray;

                for (int i = 0; i < jItems.Count; i++)
                {
                    var jItem = jItems[i] as JObject;
                    var matName = (string)jItem["name"];
                    var shaderName = (string)jItem["shaderName"];
                    var shader = Shader.Find(shaderName);
                    if (shader == null)
                    {
                        Debug.LogWarning($"Shader {shaderName} not found");
                        continue;
                    }
                    var material = new Material(shader);
                    material.hideFlags = HideFlags.HideAndDontSave;
                    material.renderQueue = 3000;
                    material.enableInstancing = false;

                    materialGroups.Add(matName, material);
                }
            }
            return materialGroups;
        }
    }
}
