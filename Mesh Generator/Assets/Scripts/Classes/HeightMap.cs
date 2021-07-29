using System;
using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEngine;

public class HeightMap
{
    private class Cell
    {
        public float tl = 0;
        public float tr = 0;
        public float bl = 0;
        public float br = 0;
        public Vector3 lnorm = Vector3.up;
        public Vector3 rnorm = Vector3.up;

        public void ChangeLeftTriangle(float amount)
        {
            bl += amount;
            tl += amount;
            br += amount;
        }

        public void ChangeRightTriangle(float amount)
        {
            br += amount;
            tl += amount;
            tr += amount;
        }
    }

    public int meshCount = 1;
    private Cell[] map;

    public GlobalParameters globalParams;
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

                cell.lnorm = CalculateNormal(cell.bl, cell.tl, cell.br);
                cell.rnorm = CalculateNormal(cell.tr, cell.br, cell.tl);
                
                map[index] = cell;
            }
        }
    }

    private Cell GetCellAtPosition(int x, int y)
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
                
                float rmbl = Putils.Remap(map[index].bl, min, max, 0, 1);
                colors[index + skipped] = new Color(rmbl, rmbl, rmbl, 1);

                // set the right column
                if (x == rowlength - 1)
                {
                    float rmbr = Putils.Remap(map[index].br, min, max, 0, 1);
                    colors[index + 1 + skipped] = new Color(rmbr, rmbr, rmbr, 1);
                    didskip = true;
                }
                
                // set the top row
                if (y == rowlength - 1)
                {
                    float rmtl = Putils.Remap(map[index].tl, min, max, 0, 1);
                    colors[index + rowlength + skipped + 1] = new Color(rmtl, rmtl, rmtl, 1);
                }

                // set the top right element
                if (x == rowlength - 1 && y == rowlength - 1)
                {
                    float rmtr = Putils.Remap(map[index].tr, min, max, 0, 1);
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

        return Putils.Tex2dToSprite(tex);
    }
    
    // public float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax) {
    //     var fromAbs  =  from - fromMin;
    //     var fromMaxAbs = fromMax - fromMin;      
    //    
    //     var normal = fromAbs / fromMaxAbs;
    //
    //     var toMaxAbs = toMax - toMin;
    //     var toAbs = toMaxAbs * normal;
    //
    //     var to = toAbs + toMin;
    //    
    //     return to;
    // }

    private Vector3 CalculateNormal(float a, float b, float c)
    {
        Vector3 p1 = Vector3.zero;
        p1.y = a;
        
        Vector3 p2 = Vector3.forward;
        p2.y = b;
        
        Vector3 p3 = Vector3.right;
        p3.y = c;

        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;

        Vector3 norm = Vector3.Cross(v1, v2).normalized;
        
        return norm;
    }

    public int WidthAndHeight()
    {
        return meshCount * Constants.meshSquares;
    }

    public Vector3 SampleNormalAtXY(float x, float y)
    {
        Vector3 norm = Vector3.up;

        int intx = (int) x;
        int inty = (int) y;
        float decx = x - intx;
        float decy = y - inty;

        if (intx < 0 || intx > WidthAndHeight()) return norm;
        if (inty < 0 || inty > WidthAndHeight()) return norm;

        Cell cell = map[intx + inty * WidthAndHeight()];

        if (decx >= decy) norm = cell.rnorm;
        else norm = cell.lnorm;

        return norm.normalized;
    }

    public void ChangeCell(float x, float y, float amount)
    {
        int intx = (int) x;
        int inty = (int) y;
        float decx = x - intx;
        float decy = y - inty;
        

        Cell cell = map[intx + inty * WidthAndHeight()];

        if (decx >= decy) cell.ChangeRightTriangle(amount);
        else cell.ChangeLeftTriangle(amount);
    }
}
