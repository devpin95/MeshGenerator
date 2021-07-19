using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNoiseMetaData
{
    public int latticeDim = 2;
    public float sampleMin = 0;
    public float sampleMax = 1;
    public Enums.SmoothingAlgorithms smoothing = Enums.SmoothingAlgorithms.None;
    public int frequency = 1;
    public float scale = 1.0f;
    public bool random = true;
    public int seed = 2046;
}
