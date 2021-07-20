using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoothing
{
    public static float None(float x)
    {
        return x;
    }
    
    public static float Cosine(float x)
    {
        if (x <= 0) return 0;
        if (x >= 1) return 1;

        float xremap = (1 - Mathf.Cos(x * Mathf.PI)) * 0.5f;
        
        return xremap;
    }

    public static float Smoothstep(float x)
    {
        if (x <= 0) return 0;
        if (x >= 1) return 1;

        // 3x^2 - 2x^3
        float xremap = x * x * (3 - 2 * x);
        
        return x;
    }

    public static float PerlinSmoothstep(float x)
    {
        if (x <= 0) return 0;
        if (x >= 1) return 1;
        
        // 6x^5 -15x^4 + 10x^3
        float xremap = x * x * x * (x * (6 * x - 15) + 10);

        return xremap;
    }

    public static float GaussianBlur(float x)
    {
        return x;
    }

    public static Dictionary<Enums.SmoothingAlgorithms, Func<float, float>> Algorithms =
        new Dictionary<Enums.SmoothingAlgorithms, Func<float, float>>()
        {
            {Enums.SmoothingAlgorithms.None, None},
            {Enums.SmoothingAlgorithms.Cosine, Cosine},
            {Enums.SmoothingAlgorithms.Smoothstep, Smoothstep},
            {Enums.SmoothingAlgorithms.PerlinSmoothstep, PerlinSmoothstep},
            {Enums.SmoothingAlgorithms.GaussianBlur, GaussianBlur}
        };
}
