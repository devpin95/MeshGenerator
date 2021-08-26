using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMetaData
{
    public int vertexCount = 0;
    public int polyCount = 0;
    public float generationTimeMS = 0f;
    public Sprite heightMap;
    public bool previousMeshAvailable = false;

    public float mapRangeMin = 0;
    public float mapRangeMax = 1;
    public float mapMinVal = 0;
    public float mapMaxVal = 1;
}
