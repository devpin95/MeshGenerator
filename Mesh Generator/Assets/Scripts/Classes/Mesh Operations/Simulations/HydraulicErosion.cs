using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Parameters;
using UnityEngine;
using Random = UnityEngine.Random;

public static class HydraulicErosion
{
    private const float HydraulicParticleInitialVolume = 1f;

    public class HydraulicParticle
    {
        public HydraulicParticle(Vector2 npos) {pos = npos;}
        
        public Vector2 pos = Vector2.zero;
        public Vector2 speed = Vector2.zero;
        public float volume = HydraulicParticleInitialVolume;
        public float sediment = 0;
    }

    public class SimKeyFrame
    {
        public float x;
        public float y;
        public float volume;
        public float sediment;
        public float speed;
        public float angle;
    }
    
   
    public static IEnumerator Simulate(HeightMap map, HydraulicErosionParameters parameters, CEvent_String checkpointNotification)
    {
        // https://github.com/weigert/SimpleErosion/blob/master/source/include/world/world.cpp
        
        int mapdim = map.WidthAndHeight();
        
        Debug.Log(parameters.FlipXY);

        for (int drops = 0; drops < parameters.DropCount; ++drops)
        {
            if (drops % 2000 == 0)
            {
                checkpointNotification.Raise("Simulating drop " + drops.ToString("n0") + " of " + parameters.DropCount.ToString("n0"));
                yield return null;
            }

            List<SimKeyFrame> frames = new List<SimKeyFrame>();

            HydraulicParticle drop = new HydraulicParticle(Vector2.zero);

            float x = Random.Range(0f, mapdim);
            float y = Random.Range(0f, mapdim);
            
            // float xoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the x direction
            // float yoffset = Random.Range(-1f, 1f) * parameters.Radius; // get a random offset in the y direction
            // float sediment = 0; // how much sediment the current drop is holding
            // float xpos = x; // the current x pos
            // float ypos = y; // the current y pos

            drop.pos.x = x;
            drop.pos.y = y;
            drop.speed.x = 0;
            drop.speed.y = 0;
            drop.sediment = 0;
            drop.volume = parameters.StartingVolume;

            int iteration = 0;
            int maxspeedcount = 0;

            while (drop.volume > parameters.MINVolume && iteration < Constants.maxDropIterations)
            {
                SimKeyFrame frame = new SimKeyFrame();
                
                if (Constants.db_DebugMode && drops < Constants.db_DropRecordLength)
                {
                    frame.sediment = drop.sediment;
                    frame.volume = drop.volume;
                    frame.x = drop.pos.x;
                    frame.y = drop.pos.y;
                    frame.speed = drop.speed.magnitude;
                    
                    frames.Add(frame);
                }

                // make sure the drop is still in the bounds of the mesh
                if (map.SampleOutOfBounds(drop.pos.x, drop.pos.y)) break;

                Vector2 initialPos = drop.pos;
                Vector3 norm;
                
                norm = map.SampleBetaNormalAtXY((int)initialPos.x, (int)initialPos.y);

                frame.angle = Vector2.SignedAngle(norm, Vector2.right);

                Vector2 forceVector;

                if (parameters.FlipXY) forceVector = new Vector2(norm.z, norm.x);
                else forceVector = new Vector2(norm.x, norm.z);

                Vector2 F = parameters.DT * forceVector; // force
                float m = (drop.volume * parameters.Density); // mass

                // a = F/m
                drop.speed += F / m;

                // update pos
                drop.pos += parameters.DT * drop.speed;

                if (map.SampleOutOfBounds(drop.pos.x, drop.pos.y)) break;

                // apply friction
                drop.speed *= 1f - parameters.DT * parameters.Friction;

                if (drop.speed.magnitude > Constants.maxDropSpeed)
                {
                    ++maxspeedcount;
                    drop.speed = drop.speed.normalized * Constants.maxDropSpeed * drop.volume;
                }

                if (maxspeedcount > Constants.maxSpeedThreshold) break;

                // if ( drop.speed.magnitude > 1 ) Debug.Log("BIG SPEED BOIIIIII");

                // figure out sediment levels
                // the height at our initial position
                float prevHeight = map.SampleMapAtXY((int) initialPos.x, (int) initialPos.y);

                // the height at our current position
                float curHeight = map.SampleMapAtXY((int) drop.pos.x, (int) drop.pos.y);
                
                float travelDistance = drop.speed.magnitude * (prevHeight - curHeight);
                float maxsed = drop.volume * travelDistance;

                if (maxsed < 0) maxsed = 0f;
                float seddiff = maxsed - drop.sediment;

                // change the map based on the sediment
                drop.sediment += parameters.DT * parameters.DepositeRate * seddiff;

                float amount = parameters.DT * drop.volume * parameters.DepositeRate * seddiff;
                
                map.ChangeNode((int)initialPos.x, (int)initialPos.y, amount);
                
                // do some evaporation
                drop.volume *= 1f - parameters.DT * parameters.EvaporationRate;

                ++iteration;
            }

            if (Constants.db_DebugMode && drops < Constants.db_DropRecordLength)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("x,y,Volume,Sediment,Speed,Angle\n");

                foreach (var frame in frames)
                {
                    sb.AppendLine(frame.x + "," + frame.y + "," + frame.volume + "," + frame.sediment + "," + frame.speed + ", " +  frame.angle);
                }

                string filename = @"C:\Users\devpi\Documents\projects\MeshGenerator\WriteupMaterials\Drop" + drops + "OutputCSV.csv";
                
                File.WriteAllText(filename, sb.ToString());
            }
            
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
