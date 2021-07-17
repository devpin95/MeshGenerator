using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerationData
{
    public int dimension = 3;
    public MeshGenerator.HeightMapTypes mapType;
    
    // perlin noise values
    public float perlinNoiseSampleMin = 0;
    public float perlinNoiseSampleMax = 1;
    public bool domainWarp = false;
    public float hurst = 0.5f;
    public int octaves = 2;
    
    // remap values
    public float remapMin = 0;
    public float remapMax = 1;

    public override string ToString()
    {
        return MeshGenerator.HeightMapTypeNames[mapType] + 
               "\n" + dimension + "x" + dimension +
               "\nPERLIN: (" + perlinNoiseSampleMin + ", " + perlinNoiseSampleMax + ")" +
               "\nREMAP: (" + remapMin + ", " + remapMax + ")";
    }
}
