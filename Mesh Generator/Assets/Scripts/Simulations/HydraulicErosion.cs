using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEditorInternal;
using UnityEngine;

public static class HydraulicErosion
{
    private const float HydraulicParticleInitialVolume = 1f;
    private const float HydraulicParticleMinVolume = 0.1f;
    private const float HydraulicParticleMaxSediment = 1f;
    private const float HydraulicParticleDensity = 1f;
    private const float HydraulicParticleDepositionRate = 0.05f;
    private const float HydraulicParticleEvaporationRate = 0.001f;
    private const float HydraulicParticleFriction = 0.05f;
    private const float DT = 1.2f;
    
    public class HydraulicParticle
    {
        public HydraulicParticle(Vector2 npos) {pos = npos;}
        
        public Vector2 pos = Vector2.zero;
        public Vector2 speed = Vector2.zero;
        public float volume = HydraulicParticleInitialVolume;
        public float sediment = 0;
    }
    
   
    public static IEnumerator Simulate(HeightMap map, HydraulicErosionParameters parameters, CEvent_String checkpointNotification)
    {
        // https://github.com/weigert/SimpleErosion/blob/master/source/include/world/world.cpp
        
        int mapdim = map.WidthAndHeight();

        for (int drops = 0; drops < parameters.DropCount; ++drops)
        {
            if (drops % 2000 == 0)
            {
                checkpointNotification.Raise("Simulating drop " + drops + " of " + parameters.DropCount);
                yield return null;
            }

            HydraulicParticle drop = new HydraulicParticle(Vector2.zero);

            float x = Random.Range(0f, mapdim);
            float y = Random.Range(0f, mapdim);
            
            float xoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the x direction
            float yoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the y direction
            // float sediment = 0; // how much sediment the current drop is holding
            // float xpos = x; // the current x pos
            // float ypos = y; // the current y pos

            drop.pos.x = x + xoffset;
            drop.pos.y = y + yoffset;
            drop.speed.x = 0;
            drop.speed.y = 0;
            drop.sediment = 0;

            while (drop.volume > HydraulicParticleMinVolume)
            {
                // make sure the drop is still in the bounds of the mesh
                if (map.SampleOutOfBounds(drop.pos.x, drop.pos.y)) break;

                Vector2 initialPos = drop.pos;
                Vector3 norm = map.SampleBetaNormalAtXY((int)initialPos.y, (int)initialPos.x);

                Vector2 F = DT * new Vector2(norm.x, norm.z); // force
                float m = (drop.volume * HydraulicParticleDensity); // mass

                // a = F/m
                drop.speed += F / m;

                // update pos
                drop.pos += DT * drop.speed;

                if (map.SampleOutOfBounds(drop.pos.x, drop.pos.y)) break;

                // apply friction
                drop.speed *= 1f - (DT * HydraulicParticleFriction);

                // figure out sediment levels
                float prevY = map.SampleMapAtXY((int) initialPos.y, (int) initialPos.x);
                float curY = map.SampleMapAtXY((int) drop.pos.y, (int) drop.pos.x);
                float travelDistance = drop.speed.magnitude * (prevY - curY);
                float maxsed = drop.volume * travelDistance;

                if (maxsed < 0) maxsed = 0f;
                float seddiff = maxsed - drop.sediment;

                // change the map based on the sediment
                drop.sediment += DT * HydraulicParticleDepositionRate * seddiff;

                float amount = DT * drop.volume * HydraulicParticleDepositionRate * seddiff;

                map.ChangeCell((int)initialPos.y, (int)initialPos.x, amount);
                
                // do some friction
                drop.volume *= 1f - (DT * HydraulicParticleEvaporationRate);
            }

            // do the trace for MaxInterations
            // for (int i = 0; i < parameters.MaxIterations; ++i)
            // {
            //
            //     // make sure the sample is still in bounds
            //     if (map.SampleOutOfBounds(particle.pos.x, particle.pos.y)) break;
            //     
            //     
            //     // get the normal at the random position
            //     Vector3 norm = map.SampleNormalAtXY((int)particle.pos.y, (int)particle.pos.x);
            //     
            //     
            //     if (Putils.normIsUp(norm)) break; // we are either straight up or off the map, so we need to stop
            //     
            //
            //     float deposit = sediment * parameters.DepositeRate * norm.y;
            //     float erosion = parameters.ErosionRate * (1 - norm.y) * Mathf.Min(1, i * parameters.IterationScale);
            //
            //     map.ChangeCell((int)particle.pos.y, (int)particle.pos.x, deposit - erosion);
            //
            //     particle.speed.x = parameters.Friction * particle.speed.x + norm.x * parameters.Speed * 10;
            //     particle.speed.y = parameters.Friction * particle.speed.y + norm.z * parameters.Speed * 10;
            //     
            //     
            //     if (particle.speed.x == 0 && particle.speed.y == 0) break;
            //
            //     particle.pos.x = x;
            //     particle.pos.y = y;
            //     x += particle.speed.x;
            //     y += particle.speed.y;
            //
            //     sediment += erosion - deposit;
            // }
            
            // yield return null;
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
