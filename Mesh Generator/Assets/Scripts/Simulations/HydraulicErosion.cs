using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEditorInternal;
using UnityEngine;

public static class HydraulicErosion
{
    
    public class HydraulicParticle
    {
        public Vector2 pos;
        public Vector2 speed = Vector2.zero;
        public float volume = Constants.hydraulicParticleInitialVolume;
        public float sediment = 0;
    }
   
    public static IEnumerator Simulate(HeightMap map, HydraulicErosionParameters parameters, CEvent_String checkpointNotification)
    {
        
        int mapdim = map.WidthAndHeight();

        for (int drop = 0; drop < parameters.DropCount; ++drop)
        {
            checkpointNotification.Raise("Simulating drop " + drop + " of " + parameters.DropCount);
            yield return null;
            
            HydraulicParticle particle = new HydraulicParticle();

            float x = Random.Range(0f, mapdim);
            float y = Random.Range(0f, mapdim);
            
            float xoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the x direction
            float yoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the y direction
            float sediment = 0; // how much sediment the current drop is holding
            // float xpos = x; // the current x pos
            // float ypos = y; // the current y pos

            particle.pos.x = x + xoffset;
            particle.pos.y = y + yoffset;
            particle.speed.x = 0;
            particle.speed.y = 0;
            particle.sediment = 0;

            // do the trace for MaxInterations
            for (int i = 0; i < parameters.MaxIterations; ++i)
            {

                // make sure the sample is still in bounds
                if (map.SampleOutOfBounds(particle.pos.x, particle.pos.y)) break;
                
                
                // get the normal at the random position
                Vector3 norm = map.SampleNormalAtXY((int)particle.pos.y, (int)particle.pos.x);
                
                
                if (Putils.normIsUp(norm)) break; // we are either straight up or off the map, so we need to stop
                

                float deposit = sediment * parameters.DepositeRate * norm.y;
                float erosion = parameters.ErosionRate * (1 - norm.y) * Mathf.Min(1, i * parameters.IterationScale);

                map.ChangeCell((int)particle.pos.y, (int)particle.pos.x, deposit - erosion);

                particle.speed.x = parameters.Friction * particle.speed.x + norm.x * parameters.Speed * 10;
                particle.speed.y = parameters.Friction * particle.speed.y + norm.z * parameters.Speed * 10;
                
                
                if (particle.speed.x == 0 && particle.speed.y == 0) break;

                particle.pos.x = x;
                particle.pos.y = y;
                x += particle.speed.x;
                y += particle.speed.y;

                sediment += erosion - deposit;
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
