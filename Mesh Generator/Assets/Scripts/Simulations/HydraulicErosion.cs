using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEditorInternal;
using UnityEngine;

public static class HydraulicErosion
{
   
    public static IEnumerator Simulate(HeightMap map, HydraulicErosionParameters parameters, CEvent_String checkpointNotification)
    {
        
        int mapdim = map.WidthAndHeight();

        for (int drop = 0; drop < parameters.DropCount; ++drop)
        {
            // checkpointNotification.Raise("Simulating drop " + drop);
            // Debug.Log("Drop " + drop);
            
            checkpointNotification.Raise("Simulating drop " + drop + " of " + parameters.DropCount);
            yield return null;

            float x = Random.Range(0f, mapdim);
            float y = Random.Range(0f, mapdim);
            
            float xoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the x direction
            float yoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the y direction
            float sediment = 0; // how much sediment the current drop is holding
            float xpos = x; // the current x pos
            float ypos = y; // the current y pos
            float xvelocity = 0; // drop velocity in the x direction
            float yvelocity = 0; // drop velocity in the y direction

            // do the trace for MaxInterations
            for (int i = 0; i < parameters.MaxIterations; ++i)
            {
                // checkpointNotification.Raise("x: " + (xpos + xoffset) + " y: " + (ypos + yoffset));
                // yield return new WaitForSeconds(1f);
                
                // make sure the sample is still in bounds
                if (map.SampleOutOfBounds(xpos + xoffset, ypos + yoffset)) break;
                
                // checkpointNotification.Raise("In Bounds");
                // yield return new WaitForSeconds(1f);
                
                // get the normal at the random position
                Vector3 norm = map.SampleNormalAtXY(xpos + xoffset, ypos + yoffset);
                
                // checkpointNotification.Raise("Normal " + norm);
                // yield return new WaitForSeconds(1f);
                
                if (Putils.normIsUp(norm)) break; // we are either straight up or off the map, so we need to stop
                
                // checkpointNotification.Raise("Normal is not too flat" );
                // yield return new WaitForSeconds(1f);

                float deposit = sediment * parameters.DepositeRate * norm.y;
                float erosion = parameters.ErosionRate * (1 - norm.y) * Mathf.Min(1, i * parameters.IterationScale);

                map.ChangeCell(xpos, ypos, deposit - erosion);

                xvelocity = parameters.Friction * xvelocity + norm.x * parameters.Speed * 10;
                yvelocity = parameters.Friction * yvelocity + norm.z * parameters.Speed * 10;
                
                // checkpointNotification.Raise("X Velocity: " + xvelocity + " Y Velocity: " + yvelocity);
                // yield return new WaitForSeconds(1f);
                
                if (xvelocity == 0 && yvelocity == 0) break;
                //
                // checkpointNotification.Raise("Velocity is not too low");
                // yield return new WaitForSeconds(1f);
                
                xpos = x;
                ypos = y;
                x += xvelocity;
                y += yvelocity;

                sediment += erosion - deposit;
                
                // checkpointNotification.Raise("Sediment: " + sediment + " Erosion: " + erosion + " Deposition: " + deposite);
                // yield return new WaitForSeconds(1f);

            }
            
            yield return null;
        }

        yield break;
    }

    // private static void Iterate(HeightMap map, HydraulicErosionParameters parameters)
    // {
    //     int mapdim = map.WidthAndHeight();
    //     int drops = parameters.DropCount * mapdim * mapdim;
    //
    //     for (int i = 0; i < drops; ++i)
    //     {
    //         Debug.Log("Drop " + i);
    //         float randx = Random.Range(0, mapdim);
    //         float randy = Random.Range(0, mapdim);
    //         Trace(randx, randy, map, parameters);
    //     }
    // }
    //
    // private static void Trace(float x, float y, HeightMap map, HydraulicErosionParameters parameters)
    // {
    //     float xoffset = Random.Range(-1, 1) * parameters.Radius; // get a random offset in the x direction
    //     float yoffset = Random.Range(-1, 1) * parameters.Radius; // get a random offset in the y direction
    //     float sediment = 0; // how much sediment the current drop is holding
    //     float xpos = x; // the current x pos
    //     float ypos = y; // the current y pos
    //     float xvelocity = 0; // drop velocity in the x direction
    //     float yvelocity = 0; // drop velocity in the y direction
    //
    //     // do the trace for MaxInterations
    //     for (int i = 0; i < parameters.MaxIterations; ++i)
    //     {
    //         // get the normal at the random position
    //         Vector3 norm = map.SampleNormalAtXY(xpos + xoffset, ypos + yoffset);
    //
    //         if (Putils.normIsUp(norm)) break; // we are either straight up or off the map, so we need to stop
    //
    //         float deposite = sediment * parameters.DepositeRate * norm.y;
    //         float erosion = parameters.ErosionRate * (1 - norm.y) * Mathf.Min(1, i * parameters.IterationScale);
    //
    //         map.ChangeCell(xpos, ypos, deposite - erosion);
    //
    //         xvelocity = parameters.Friction * xvelocity * norm.x * parameters.Speed;
    //         yvelocity = parameters.Friction * yvelocity * norm.z * parameters.Speed;
    //
    //         x += xvelocity;
    //         y += yvelocity;
    //         xpos = x;
    //         ypos = y;
    //
    //         sediment += erosion - deposite;
    //     }
    // }
}
