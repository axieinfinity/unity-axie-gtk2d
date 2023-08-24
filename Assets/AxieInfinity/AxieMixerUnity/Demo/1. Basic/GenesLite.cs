using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GenesLite : MonoBehaviour
{
    public class BitExtractor
    {
        BigInteger value;
        int bitsLeft;

        public BitExtractor(BigInteger value, int totalBits)
        {
            this.value = value;
            this.bitsLeft = totalBits;
        }

        public int peek(int numBits)
        {
            //todo: bigint or int
            var bitOn = new BigInteger((1 << numBits) - 1); // todo: check it
            if (bitsLeft > numBits)
            {
                var peeked = (value >> (bitsLeft - numBits)) & bitOn;
                return (int)peeked;
            }
            else
            {
                var peeked = value & bitOn;
                return (int)peeked;
            }
        }

        /// <returns>-1 means NULL</returns>
        public int extract(int numBits)
        {
            //todo: bigint or int
            if (bitsLeft == 0)
                return -1; // todo: return null

            var extracted = peek(numBits);
            if (bitsLeft > numBits)
            {
                bitsLeft = bitsLeft - numBits;
            }
            else
            {
                bitsLeft = 0;
            }

            return extracted;
        }
    }

    [UnityEditor.MenuItem("Test/Decode Genes")]
    static void TestDecodeGenes()
    {
        string colorStr = @"[
          {'index': 0, 'key': 'beast-00', 'skin': 0, 'class': 'beast', 'color_value': 0, 'primary1': 'fdfcf2', 'shaded1': 'ddd6ae', 'primary2': 'af8c56', 'shaded2': '7d542c', 'line': 'fa9000', 'partColorShift': '0000000'},
          {'index': 1, 'key': 'beast-01', 'skin': 0, 'class': 'beast', 'color_value': 1, 'primary1': '544f44', 'shaded1': '3f392f', 'primary2': 'edd8b8', 'shaded2': 'c19872', 'line': '6b4f24', 'partColorShift': '0000000'},
          {'index': 2, 'key': 'beast-02', 'skin': 0, 'class': 'beast', 'color_value': 2, 'primary1': 'ffd500', 'shaded1': 'f8a500', 'primary2': 'fffeda', 'shaded2': 'f3d459', 'line': 'fa9000', 'partColorShift': '0000000'},
          {'index': 3, 'key': 'beast-03', 'skin': 0, 'class': 'beast', 'color_value': 3, 'primary1': 'fdb014', 'shaded1': 'eb7e00', 'primary2': 'fef0a3', 'shaded2': 'f0bb53', 'line': 'ff8000', 'partColorShift': '0000000'},
          {'index': 4, 'key': 'beast-04', 'skin': 0, 'class': 'beast', 'color_value': 4, 'primary1': 'f5a037', 'shaded1': 'd98035', 'primary2': 'fde5b6', 'shaded2': 'ecb686', 'line': 'ff8000', 'partColorShift': '0000000'},
          {'index': 5, 'key': 'beast-06', 'skin': 0, 'class': 'beast', 'color_value': 6, 'primary1': '427cad', 'shaded1': '1362a2', 'primary2': 'ffcd63', 'shaded2': 'dd9200', 'line': '0063af', 'partColorShift': '0000000'},
          {'index': 6, 'key': 'plant-00', 'skin': 0, 'class': 'plant', 'color_value': 0, 'primary1': 'fefdf1', 'shaded1': 'd2d9b3', 'primary2': '98984a', 'shaded2': '606718', 'line': 'a0be00', 'partColorShift': '0000000'},
          {'index': 7, 'key': 'plant-01', 'skin': 0, 'class': 'plant', 'color_value': 1, 'primary1': '4e523e', 'shaded1': '414630', 'primary2': 'efe5a7', 'shaded2': 'b0a657', 'line': '415800', 'partColorShift': '0000000'},
          {'index': 8, 'key': 'plant-02', 'skin': 0, 'class': 'plant', 'color_value': 2, 'primary1': 'afdb1b', 'shaded1': '75ba00', 'primary2': 'e3efdb', 'shaded2': 'aebd44', 'line': '70ce00', 'partColorShift': '0000000'},
          {'index': 9, 'key': 'plant-03', 'skin': 0, 'class': 'plant', 'color_value': 3, 'primary1': 'decd00', 'shaded1': 'a89b00', 'primary2': 'f3eaba', 'shaded2': 'c3b641', 'line': 'aab900', 'partColorShift': '0000000'},
          {'index': 10, 'key': 'plant-04', 'skin': 0, 'class': 'plant', 'color_value': 4, 'primary1': '99ff73', 'shaded1': '6ada00', 'primary2': 'fbfdf1', 'shaded2': 'b1e180', 'line': '10dd00', 'partColorShift': '0000000'},
          {'index': 11, 'key': 'aquatic-00', 'skin': 0, 'class': 'aquatic', 'color_value': 0, 'primary1': 'f4fff4', 'shaded1': 'b5e1cc', 'primary2': '3ca1d9', 'shaded2': '1170aa', 'line': '00bbff', 'partColorShift': '0000000'},
          {'index': 12, 'key': 'aquatic-01', 'skin': 0, 'class': 'aquatic', 'color_value': 1, 'primary1': '4d545e', 'shaded1': '3d434b', 'primary2': 'a9ead5', 'shaded2': '44b2b2', 'line': '395089', 'partColorShift': '0000000'},
          {'index': 13, 'key': 'aquatic-02', 'skin': 0, 'class': 'aquatic', 'color_value': 2, 'primary1': '00f1d2', 'shaded1': '00c6cc', 'primary2': 'befedb', 'shaded2': '4ddcc6', 'line': '00d4f0', 'partColorShift': '0000000'},
          {'index': 14, 'key': 'aquatic-03', 'skin': 0, 'class': 'aquatic', 'color_value': 3, 'primary1': '00dff3', 'shaded1': '00add6', 'primary2': 'bffef4', 'shaded2': '59d7da', 'line': '00d4f0', 'partColorShift': '0000000'},
          {'index': 15, 'key': 'aquatic-04', 'skin': 0, 'class': 'aquatic', 'color_value': 4, 'primary1': '00b8ff', 'shaded1': '008de7', 'primary2': 'c9eeff', 'shaded2': '00cef7', 'line': '00bbff', 'partColorShift': '0000000'},
          {'index': 16, 'key': 'aquatic-06', 'skin': 0, 'class': 'aquatic', 'color_value': 6, 'primary1': 'e12f64', 'shaded1': 'bc0056', 'primary2': '62ffe2', 'shaded2': '00c2c7', 'line': 'e50060', 'partColorShift': '0000000'},
          {'index': 17, 'key': 'bug-00', 'skin': 0, 'class': 'bug', 'color_value': 0, 'primary1': 'fffaf5', 'shaded1': 'efccbb', 'primary2': 'e22737', 'shaded2': '970940', 'line': 'de0000', 'partColorShift': '0000000'},
          {'index': 18, 'key': 'bug-01', 'skin': 0, 'class': 'bug', 'color_value': 1, 'primary1': '553b39', 'shaded1': '432e30', 'primary2': 'ffe7e2', 'shaded2': 'ff9e94', 'line': '6f241e', 'partColorShift': '0000000'},
          {'index': 19, 'key': 'bug-02', 'skin': 0, 'class': 'bug', 'color_value': 2, 'primary1': 'ff606c', 'shaded1': 'f2213e', 'primary2': 'fff4f9', 'shaded2': 'ffa9af', 'line': 'de0000', 'partColorShift': '0000000'},
          {'index': 20, 'key': 'bug-03', 'skin': 0, 'class': 'bug', 'color_value': 3, 'primary1': 'ff433e', 'shaded1': 'dc1244', 'primary2': 'ffdcce', 'shaded2': 'ffa587', 'line': 'de0000', 'partColorShift': '0000000'},
          {'index': 21, 'key': 'bug-04', 'skin': 0, 'class': 'bug', 'color_value': 4, 'primary1': 'df2e54', 'shaded1': 'b4154f', 'primary2': 'ffd8d6', 'shaded2': 'ff9f96', 'line': 'de0000', 'partColorShift': '0000000'},
          {'index': 22, 'key': 'bird-00', 'skin': 0, 'class': 'bird', 'color_value': 0, 'primary1': 'fff7ff', 'shaded1': 'f6c1e3', 'primary2': 'fa59a0', 'shaded2': 'cc2e6f', 'line': 'e40086', 'partColorShift': '0000000'},
          {'index': 23, 'key': 'bird-01', 'skin': 0, 'class': 'bird', 'color_value': 1, 'primary1': '583c44', 'shaded1': '4a303a', 'primary2': 'fec3dd', 'shaded2': 'e37aa5', 'line': '6d2d5e', 'partColorShift': '0000000'},
          {'index': 24, 'key': 'bird-02', 'skin': 0, 'class': 'bird', 'color_value': 2, 'primary1': 'ff99b0', 'shaded1': 'f6749d', 'primary2': 'ffd8f3', 'shaded2': 'f89dcc', 'line': 'e40086', 'partColorShift': '0000000'},
          {'index': 25, 'key': 'bird-03', 'skin': 0, 'class': 'bird', 'color_value': 3, 'primary1': 'ffafc1', 'shaded1': 'f78299', 'primary2': 'ffe0e7', 'shaded2': 'ffa5b3', 'line': 'e40086', 'partColorShift': '0000000'},
          {'index': 26, 'key': 'bird-04', 'skin': 0, 'class': 'bird', 'color_value': 4, 'primary1': 'ff78b4', 'shaded1': 'f358a1', 'primary2': 'ffedff', 'shaded2': 'ffb0e1', 'line': 'e40086', 'partColorShift': '0000000'},
          {'index': 27, 'key': 'reptile-00', 'skin': 0, 'class': 'reptile', 'color_value': 0, 'primary1': 'fff7ff', 'shaded1': 'e2c8ef', 'primary2': 'bb68ce', 'shaded2': '834296', 'line': '9f00ef', 'partColorShift': '0000000'},
          {'index': 28, 'key': 'reptile-01', 'skin': 0, 'class': 'reptile', 'color_value': 1, 'primary1': '5e436d', 'shaded1': '533759', 'primary2': 'f4cdfe', 'shaded2': 'd38ed3', 'line': '733ba0', 'partColorShift': '0000000'},
          {'index': 29, 'key': 'reptile-02', 'skin': 0, 'class': 'reptile', 'color_value': 2, 'primary1': 'c569cf', 'shaded1': 'a754b4', 'primary2': 'ffd2fe', 'shaded2': 'f78ee2', 'line': 'd900d0', 'partColorShift': '0000000'},
          {'index': 30, 'key': 'reptile-03', 'skin': 0, 'class': 'reptile', 'color_value': 3, 'primary1': '9967fb', 'shaded1': '7d4ce7', 'primary2': 'dac3ff', 'shaded2': 'a988ff', 'line': '8d24ff', 'partColorShift': '0000000'},
          {'index': 31, 'key': 'reptile-04', 'skin': 0, 'class': 'reptile', 'color_value': 4, 'primary1': 'f8bbff', 'shaded1': 'e28ddd', 'primary2': 'fff1fe', 'shaded2': 'fbaee9', 'line': 'e90fe7', 'partColorShift': '0000000'},
          {'index': 32, 'key': 'reptile-06', 'skin': 0, 'class': 'reptile', 'color_value': 6, 'primary1': '00c15f', 'shaded1': '009291', 'primary2': 'f4aaff', 'shaded2': 'c56fff', 'line': '008cd1', 'partColorShift': '0000000'},
          {'index': 33, 'key': 'dawn-00', 'skin': 0, 'class': 'dawn', 'color_value': 0, 'primary1': 'fcfbf2', 'shaded1': 'aee5c4', 'primary2': 'd4f8d0', 'shaded2': '7ddbcf', 'line': '7d7300', 'partColorShift': '0000000'},
          {'index': 34, 'key': 'dawn-01', 'skin': 0, 'class': 'dawn', 'color_value': 1, 'primary1': '594026', 'shaded1': '452e1f', 'primary2': 'fede29', 'shaded2': 'e88e00', 'line': '702f00', 'partColorShift': '0000000'},
          {'index': 35, 'key': 'dawn-02', 'skin': 0, 'class': 'dawn', 'color_value': 2, 'primary1': 'bbbaff', 'shaded1': 'ba96ff', 'primary2': 'eaf8ff', 'shaded2': 'a7b6ff', 'line': '9933ff', 'partColorShift': '0000000'},
          {'index': 36, 'key': 'dawn-03', 'skin': 0, 'class': 'dawn', 'color_value': 3, 'primary1': 'ffff8d', 'shaded1': 'ffd200', 'primary2': 'feffe3', 'shaded2': '88e9d7', 'line': 'e66c00', 'partColorShift': '0000000'},
          {'index': 37, 'key': 'dawn-04', 'skin': 0, 'class': 'dawn', 'color_value': 4, 'primary1': 'a5ffff', 'shaded1': 'f6baff', 'primary2': 'ffffff', 'shaded2': 'ffb2d9', 'line': 'a527ff', 'partColorShift': '0000000'},
          {'index': 38, 'key': 'dusk-00', 'skin': 0, 'class': 'dusk', 'color_value': 0, 'primary1': 'f4fefb', 'shaded1': '93ebd1', 'primary2': 'feffff', 'shaded2': 'a4e7e0', 'line': '007c78', 'partColorShift': '0000000'},
          {'index': 39, 'key': 'dusk-01', 'skin': 0, 'class': 'dusk', 'color_value': 1, 'primary1': '1d4d4f', 'shaded1': '1b3b47', 'primary2': 'd0fec0', 'shaded2': '00dfc6', 'line': '00445b', 'partColorShift': '0000000'},
          {'index': 40, 'key': 'dusk-02', 'skin': 0, 'class': 'dusk', 'color_value': 2, 'primary1': '0097a0', 'shaded1': '006e95', 'primary2': 'abffac', 'shaded2': '00e180', 'line': '0099bc', 'partColorShift': '0000000'},
          {'index': 41, 'key': 'dusk-03', 'skin': 0, 'class': 'dusk', 'color_value': 3, 'primary1': '007181', 'shaded1': '005372', 'primary2': '62feda', 'shaded2': '00dabb', 'line': '006e99', 'partColorShift': '0000000'},
          {'index': 42, 'key': 'dusk-04', 'skin': 0, 'class': 'dusk', 'color_value': 4, 'primary1': '00aeb7', 'shaded1': '0081a5', 'primary2': 'a4ffcb', 'shaded2': '00dfa6', 'line': '009be5', 'partColorShift': '0000000'},
          {'index': 43, 'key': 'mech-00', 'skin': 0, 'class': 'mech', 'color_value': 0, 'primary1': 'f6fbff', 'shaded1': 'c6cfcf', 'primary2': '929292', 'shaded2': '5f5f5f', 'line': '838383', 'partColorShift': '0000000'},
          {'index': 44, 'key': 'mech-01', 'skin': 0, 'class': 'mech', 'color_value': 1, 'primary1': '505050', 'shaded1': '434343', 'primary2': 'e5e5e5', 'shaded2': 'a6a6a6', 'line': '474747', 'partColorShift': '0000000'},
          {'index': 45, 'key': 'mech-02', 'skin': 0, 'class': 'mech', 'color_value': 2, 'primary1': 'ced5d7', 'shaded1': '97aeb0', 'primary2': 'f1fdf0', 'shaded2': '9bcfca', 'line': '008eb0', 'partColorShift': '0000000'},
          {'index': 46, 'key': 'mech-03', 'skin': 0, 'class': 'mech', 'color_value': 3, 'primary1': 'ad6d3f', 'shaded1': '994b3b', 'primary2': '9ae6cb', 'shaded2': '56a6cd', 'line': 'cb4800', 'partColorShift': '0000000'},
          {'index': 47, 'key': 'mech-04', 'skin': 0, 'class': 'mech', 'color_value': 4, 'primary1': '8c8e9e', 'shaded1': '706a8c', 'primary2': 'ebe7de', 'shaded2': 'bda395', 'line': '595fc2', 'partColorShift': '0000000'},
          {'index': 48, 'key': 'body-frosty', 'skin': 1, 'class': 'any', 'color_value': -1, 'primary1': 'f3fefe', 'shaded1': '6acad7', 'primary2': 'f3fefe', 'shaded2': '6acad7', 'line': '0097d2', 'partColorShift': '0000000'},
          {'index': 49, 'key': 'beast-summer', 'skin': 2, 'class': 'beast', 'color_value': -1, 'primary1': 'fe9701', 'shaded1': 'df5414', 'primary2': 'fff6ec', 'shaded2': 'dec8b7', 'line': 'ff8000', 'partColorShift': '0000011'},
          {'index': 50, 'key': 'plant-summer', 'skin': 2, 'class': 'plant', 'color_value': -1, 'primary1': '67e92f', 'shaded1': '019d74', 'primary2': 'f6fff7', 'shaded2': 'bcd5cf', 'line': '10dd00', 'partColorShift': '0000011'},
          {'index': 51, 'key': 'aquatic-summer', 'skin': 2, 'class': 'aquatic', 'color_value': -1, 'primary1': '00c5d0', 'shaded1': '0081c2', 'primary2': 'f5fffd', 'shaded2': 'c2cedd', 'line': '00d4f0', 'partColorShift': '0000011'},
          {'index': 52, 'key': 'bug-summer', 'skin': 2, 'class': 'bug', 'color_value': -1, 'primary1': 'ff4363', 'shaded1': 'd20539', 'primary2': 'fffdff', 'shaded2': 'e5c1c8', 'line': 'de0000', 'partColorShift': '0000011'},
          {'index': 53, 'key': 'bird-summer', 'skin': 2, 'class': 'bird', 'color_value': -1, 'primary1': 'ff78bb', 'shaded1': 'e63c8b', 'primary2': 'fffdff', 'shaded2': 'e4bfd8', 'line': 'e40086', 'partColorShift': '0000011'},
          {'index': 54, 'key': 'reptile-summer', 'skin': 2, 'class': 'reptile', 'color_value': -1, 'primary1': 'e173ff', 'shaded1': 'a349f9', 'primary2': 'fefeff', 'shaded2': 'd9c1e7', 'line': '8d24ff', 'partColorShift': '0000011'},
          {'index': 55, 'key': 'dawn-summer', 'skin': 2, 'class': 'dawn', 'color_value': -1, 'primary1': 'ffda00', 'shaded1': 'fff527', 'primary2': 'f8fdea', 'shaded2': 'e5e4c6', 'line': 'e66c00', 'partColorShift': '0000011'},
          {'index': 56, 'key': 'mech-summer', 'skin': 2, 'class': 'mech', 'color_value': -1, 'primary1': 'c7d7e9', 'shaded1': 'b0e4ff', 'primary2': 'fefeff', 'shaded2': 'dfe9ec', 'line': '008eb0', 'partColorShift': '0000011'},
          {'index': 57, 'key': 'dusk-summer', 'skin': 2, 'class': 'dusk', 'color_value': -1, 'primary1': '474b84', 'shaded1': '4e50dd', 'primary2': 'fefeff', 'shaded2': 'e4e6f2', 'line': '006e99', 'partColorShift': '0000011'}
        ]";
        JArray jColors = JArray.Parse(colorStr);

        JArray jGenesData = JArray.Parse(System.IO.File.ReadAllText("part_states.json"));
        //string genesStr = "0x28000000000001000200c060c404000000010014204082020001001410a08504000100141020850200010014204144060001001420a085080001001418a0c506";
        string genesStr = "0x18000000000001000240d030100800000001000c084143020001000c08604302000100001061830a0001000c1060c30a0001000c3061830c0001000c08604508";
        if (genesStr.StartsWith("0x"))
        {
            genesStr = genesStr.Substring(2);
        }
        string finalGenes512 = genesStr;
        if (finalGenes512.Length < 128)
        {
            finalGenes512 = finalGenes512.PadLeft(128, '0');
        }
        System.Numerics.BigInteger.TryParse(finalGenes512, System.Globalization.NumberStyles.HexNumber, null, out var genes);
        GetAxieBodyStructure512(genes, jGenesData, jColors);
    }

    static string[] AXIE_PART_TYPES = new string[]
    {
        "eyes", "mouth", "ears", "horn", "back", "tail"
    };

    private static string CharacterClassFromValue(int val)
    {
        switch (val)
        {
            case 0: return "beast";
            case 1: return "bug";
            case 2: return "bird";
            case 3: return "plant";
            case 4: return "aquatic";
            case 5: return "reptile";
            case 16: return "mech";
            case 17: return "dawn";
            case 18: return "dusk";
        }
        return "none";
    }

    public static string GetAxiePrimaryColor(string characterClass, int primaryColor, int bodySkin, JArray jColors)
    {
        int variantIndex = -1;
        for (int i = 0; i < jColors.Count; i++)
        {
            var jColor = jColors[i];
            int colorSkin = (int)jColor["skin"];
            int colorSkinPrimary = (int)jColor["color_value"];
            string colorClass = (string)jColor["class"];
            if (colorSkin != bodySkin) continue;
            if ((colorClass == "none" || colorClass == characterClass) &&
                (colorSkinPrimary == -1 || colorSkinPrimary == primaryColor))
            {
                variantIndex = i;
                break;
            }
        }
        if (variantIndex == -1)
        {
            variantIndex = 0;
        }
        return (string)(jColors[variantIndex]["primary1"]);
    }

    private static void FillZero(ref string val, int total)
    {
        for(int i=0;i< total; i++)
        {
            val += "0";
        }
    }

    private static int CharacterClassToValue(string className)
    {
        switch (className)
        {
            case "beast": return 0;
            case "bug": return 1 ;
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


    //[UnityEditor.MenuItem("Test/Fake Genes")]
    //static void TestFakeGenes()
    //{
    //    JArray jGenesData = JArray.Parse(System.IO.File.ReadAllText("part_states.json"));

    //    //Dictionary<string, string> adultCombo = new Dictionary<string, string>();
    //    //adultCombo["body-class"] = "beast";
    //    var adultCombo = new Dictionary<string, string> {
    //                {"back", "back-ronin" },
    //                {"ears", "beast-ears-06.1" },
    //                {"eyes", "eyes-calico-zeal" },
    //                {"horn", "horn-pliers" },
    //                {"mouth", "mouth-kawaii" },
    //                {"tail", "tail-pupae" },
    //                {"body-class", "beast" },
    //            };
    //    string genes = FakeAxie(adultCombo, jGenesData);
    //    var hex = string.Join("",
    //        Enumerable.Range(0, genes.Length / 8)
    //        .Select(i => Convert.ToByte(genes.Substring(i * 8, 8), 2).ToString("X2")));
    //    //string strHex = Convert.ToInt32(genes, 2).ToString("X");
    //    Debug.Log(hex);
    //}
    //public static string FakeAxie(Dictionary<string, string> adultCombo, JArray jGenesData)
    //{
    //    var genes = "";
    //    int mainClass = CharacterClassToValue(adultCombo["body-class"]);

    //    genes += Convert.ToString(mainClass, 2).PadLeft(5, '0');
    //    genes += Convert.ToString(0, 2).PadLeft(45, '0'); //reservation
    //    genes += Convert.ToString(0, 2).PadLeft(5, '0'); //contribution
    //    genes += Convert.ToString(0, 2).PadLeft(1, '0'); //bodySkinInheritability
    //    genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodySkin
    //    genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodyDetail0
    //    genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodyDetail1
    //    genes += Convert.ToString(0, 2).PadLeft(9, '0'); //bodyDetail2

    //    genes += Convert.ToString(0, 2).PadLeft(6, '0'); //primaryColor0
    //    genes += Convert.ToString(0, 2).PadLeft(6, '0'); //primaryColor1
    //    genes += Convert.ToString(0, 2).PadLeft(6, '0'); //primaryColor2

    //    genes += Convert.ToString(0, 2).PadLeft(6, '0'); //secondaryColor0
    //    genes += Convert.ToString(0, 2).PadLeft(6, '0'); //secondaryColor1
    //    genes += Convert.ToString(0, 2).PadLeft(6, '0'); //secondaryColor2

    //    for (int partIndex = 0; partIndex < 6; partIndex++)
    //    {
    //        var partType = AXIE_PART_TYPES[partIndex];

    //        string partId = adultCombo[partType];
    //        JObject jPart = null;
    //        for (int i = 0; i < jGenesData.Count; i++)
    //        {
    //            if ((string)jGenesData[i]["part_id"] == partId)
    //            {
    //                jPart = jGenesData[i] as JObject;
    //            }
    //        }

    //        genes += Convert.ToString(0, 2).PadLeft(2, '0'); //partStage
    //        genes += Convert.ToString(0, 2).PadLeft(13, '0'); //partReservation
    //        genes += Convert.ToString(0, 2).PadLeft(1, '0'); //partSkinInheritability
    //        genes += Convert.ToString((int)jPart["part_skin"], 2).PadLeft(9, '0'); //partSkin

    //        genes += Convert.ToString(CharacterClassToValue((string)jPart["class"]), 2).PadLeft(5, '0');
    //        genes += Convert.ToString((int)jPart["part_value"], 2).PadLeft(8, '0');

    //        genes += Convert.ToString(CharacterClassToValue((string)jPart["class"]), 2).PadLeft(5, '0');
    //        genes += Convert.ToString((int)jPart["part_value"], 2).PadLeft(8, '0');

    //        genes += Convert.ToString(CharacterClassToValue((string)jPart["class"]), 2).PadLeft(5, '0');
    //        genes += Convert.ToString((int)jPart["part_value"], 2).PadLeft(8, '0');
 
    //    }
    
    //    return genes;
    //}
    public static void GetAxieBodyStructure512(BigInteger genes, JArray jGenesData, JArray jColors)
    {
        var bits = new BitExtractor(genes, 512);

        int mainClass = bits.extract(5);
        int reservation = bits.extract(45);
        int contribution = bits.extract(5);

        int bodySkinInheritability = bits.extract(1);
        int bodySkin = bits.extract(9);
        int bodyDetail0 = bits.extract(9);
        int bodyDetail1 = bits.extract(9);
        int bodyDetail2 = bits.extract(9);

        int primaryColor0 = bits.extract(6);
        int primaryColor1 = bits.extract(6);
        int primaryColor2 = bits.extract(6);

        int secondaryColor0 = bits.extract(6);
        int secondaryColor1 = bits.extract(6);
        int secondaryColor2 = bits.extract(6);

        string primaryColor = GetAxiePrimaryColor(CharacterClassFromValue(mainClass), primaryColor0, bodySkin, jColors);
        Debug.Log($"primaryColor: {primaryColor}");
        //var bodyStructure = new AxieBodyStructure
        //{
        //    @class = CharacterClassFromValue(mainClass), //todo: check casting
        //    body = new List<int>() { bodyDetail0, bodyDetail1, bodyDetail2 },
        //    bodySkin = bodySkin,
        //    primaryColors = new List<int>() { primaryColor0, primaryColor1, primaryColor2 },
        //    parts = new Dictionary<AxiePartType, AxiePartStructure>(),
        //    secondaryColors = new List<int>() { secondaryColor0, secondaryColor1, secondaryColor2 }
        //};

        for (int partIndex = 0; partIndex < 6; partIndex++)
        {
            int partStage = bits.extract(2);
            int partReservation = bits.extract(13);
            int partSkinInheritability = bits.extract(1);
            int partSkin = bits.extract(9);

            int partClass0 = bits.extract(5);
            int partValue0 = bits.extract(8);

            int partClass1 = bits.extract(5);
            int partValue1 = bits.extract(8);

            int partClass2 = bits.extract(5);
            int partValue2 = bits.extract(8);


            var partType = AXIE_PART_TYPES[partIndex];
            var partClass = CharacterClassFromValue(partClass0);

            string partId = null;
            for (int i = 0; i < jGenesData.Count; i++)
            {
                JObject jPart = jGenesData[i] as JObject;
                if((string)jPart["class"] == partClass &&
                    (string)jPart["part_type"] == partType &&
                    (int)jPart["part_value"] == partValue0 &&
                    (int)jPart["part_skin"] == partSkin)
                {
                    partId = (string)jPart["part_id"];
                }
            }
            Debug.Log(partId);
            //var part = new AxiePartStructure
            //{
            //    stageCap = 2,
            //    stage = partStage,
            //    reservation = partReservation,
            //    skinInheritAbility = partSkinInheritability == 0 ? false : true,
            //    skin = partSkin,
            //    groups = new List<AxiePartGroupStructure>()
            //};

            //part.groups.Add(new AxiePartGroupStructure { @class = CharacterClassFromValue(partClass0), value = partValue0, });
            //part.groups.Add(new AxiePartGroupStructure { @class = CharacterClassFromValue(partClass1), value = partValue1, });
            //part.groups.Add(new AxiePartGroupStructure { @class = CharacterClassFromValue(partClass2), value = partValue2, });
            //bodyStructure.parts.Add(partType, part);
        }

        //return bodyStructure;
    }
}
