using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapStretch
{
    public static (float[,], float, float) Stretch(float[,] map, int mapedge, float mapmin, float mapmax)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        
        for (int row = 0; row < mapedge; ++row)
        {
            for (int col = 0; col < mapedge; ++col)
            {
                map[row, col] = Putils.Remap(map[row, col], mapmin, mapmax, 0, 1);

                if (map[row, col] < min) min = map[row, col];
                if (map[row, col] > max) max = map[row, col];
            }
        }
        
        return (map, min, max);
    }

    public static float[,] RemapHeights(float[,] map, int mapedge, float mapMinVal, float mapMaxVal, float newRangeMin, float newRangeMax)
    {
        for (int row = 0; row < mapedge; ++row)
        {
            for (int col = 0; col < mapedge; ++col)
            {
                float newh = Putils.Remap(map[row, col], mapMinVal, mapMaxVal, newRangeMin, newRangeMax);
                map[row, col] = Putils.Remap(newh, newRangeMin, newRangeMax, 0, 1);
            }
        }
        
        return map;
    }
}
