using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genes
{
    public class FakeAxie512
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Test/Fake Genes")]
        static void TestFakeGenes()
        {

            var adultCombo = new Dictionary<string, string> {
                    {"back", "beast-back-04.0" },
                    {"ears", "beast-ears-06.1" },
                    {"eyes", "bug-eyes-02" },
                    {"horn", "aquatic-horn-02" },
                    {"mouth", "bird-mouth-02" },
                    {"tail", "reptile-tail-10" },
                    {"body-class", "beast" },
                };
            string genes = FakeAxie(adultCombo);
            Debug.Log(genes);
        }
#endif
        static string[] AXIE_PART_TYPES = new string[]
        {
            "eyes", "mouth", "ears", "horn", "back", "tail"
        };

        private static int CharacterClassToValue(string className)
        {
            switch (className)
            {
                case "beast": return 0;
                case "bug": return 1;
                case "bird": return 2;
                case "plant": return 3;
                case "aquatic": return 4;
                case "reptile": return 5;
                case "mech": return 16;
                case "dawn": return 17;
                case "dusk": return 18;
            }
            return 0;
        }
        public static string FakeAxie(Dictionary<string, string> adultCombo)
        {
            var genes = "";
            int mainClass = CharacterClassToValue(adultCombo["body-class"]);

            genes += Convert.ToString(mainClass, 2).PadLeft(5, '0');
            genes += Convert.ToString(0, 2).PadLeft(45, '0'); //reservation
            genes += Convert.ToString(0, 2).PadLeft(5, '0'); //contribution
            genes += Convert.ToString(0, 2).PadLeft(1, '0'); //bodySkinInheritability
            genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodySkin
            genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodyDetail0
            genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodyDetail1
            genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodyDetail2

            genes += Convert.ToString(0, 2).PadLeft(6, '0'); //primaryColor0
            genes += Convert.ToString(0, 2).PadLeft(6, '0'); //primaryColor1
            genes += Convert.ToString(0, 2).PadLeft(6, '0'); //primaryColor2

            genes += Convert.ToString(0, 2).PadLeft(6, '0'); //secondaryColor0
            genes += Convert.ToString(0, 2).PadLeft(6, '0'); //secondaryColor1
            genes += Convert.ToString(0, 2).PadLeft(6, '0'); //secondaryColor2

            for (int partIndex = 0; partIndex < 6; partIndex++)
            {
                var partType = AXIE_PART_TYPES[partIndex];

                string partTag = adultCombo[partType];
                string[] partWords = partTag.Split('.');
                int partSkin = 0;
                string partId;
                if (partWords.Length == 2)
                {
                    partId = partWords[0];
                    int.TryParse(partWords[1], out partSkin);
                }
                else
                {
                    partId = partTag;
                }

                partWords = partId.Split('-');
                string partClass = partWords[0];
                int.TryParse(partWords[2], out var partValue);

                genes += Convert.ToString(0, 2).PadLeft(2, '0'); //partStage
                genes += Convert.ToString(0, 2).PadLeft(13, '0'); //partReservation
                genes += Convert.ToString(0, 2).PadLeft(1, '0'); //partSkinInheritability
                genes += Convert.ToString(partSkin, 2).PadLeft(9, '0'); //partSkin

                genes += Convert.ToString(CharacterClassToValue(partClass), 2).PadLeft(5, '0');
                genes += Convert.ToString(partValue, 2).PadLeft(8, '0');

                genes += Convert.ToString(CharacterClassToValue(partClass), 2).PadLeft(5, '0');
                genes += Convert.ToString(partValue, 2).PadLeft(8, '0');

                genes += Convert.ToString(CharacterClassToValue(partClass), 2).PadLeft(5, '0');
                genes += Convert.ToString(partValue, 2).PadLeft(8, '0');
            }

            var hex = string.Join("",
              Enumerable.Range(0, genes.Length / 8)
              .Select(i => Convert.ToByte(genes.Substring(i * 8, 8), 2).ToString("X2")));
            return hex;
        }
    }
}
