using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static int meshSquares = 254;
    public static int meshVerts = meshSquares + 1;
    
    // debug
    public static bool db_DebugMode = false;
    public static int db_DropRecordLength = 20;

    public static float maxDropSpeed = 2f;
    public static int maxDropIterations = 80;
    public static int maxSpeedThreshold = 2;
}
