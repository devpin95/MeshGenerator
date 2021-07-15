using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LayerData
{
    public enum QueryType
    {
        Add,
        Edit,
        Remove
    }
    
    public string layerName = "";
    public QueryType queryType = QueryType.Add;
    public Texture2D map;
    public float remapMin = 0;
    public float remapMax = 1;
}   
