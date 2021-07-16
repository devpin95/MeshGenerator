using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerationData
{
    public int dimension;
    public MeshGenerator.HeightMapTypes mapType;
    
    // perlin noise values
    public float perlinNoiseSampleMin;
    public float perlinNoiseSampleMax;
    public bool domainWarp = false;
    public float hurst;
    public int octaves;
    
    // remap values
    public float remapMin;
    public float remapMax;

    public override string ToString()
    {
        return MeshGenerator.HeightMapTypeNames[mapType] + 
               "\n" + dimension + "x" + dimension +
               "\nPERLIN: (" + perlinNoiseSampleMin + ", " + perlinNoiseSampleMax + ")" +
               "\nREMAP: (" + remapMin + ", " + remapMax + ")";
    }
}
