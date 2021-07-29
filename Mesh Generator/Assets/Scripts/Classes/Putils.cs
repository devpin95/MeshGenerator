using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEngine;

public static class Putils
{
    public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax) {
        var fromAbs  =  from - fromMin;
        var fromMaxAbs = fromMax - fromMin;      
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }

    public static Sprite Tex2dToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
    
    public static bool normIsUp(Vector3 norm)
    {
        return norm.y >= 1 - HydraulicErosionParameters.flatThreshold && 
               norm.y <= 1 + HydraulicErosionParameters.flatThreshold;
    }
}
