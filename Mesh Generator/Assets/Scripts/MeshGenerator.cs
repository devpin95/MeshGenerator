using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using Unity.Mathematics;

public class MeshGenerator : MonoBehaviour
{
    public int xMeshOffset = 0;
    public int zMeshOffset = 0;

    public int row = 0;
    public int column = 0;
    

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private Color[] _colors;

    public HeightMapList maps;
    // public CEvent_String checkpointNotification;
    
    
    // private float _startTime;
    // private float _endTime;
    // private float _deltaTime;

    private float[,] _lattice;
    
    private MeshGenerationData _data = new MeshGenerationData();

    // [Header("Events")] 
    // public CEvent_MeshMetaData meshDataNotification;
    private MeshMetaData _metaData = new MeshMetaData();

    // Start is called before the first frame update
    void Start()
    {
        // StartCoroutine(GenerateMesh());
    }

    public void GenerateMesh()
    {
        CreateShape();
        UpdateMesh();
    }
    
    private void CreateShape()
    {
        CreateVerts();
        CreateTris();
        CreateColors();
    }

    private void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;

        _mesh.RecalculateNormals();
    }

    private void UpdateDirtyMesh()
    {
        // I guess our _mesh isnt the same anymore so just go get it from the mesh filter
        MeshFilter meshfilter = GetComponent<MeshFilter>();
        meshfilter.mesh.vertices = _vertices;
        meshfilter.mesh.colors = _colors;
        meshfilter.mesh.RecalculateNormals();
    }
    
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(xMeshOffset, 0, zMeshOffset), 1);
    }

    private void CreateVerts()
    {
        List<Vector3> vertlist = new List<Vector3>();
        float maxy = float.MinValue;
        float miny = float.MaxValue;
        
        // create vertices
        for (int z = 0; z < Constants.meshVerts; ++z)
        {
            for (int x = 0; x < Constants.meshVerts; ++x)
            {
                Vector3 newvert = new Vector3(x, 0, z);

                float y = 0;

                int realx = x + xMeshOffset;
                int realz = z + zMeshOffset;
                
                switch (_data.mapType)
                {
                    case Enums.HeightMapTypes.PerlinNoise:
                        y = SamplePerlinNoise(realx, realz);
                        break;
                    
                    case Enums.HeightMapTypes.ImageMap:
                        foreach (var map in maps.mapList)
                        {
                            y += SampleHeightMapImage(realx, realz, map);
                        }
                        break;
                    
                    case Enums.HeightMapTypes.PerlinOctaves:
                        y = SampleOctavePerlinNoise(realx, realz);
                        break;
                    
                    case Enums.HeightMapTypes.Plane:
                        break;
                    
                    case Enums.HeightMapTypes.SimpleNoise:
                        y = SampleSimpleNoise(realx, realz);
                        break;
                    
                    default:
                        newvert.y = 0;
                        break;
                }

                if (_data.invert && _data.mapType != Enums.HeightMapTypes.PerlinOctaves) y = 1 - y;

                if (y < miny) miny = y;
                if (y > maxy) maxy = y;
                
                newvert.y = y;
                vertlist.Add(newvert);
            }
        }
        
        _vertices = vertlist.ToArray();
    }

    private void CreateTris()
    {
        // checkpointNotification.Raise("Generating " + (_data.dimension + 2) * (_data.dimension + 2) + " polygons...");
        List<int> trilist = new List<int>();

        for( int z = 0; z < Constants.meshSquares; ++z )
        {
            int offset = z * (Constants.meshSquares + 1); // offset
            for (int x = 0; x < Constants.meshSquares; ++x)
            {
                int bl = x + offset;
                int tl = x + Constants.meshSquares + offset + 1;
                int tr = x + Constants.meshSquares + offset + 2;
                int br = x + offset + 1;
                
                // left tri
                trilist.Add(bl);
                trilist.Add(tl);
                trilist.Add(br);
                
                // right tri
                trilist.Add(br);
                trilist.Add(tl);
                trilist.Add(tr);
            }
        }
        
        // Debug.Log("Mesh has " + trilist.Count / 3 + " triangles");
        _triangles = trilist.ToArray();
    }

    private void CreateColors()
    {
        _colors = new Color[_vertices.Length];

        for (int i = 0; i < _colors.Length; ++i)
        {
            float remapped = Putils.Remap(_vertices[i].y, _data.remapMin, _data.remapMax, 0, 1);
            _colors[i] = Color.Lerp(Color.black, Color.white, remapped);
        }
    }

    public void RegenerateMesh(MeshGenerationData data)
    {
        Debug.Log(data.ToString());
        
        // bool needToShowVisualVerts = _visualVerts.Count > 0;
        // ShowVisualVertices(false);

        _data = data;

        if (_data.mapType == Enums.HeightMapTypes.SimpleNoise)
        {
            if (_data.simpleNoise.random)
            {
                UnityEngine.Random.InitState((int)Time.time * 432134 / 546632);
                _data.simpleNoise.seed = UnityEngine.Random.Range(int.MinValue, int.MinValue);

                while (_data.simpleNoise.sampleMin < 0)
                {
                    _data.simpleNoise.sampleMin += 10;
                    _data.simpleNoise.sampleMax += 10;
                }
            }

            GenerateLattice();
        }

        _mesh.Clear();
        Array.Clear(_triangles, 0, _triangles.Length);
        Array.Clear(_vertices, 0, _vertices.Length);
        
        // StartCoroutine(GenerateMesh());
        
        // if ( needToShowVisualVerts /*&& _vertices.Length < visualVerticeThreshold*/ ) ShowVisualVertices(true);
    }

    public float SamplePerlinNoise(int x, int z)
    {
        float xSamplePoint = Putils.Remap(x, 0, _data.dimension, _data.perlin.sampleMin, _data.perlin.sampleMax);
        float zSamplePoint = Putils.Remap(z, 0, _data.dimension, _data.perlin.sampleMin, _data.perlin.sampleMax);
        
        // float mapRange = _data.perlin.sampleMax - _data.perlin.sampleMin;
        //
        // float xSampleProportion = x / (float)_data.dimension;
        // float xMapProportion = mapRange * xSampleProportion;
        // float xSamplePoint = _data.perlin.sampleMax + xMapProportion;
        //
        // float zSampleProportion = z / (float)_data.dimension;
        // float zMapProportion = mapRange * zSampleProportion;
        // float zSamplePoint = _data.perlin.sampleMax + zMapProportion;

        float sample;
        
        if (_data.perlin.domainWarp)
        {
            Vector2 pos = new Vector2(xSamplePoint, zSamplePoint);
            
            Vector2 q = new Vector2( fbm( pos + new Vector2(0.0f,0.0f), _data.perlin.hurst ),
                fbm( pos + new Vector2(5.2f,1.3f), _data.perlin.hurst ) );

            Vector2 r = new Vector2( fbm( pos + 4.0f * q + new Vector2(1.7f,9.2f), _data.perlin.hurst ),
                fbm( pos + 4.0f * q + new Vector2(8.3f,2.8f), _data.perlin.hurst ) );

            sample = fbm( pos + 4.0f * r, _data.perlin.hurst );
        }
        else
        {
            float perlin = Mathf.PerlinNoise(xSamplePoint, zSamplePoint);
            sample = Mathf.Clamp01(perlin); // make sure that the value is actually between 0 and 1

            if (_data.perlin.ridged) sample = InvertAbs(sample);
        }
        
        float rmsample = Putils.Remap(sample, 0, 1, _data.remapMin, _data.remapMax);

        return rmsample;
    }

    public float SampleOctavePerlinNoise(int x, int z)
    {
        float y = 0;

        float xSamplePoint = Putils.Remap(x, 0, _data.dimension, _data.octaveNoise.sampleMin, _data.octaveNoise.sampleMax);
        float zSamplePoint = Putils.Remap(z, 0, _data.dimension, _data.octaveNoise.sampleMin, _data.octaveNoise.sampleMax);

        for (int i = 0; i < _data.octaveNoise.frequencies.Length; ++i)
        {
            float perlin = _data.octaveNoise.amplitudes[i] * Mathf.PerlinNoise(_data.octaveNoise.frequencies[i] * xSamplePoint, _data.octaveNoise.frequencies[i] * zSamplePoint);

            y += perlin;
        }

        y /= _data.octaveNoise.max;

        return Putils.Remap(y, 0, 1, _data.remapMin, _data.remapMax);
        return y;
    }

    public float SampleSimpleNoise(int x, int z)
    {
        float remappedx = Putils.Remap(x, 0, _data.dimension, _data.simpleNoise.sampleMin, _data.simpleNoise.sampleMax);
        remappedx += _data.simpleNoise.frequency;
        float remappedz = Putils.Remap(z, 0, _data.dimension, _data.simpleNoise.sampleMin, _data.simpleNoise.sampleMax);
        remappedz += _data.simpleNoise.frequency;
        
        // Debug.Log(x + ", " + z);
        //https://en.wikipedia.org/wiki/Bilinear_interpolation
        
        // bring the values back to the origin block [0, latticeDim]
        float modX = remappedx % _data.simpleNoise.latticeDim;
        float modY = remappedz % _data.simpleNoise.latticeDim;

        int xmin = (int) modX;
        int xmax = xmin + 1;
        int ymin = (int) modY;
        int ymax = ymin + 1;

        // if we go off the edge of the lattice, we need to loop back around from the bottom
        int yrounded = ymax;
        int xrounded = xmax;

        if (ymax >= _data.simpleNoise.latticeDim) yrounded = 0;
        if (xmax >= _data.simpleNoise.latticeDim) xrounded = 0;
        
        float Q11 = Lattice.Instance.SampleLattice(xmin, ymin); // bottom left point
        float Q12 = Lattice.Instance.SampleLattice(xmin, yrounded); // top left point
        float Q21 = Lattice.Instance.SampleLattice(xrounded, ymin); // bottom right point
        float Q22 = Lattice.Instance.SampleLattice(xrounded, yrounded); // top right point
        
        // linear interpolation in the x direction
        float R1 = (xmax - modX) * Q11 + (modX - xmin) * Q21; // x on the line between Q11 and Q21, y = ymin
        float R2 = (xmax - modX) * Q12 + (modX - xmin) * Q22; // x on the line between Q12 and Q22, y = ymax
        
        // linear interpolation in the y direction (between R1 and R2)
        float p = (ymax - modY) * R1 + (modY - ymin) * R2;

        var smooth = Smoothing.Algorithms[_data.simpleNoise.smoothing];
        float psmooth = smooth(p);

        float remapp = Putils.Remap(psmooth, 0, 1, _data.remapMin, _data.remapMax);
        
        return remapp * _data.simpleNoise.scale;
    }

    public float SampleHeightMapImage(int x, int z, LayerData data)
    {
        float y = 0;
        int xSamplePoint = (int)Putils.Remap(x, 0, _data.dimension, 0, data.map.width);
        int zSamplePoint = (int)Putils.Remap(z, 0, _data.dimension, 0, data.map.height);

        y = data.map.GetPixel(xSamplePoint, zSamplePoint).grayscale;

        float yremap = Putils.Remap(y, 0, 1, data.remapMin, data.remapMax);

        return yremap;
    }

    public float fbm(Vector2 pos, float hurst)
    {
        float t = 0.0f;
        int numOctaves = 2;

        for (int i = 0; i < numOctaves; ++i)
        {
            float f = Mathf.Pow(2.0f, (float) i);
            float a = Mathf.Pow(f, -hurst);
            
            float perlin = Mathf.PerlinNoise(f * pos.x, f * pos.y);
            float sample = Mathf.Clamp01(perlin); // make sure that the value is actually between 0 and 1

            if (_data.perlin.ridged) sample = InvertAbs(sample);
            
            // t += a * Mathf.PerlinNoise(f * pos.x, f * pos.y);
            t += a * sample;
        }

        return t;
    }

    private Texture2D GenerateHeightMapTexture(int size, Vector2 dim)
    {
        Texture2D tex = new Texture2D(_data.dimension + 1, _data.dimension + 1);

        Color[] colors = new Color[size];
        
        
        // Debug.Log("Size: " + xSize + "x" + zSize + " = " + ((xSize + 1) * (zSize + 1)));

        for (int i = 0; i < size; ++i)
        {
            // Debug.Log("Index (" + z + "," + x + ") [" + (z + x * zSize) + "]");
            float height = _vertices[i].y;

            float heightTo01 = Putils.Remap(height, _data.remapMin, _data.remapMax, 0, 1);
            // float height01ToRGB = Remap(heightTo01, 0, 1, 0, 255);
                
            colors[i] = new Color(heightTo01, heightTo01, heightTo01, 1);

            // Debug.Log("Index (" + i  + ") [" + " Height " + height + " remapped to (" + heightTo01 + ", " + height01ToRGB + ")");
        }

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(0, 0, Constants.meshSquares, Constants.meshSquares, colors);
        tex.Apply();

        return tex;
    }

    public int GetMeshDimensions()
    {
        return _data.dimension;
    }

    private void GenerateLattice()
    {
        UnityEngine.Random.InitState(_data.simpleNoise.seed);
        
        _lattice = new float[_data.simpleNoise.latticeDim, _data.simpleNoise.latticeDim];
        
        for (int x = 0; x < _data.simpleNoise.latticeDim; ++x)
        {
            for (int y = 0; y < _data.simpleNoise.latticeDim; ++y)
            {
                _lattice[x, y] = UnityEngine.Random.Range(0.0f, 1.0f);
            }
        }
    }

    public void InitData(MeshGenerationData ndata)
    {
        _data = ndata;
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    public void ClearMesh()
    {
        _mesh.Clear();
        Array.Clear(_triangles, 0, _triangles.Length);
        Array.Clear(_vertices, 0, _vertices.Length);
    }

    private float InvertAbs(float val)
    {
        val = Putils.Remap(val, 0, 1, -1, 1);
        val = Mathf.Abs(val);
        val *= -1;
        val = Putils.Remap(val, -1, 0, 0, 1);
        return val;
    }

    public void UpdateVerts(HeightMap map, int coloffset, int rowoffset)
    {
        var vertgrid = Putils.FlatArrayToTwoDArray(_vertices, Constants.meshVerts, Constants.meshVerts);
        
        for (int mrow = 0; mrow < Constants.meshVerts; ++mrow)
        {
            for (int mcol = 0; mcol < Constants.meshVerts; ++mcol)
            {
                // bool skip = false;
                // int index = x + y * Constants.meshSquares;

                vertgrid[mrow, mcol].y = map.SampleMapAtXYOffset(mrow, rowoffset, mcol, coloffset);
                
                // if (x == Constants.meshSquares - 1)
                // {
                //     // we're at the end of the row
                //     // we need to take the bottom right cell point too
                //     _vertices[index + skippedx + 1].y = map.SampleMapAtXY(x, xoffset, y, yoffset, HeightMap.Nodes.BottomRight);
                //     skip = true;
                // }
                //
                // if (y == Constants.meshSquares - 1)
                // {
                //     _vertices[index + Constants.meshVerts + skippedx ].y = map.SampleMapAtXY(x, xoffset, y, yoffset, HeightMap.Nodes.TopLeft);
                // }
                //
                // if (x == Constants.meshSquares - 1 && y == Constants.meshSquares - 1)
                // {
                //     _vertices[index + Constants.meshVerts + skippedx + 1].y = map.SampleMapAtXY(x, xoffset, y, yoffset, HeightMap.Nodes.TopRight);
                // }
                //
                // if (skip) ++skippedx;
            }
        }

        _vertices = Putils.Flatten2DArray(vertgrid, Constants.meshVerts, Constants.meshVerts);
        
        CreateColors();

        UpdateDirtyMesh();
    }
}
