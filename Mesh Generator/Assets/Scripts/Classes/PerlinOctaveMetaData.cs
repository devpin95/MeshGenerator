using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinOctaveMetaData
{
    public float sampleMin = 0;
    public float sampleMax = 1;
    public float max = 1;
    public float[] amplitudes = {1, 0.5f, 0.25f, 0.125f, 0.0625f};
    public float[] frequencies = {1, 0.5f, 0.25f, 0.125f, 0.0625f};
}
