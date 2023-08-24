using AxieCore.AxieMixer;
using Spine.Unity;
using UnityEngine;

namespace AxieMixer.Unity
{
    public enum AxieFormType
    {
        Normal,
        //Isometric,

        Count
    }
    public interface IAxieMixerMaterials
    {
        SpineAtlasAsset GetFullSplatAtlasAsset(AxieFormType formType);
        SpineAtlasAsset GetSingleSplatAtlasAsset(AxieFormType formType);
        IAxieGenesStuff GetGenesStuff(AxieFormType formType);
        IAxieMixerStuff GetMixerStuff(AxieFormType formType);
        Material GetSampleGraphicMaterial(AxieFormType formType);
    }
}
