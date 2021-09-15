using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinMetaData
{
    public float sampleMin = 0;
    public float sampleMax = 1;
    public bool domainWarp = false;
    public bool ridged = false;
    public float domainFactorX = 80f;
    public float domainFactorY = 80f;
    public int octaves = 2;
}
