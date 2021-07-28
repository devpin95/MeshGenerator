using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMap
{
    public class Cell
    {
        public float tl = 0;
        public float tr = 0;
        public float bl = 0;
        public float br = 0;
    }

    public int meshCount = 1;
    public Cell[] map;

    private int _area = 0;

    public void InitMap(int size)
    {
        meshCount = size;
        int realsize = meshCount * Constants.meshSquares;
        _area = realsize * realsize;
        
        Debug.Log("Height map area = " + _area);
        
        map = new Cell[_area];
    }

    public void SetChunk(Vector3[] verts, int xoffset, int yoffset)
    {
        // Debug.Log("XOFFSET: " + xcoffset + " YOFFSET: " + ycoffset);
        if (map == null) throw new Exception("Map not initialized");

        int startingy = Constants.meshVerts * yoffset;
        int endingy = Constants.meshVerts * yoffset + Constants.meshVerts;
        if (startingy > 0)
        {
            startingy -= yoffset;
            endingy -= yoffset;
        }
        
        int startingx = Constants.meshVerts * xoffset;
        int endingx = Constants.meshVerts * xoffset + Constants.meshVerts;
        if (startingx > 0)
        {
            startingx -= xoffset;
            endingx -= xoffset;
        }

        for (int y = startingy; y < endingy - 1; ++y)
        {
            for (int x = startingx; x < endingx - 1; ++x)
            {
                int index = x + y * (Constants.meshSquares * meshCount);

                int lx = x - startingx;
                int ly = y - startingy;
                
                Cell cell = new Cell();
                int offset = ly * Constants.meshVerts;
                cell.bl = verts[lx + offset].y;
                cell.br = verts[lx + offset + 1].y;
                cell.tl = verts[lx + Constants.meshVerts + offset].y;
                cell.tr = verts[lx + Constants.meshVerts + offset + 1].y;
                map[index] = cell;
                // Debug.Log(index);
            }
        }
        
        // for (int y = 0; y < Constants.meshSquares; ++y)
        // {
        //     for (int x = 0; x < Constants.meshSquares; ++x)
        //     {
        //         // get the local index of the cell
        //         int vertind = x + y * Constants.meshSize;
        //         
        //         // get the 4 corners of the cell
        //         Vector3 vertbl = verts[vertind];
        //         Vector3 vertbr = verts[vertind + 1];
        //         Vector3 verttl = verts[vertind + Constants.meshSize + 1];
        //         Vector3 verttr = verts[vertind + Constants.meshSize + 2];
        //         
        //         // get the global index of the cell in the map
        //         int xglobal = (xoffset * Constants.meshSize) + x;
        //         int yglobal = (yoffset * Constants.meshSize) + y;
        //         int mapind = xglobal + (yglobal * meshCount * Constants.meshSquares);
        //
        //         // Debug.Log("M(" + xoffset + ", " + yoffset + ") L(" + x + ", " + y + ") => G(" + xglobal + ", " + yglobal + ") = " + mapind);
        //         // Debug.Log("X: " + xglobal + " Y: " + yglobal + " = " + map_ind);
        //         
        //         // add the cell to the map
        //         Cell cell = new Cell();
        //         cell.tl = verttl.y;
        //         cell.tr = verttr.y;
        //         cell.bl = vertbl.y;
        //         cell.br = vertbr.y;
        //         map[mapind] = cell;
        //     }
        // }
    }

    public Cell GetCellAtPosition(int x, int y)
    {
        return map[ x + y * meshCount];
    }

    public Sprite GenerateHeightMapPreview(float min, float max)
    {
        int edgesquares = meshCount * Constants.meshVerts;
        int overlappingverts = meshCount - 1;
        int mapedge = edgesquares - overlappingverts;
        Texture2D tex = new Texture2D(mapedge, mapedge);

        Color[] colors = new Color[mapedge * mapedge];
        int rowlength = meshCount * Constants.meshSquares;
        int skipped = 0;

        for (int y = 0; y < rowlength; ++y)
        {
            for (int x = 0; x < rowlength; ++x)
            {
                bool didskip = false;
                int index = x + y * rowlength;
                
                float rmbl = Remap(map[index].bl, min, max, 0, 1);
                colors[index + skipped] = new Color(rmbl, rmbl, rmbl, 1);

                // set the right column
                if (x == rowlength - 1)
                {
                    float rmbr = Remap(map[index].br, min, max, 0, 1);
                    colors[index + 1 + skipped] = new Color(rmbr, rmbr, rmbr, 1);
                    didskip = true;
                }
                
                // set the top row
                if (y == rowlength - 1)
                {
                    float rmtl = Remap(map[index].tl, min, max, 0, 1);
                    colors[index + rowlength + skipped + 1] = new Color(rmtl, rmtl, rmtl, 1);
                }

                // set the top right element
                if (x == rowlength - 1 && y == rowlength - 1)
                {
                    float rmtr = Remap(map[index].tr, min, max, 0, 1);
                    colors[index + rowlength + skipped + 2] = new Color(rmtr, rmtr, rmtr, 1);
                } 
                
                if ( didskip ) ++skipped;
            }
        }
        
        // for (int i = 0; i < map.Length; ++i)
        // {
        //     float rmbl = Remap(map[i].bl, min, max, 0, 1);
        //     colors[i] = new Color(rmbl, rmbl, rmbl, 1);
        // }
        
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(0, 0, mapedge, mapedge, colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
    
    public float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax) {
        var fromAbs  =  from - fromMin;
        var fromMaxAbs = fromMax - fromMin;      
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }
}
