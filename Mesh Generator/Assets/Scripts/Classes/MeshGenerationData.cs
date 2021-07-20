using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerationData
{
    public int dimension = 3;
    public Enums.HeightMapTypes mapType = Enums.HeightMapTypes.Plane;
    // remap values
    public float remapMin = 0;
    public float remapMax = 1;

    public bool invert = false;
    
    // perlin noise values
    public PerlinMetaData perlin = new PerlinMetaData();

    // simple noise values
    public SimpleNoiseMetaData simpleNoise = new SimpleNoiseMetaData();
    

    public override string ToString()
    {
        return Enums.HeightMapTypeNames[mapType] + 
               "\n" + dimension + "x" + dimension +
               "\nPERLIN: (" + perlin.sampleMin + ", " + perlin.sampleMax + ")" +
               "\nREMAP: (" + remapMin + ", " + remapMax + ")";
    }
}
