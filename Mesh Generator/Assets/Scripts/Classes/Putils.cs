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
    
    public static T[,] FlatArrayToTwoDArray<T>(T[] flat, int width, int height)
    {
        var grid = new T[width, height];

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int index = x + y * width;
                grid[y, x] = flat[index];
            }
        }

        return grid;
    }

    public static T[] Flatten2DArray<T>(T[,] grid, int width, int height)
    {
        List<T> flatlist = new List<T>();
        
        for ( int y = 0; y < height; ++y )
        {
            for (int x = 0; x < width; ++x)
            {
                flatlist.Add(grid[y, x]);
            }
        }

        return flatlist.ToArray();
    }

    public static Vector3 CalculateTriangleCentroid(Vector3 a, Vector3 b, Vector3 c)
    {
        float x = (a.x + b.x + c.x) / 3f;
        float y = (a.y + b.y + c.y) / 3f;
        float z = (a.z + b.z + c.z) / 3f;

        return new Vector3(x, y, z);
    }

    public static Vector3 CalculateTriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 n1 = b - a;
        Vector3 n2 = c - a;
        return Vector3.Cross(n1, n2).normalized;
    }
}
