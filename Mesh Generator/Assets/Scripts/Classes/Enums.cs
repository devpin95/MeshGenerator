using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums
{
    // Height Map Types ------------------------------------------------------------------------------------------------
    public enum HeightMapTypes
    {
        Plane,
        SimpleNoise,
        PerlinNoise,
        PerlinOctaves,
        ImageMap
    }
    
    public static Dictionary<HeightMapTypes, string> HeightMapTypeNames = new Dictionary<HeightMapTypes, string>()
    {
        {HeightMapTypes.Plane, "Plane"},
        {HeightMapTypes.SimpleNoise, "Simple Noise"},
        {HeightMapTypes.PerlinNoise, "Perlin Noise"},
        {HeightMapTypes.PerlinOctaves, "Octaves (Perlin)"},
        {HeightMapTypes.ImageMap, "Image Map"}
    };
    
    // -----------------------------------------------------------------------------------------------------------------

    // Smoothing Algorithms --------------------------------------------------------------------------------------------
    public enum SmoothingAlgorithms
    {
        None,
        GaussianBlur,
        Smoothstep,
        PerlinSmoothstep,
        Cosine
    }
    
    public static Dictionary<SmoothingAlgorithms, string> SmoothingAlgorithmNames =
        new Dictionary<SmoothingAlgorithms, string>()
        {
            {SmoothingAlgorithms.None, "None"},
            {SmoothingAlgorithms.GaussianBlur, "Gaussian Blur"},
            {SmoothingAlgorithms.Smoothstep, "Smoothstep"},
            {SmoothingAlgorithms.PerlinSmoothstep, "Perlin Smoothstep"},
            {SmoothingAlgorithms.Cosine, "Cosine"}
        };
    
    // -----------------------------------------------------------------------------------------------------------------

    // Erosion Algorithms ----------------------------------------------------------------------------------------------
    public enum ErosionAlgorithms
    {
        Hydraulic
    }
    
    public static Dictionary<ErosionAlgorithms, string> ErosionSimulationNames =
        new Dictionary<ErosionAlgorithms, string>()
        {
            {ErosionAlgorithms.Hydraulic, "Hydraulic Erosion"}
        };
    
    // -----------------------------------------------------------------------------------------------------------------
    
    // Gaussian Border Modes -------------------------------------------------------------------------------------------
    public enum GaussianBlurBorderModes
    {
        Ignore,
        BlendWhite,
        BlendBlack,
        Mirror,
        Nearest
    }
    
    public static Dictionary<GaussianBlurBorderModes, string> GaussianBlurBorderModeNames =
        new Dictionary<GaussianBlurBorderModes, string>()
        {
            {GaussianBlurBorderModes.Ignore, "Ignore"},
            {GaussianBlurBorderModes.BlendWhite, "Blend White"},
            {GaussianBlurBorderModes.BlendBlack, "Blend Black"},
            {GaussianBlurBorderModes.Mirror, "Mirror"},
            {GaussianBlurBorderModes.Nearest, "Nearest"}
        };
    
    // -----------------------------------------------------------------------------------------------------------------

    // Operations ------------------------------------------------------------------------------------------------------
    public enum OperationTypes
    {
        Stretch,
        GaussianBlur
    }
    
    public static Dictionary<OperationTypes, string> OperationTypeNames = new Dictionary<OperationTypes, string>()
    {
        {OperationTypes.Stretch, "Stretch Heights"},
        {OperationTypes.GaussianBlur, "Gaussian Blur"}
    };
    // -----------------------------------------------------------------------------------------------------------------
}
