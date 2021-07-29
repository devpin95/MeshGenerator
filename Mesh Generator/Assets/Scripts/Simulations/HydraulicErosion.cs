using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEngine;

public class HydraulicErosion
{
    private HeightMap _map;
    private HydraulicErosionParameters _parameters;
    
    public void Simulate(HeightMap map, HydraulicErosionParameters parameters)
    {
        _map = map;
        _parameters = parameters;
    }

    private void Iterate()
    {
        int mapdim = _map.WidthAndHeight();
        int drops = _parameters.DropCount * mapdim * mapdim;

        for (int i = 0; i < drops; ++i)
        {
            float randx = Random.Range(0, mapdim);
            float randy = Random.Range(0, mapdim);
            Trace(randx, randy);
        }
    }

    private void Trace(float x, float y)
    {
        float xoffset = Random.Range(-1, 1) * _parameters.Radius; // get a random offset in the x direction
        float yoffset = Random.Range(-1, 1) * _parameters.Radius; // get a random offset in the y direction
        float sediment = 0; // how much sediment the current drop is holding
        float xpos = x; // the current x pos
        float ypos = y; // the current y pos
        float xvelocity = 0; // drop velocity in the x direction
        float yvelocity = 0; // drop velocity in the y direction

        // do the trace for MaxInterations
        for (int i = 0; i < _parameters.MaxIterations; ++i)
        {
            // get the normal at the random position
            Vector3 norm = _map.SampleNormalAtXY(xpos + xoffset, ypos + yoffset);

            if (Putils.normIsUp(norm)) break; // we are either straight up or off the map, so we need to stop

            float deposite = sediment * _parameters.DepositeRate * norm.y;
            float erosion = _parameters.ErosionRate * (1 - norm.y) * Mathf.Min(1, i * _parameters.IterationScale);

            _map.ChangeCell(xpos, ypos, deposite - erosion);

            xvelocity = _parameters.Friction * xvelocity * norm.x * _parameters.Speed;
            yvelocity = _parameters.Friction * yvelocity * norm.z * _parameters.Speed;

            x += xvelocity;
            y += yvelocity;
            xpos = x;
            ypos = y;

            sediment += erosion - deposite;
        }
    }
}
