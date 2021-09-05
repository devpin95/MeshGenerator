using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parameters;
using UnityEngine;

public static class Putils
{
    public static float flatNormalThreshold = 0.00001f;
    
    public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax) {
        // if ( from > fromMax || from < fromMin ) Debug.LogWarning("Value is outside initial range");
        
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
        return norm.y >= 1 - flatNormalThreshold && 
               norm.y <= 1 + flatNormalThreshold;
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

    public static void RemapVectorHeightList(List<Vector3> list, float min, float max, float tomin, float tomax)
    {

        for (int i = 0; i < list.Count; ++i)
        {
            Vector3 v = list[i]; 
            float h = Remap(list[i].y, min, max, 0f, 1f);
            v.y = h;
            list[i] = v;
        }
    }

    public static bool ValueWithinRange(float val, float limit, float threshold)
    {
        return val >= limit - threshold && val <= limit + threshold;
    }

    public static string DateTimeString()
    {
        return System.DateTime.Now.ToString("ddMMyyyyhhmmss");
    }

    public static string MsToTimeString(float time)
    {
        string stime = "peepee poopoo";

        if (time <= 60000)
        {
            stime = time.ToString("n2") + "ms";
        }
        else if (time < 60000 * 60)
        {
            float minutes = time / (1000f * 60);
            stime = minutes.ToString("n2") + "min";
        }

        return stime;
    }
}
