using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StretchMetaData
{
    private bool remap = false;
    private float remapMin = 0f;
    private float remapMax = 1f;

    public bool Remap
    {
        get => remap;
        set => remap = value;
    }

    public float RemapMin
    {
        get => remapMin;
        set => remapMin = value;
    }

    public float RemapMax
    {
        get => remapMax;
        set => remapMax = value;
    }
}
