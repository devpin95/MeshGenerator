using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums
{
    public enum HeightMapTypes
    {
        Plane,
        SimpleNoise,
        PerlinNoise,
        ImageMap
    }

    public enum SmoothingAlgorithms
    {
        None,
        GaussianBlur,
        Smoothstep,
        PerlinSmoothstep,
        Cosine
    }

    public static Dictionary<HeightMapTypes, string> HeightMapTypeNames = new Dictionary<HeightMapTypes, string>()
    {
        {HeightMapTypes.Plane, "Plane"},
        {HeightMapTypes.SimpleNoise, "Simple Noise"},
        {HeightMapTypes.PerlinNoise, "Perlin Noise"},
        {HeightMapTypes.ImageMap, "Image Map"}
    };
    
    public static Dictionary<string, HeightMapTypes> HeightMapNameTypes = new Dictionary<string, HeightMapTypes>()
    {
        {"Plane", HeightMapTypes.Plane},
        {"Simple Noise", HeightMapTypes.SimpleNoise},
        {"Perlin Noise", HeightMapTypes.PerlinNoise},
        {"Image Map", HeightMapTypes.ImageMap}
    };

    public static Dictionary<SmoothingAlgorithms, string> SmoothingAlgorithmNames =
        new Dictionary<SmoothingAlgorithms, string>()
        {
            {SmoothingAlgorithms.None, "None"},
            {SmoothingAlgorithms.GaussianBlur, "Gaussian Blur"},
            {SmoothingAlgorithms.Smoothstep, "Smoothstep"},
            {SmoothingAlgorithms.PerlinSmoothstep, "Perlin Smoothstep"},
            {SmoothingAlgorithms.Cosine, "Cosine"}
        };

    public static Dictionary<string, SmoothingAlgorithms> SmoothingAlgorithmTypes =
        new Dictionary<string, SmoothingAlgorithms>()
        {
            {"None", SmoothingAlgorithms.None},
            {"Gaussian Blur", SmoothingAlgorithms.GaussianBlur},
            {"Smoothstep", SmoothingAlgorithms.Smoothstep},
            {"Perlin Smoothstep", SmoothingAlgorithms.PerlinSmoothstep},
            {"Cosine", SmoothingAlgorithms.Cosine}
        };
}