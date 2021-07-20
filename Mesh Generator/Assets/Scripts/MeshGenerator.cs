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

public class MeshGenerator : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    private List<GameObject> _visualVerts = new List<GameObject>();

    public HeightMapList maps;
    public CEvent_String checkpointNotification;
    
    
    private float _startTime;
    private float _endTime;
    private float _deltaTime;

    private float[,] _lattice;
    
    private MeshGenerationData _data = new MeshGenerationData();

    [Header("Events")] 
    public CEvent_MeshMetaData meshDataNotification;
    private MeshMetaData _metaData = new MeshMetaData();

    // Start is called before the first frame update
    void Start()
    {
       
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        StartCoroutine(GenerateMesh());
    }

    IEnumerator GenerateMesh()
    {
        while (true)
        {
            checkpointNotification.Raise("Generating " + Enums.HeightMapTypeNames[_data.mapType] + " mesh...");
            _startTime = Time.realtimeSinceStartup;
            CreateShape();
            _endTime = Time.realtimeSinceStartup;
            _deltaTime = _endTime - _startTime;

            yield return null;
            
            _metaData.vertexCount = _vertices.Length;
            _metaData.polyCount = _triangles.Length / 3;
            _metaData.generationTimeMS = _deltaTime * 1000;
            
            yield return null;
        
            checkpointNotification.Raise("Mesh generation completed in " + _metaData.generationTimeMS +  "ms. Updating mesh object and recalculating normals...");
            UpdateMesh();
            
            checkpointNotification.Raise("Generating height map preview...");
            _metaData.heightMap = GenerateHeightMapTexture(_vertices.Length, new Vector2(_data.dimension, _data.dimension));
            
            yield return new WaitForSeconds(1);
            meshDataNotification.Raise(_metaData);

            break;
        }
        
        yield break;
    }
    
    private void CreateShape()
    {
        CreateVerts();
        CreateTris();
    }

    private void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        
        _mesh.RecalculateNormals();
    }

    // public void OnDrawGizmos()
    // {
    //     if (_vertices == null) return;
    //
    //     int i = 0;
    //     foreach (var vert in _vertices)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawSphere(vert, .01f);
    //         Handles.Label(vert, i.ToString());
    //
    //         ++i;
    //     }
    // }

    private void CreateVerts()
    {
        List<Vector3> vertlist = new List<Vector3>();
        
        checkpointNotification.Raise("Generating " + (_data.dimension + 1) + "x" + (_data.dimension + 1) + " grid (" + (_data.dimension + 1 * _data.dimension + 1) + " verts)...");
        // create vertices
        for (int z = 0; z <= _data.dimension; ++z)
        {
            for (int x = 0; x <= _data.dimension; ++x)
            {
                Vector3 newvert = new Vector3(x, 0, z);

                float y = 0;
                
                switch (_data.mapType)
                {
                    case Enums.HeightMapTypes.PerlinNoise:
                        y = SamplePerlinNoise(x, z);
                        break;
                    case Enums.HeightMapTypes.ImageMap:
                        foreach (var map in maps.mapList)
                        {
                            y += SampleHeightMap(x, z, map);
                        }
                        break;
                    case Enums.HeightMapTypes.Plane:
                        break;
                    case Enums.HeightMapTypes.SimpleNoise:
                        y = SampleSimpleNoise(x, z);
                        break;
                    default:
                        newvert.y = 0;
                        break;
                }

                if (_data.invert) y = 1 - y;
                newvert.y = y;
                vertlist.Add(newvert);
            }
        }

        _vertices = vertlist.ToArray();
    }

    private void CreateTris()
    {
        checkpointNotification.Raise("Generating " + (_data.dimension + 2) * (_data.dimension + 2) + " polygons...");
        List<int> trilist = new List<int>();
        
        for( int z = 0; z < _data.dimension; ++z )
        {
            int y = z * (_data.dimension + 1); // offset
            for (int x = 0; x < _data.dimension; ++x)
            {
                int bl = x + y;
                int tl = x + _data.dimension + y + 1;
                int tr = x + _data.dimension + y + 2;
                int br = x + y + 1;
                
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
        
        _triangles = trilist.ToArray();
    }

    public void ShowVisualVertices(bool show)
    {
        if (show)
        {
            foreach (var vert in _vertices)
            {
                var obj = PrimativePool.Instance.GetPooledObject();
                if (obj)
                {
                    obj.transform.position = vert;
                    _visualVerts.Add(obj);
                }
            }
        }
        else
        {
            foreach (var obj in _visualVerts)
            {
                PrimativePool.Instance.ReturnToPool(obj);
            }
            _visualVerts = new List<GameObject>();
        }
    }

    public void RegenerateMesh(MeshGenerationData data)
    {
        Debug.Log(data.ToString());
        
        bool needToShowVisualVerts = _visualVerts.Count > 0;

        ShowVisualVertices(false);

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
        
        StartCoroutine(GenerateMesh());
        
        if ( needToShowVisualVerts /*&& _vertices.Length < visualVerticeThreshold*/ ) ShowVisualVertices(true);
    }

    public float SamplePerlinNoise(int x, int z)
    {
        float mapRange = _data.perlin.sampleMax - _data.perlin.sampleMin;
        
        float xSampleProportion = x / (float)_data.dimension + 1;
        float xMapProportion = mapRange * xSampleProportion;
        float xSamplePoint = _data.perlin.sampleMax + xMapProportion;
        
        float zSampleProportion = z / (float)_data.dimension + 1;
        float zMapProportion = mapRange * zSampleProportion;
        float zSamplePoint = _data.perlin.sampleMax + zMapProportion;

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
        }
        
        float rmsample = Remap(sample, 0, 1, _data.remapMin, _data.remapMax);

        return rmsample;
    }

    public float SampleSimpleNoise(int x, int z)
    {
        float remappedx = Remap(x, 0, _data.dimension, _data.simpleNoise.sampleMin, _data.simpleNoise.sampleMax);
        remappedx += _data.simpleNoise.frequency;
        float remappedz = Remap(z, 0, _data.dimension, _data.simpleNoise.sampleMin, _data.simpleNoise.sampleMax);
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
        
        float Q11 = _lattice[xmin, ymin]; // bottom left point
        float Q12 = _lattice[xmin, yrounded]; // top left point
        float Q21 = _lattice[xrounded, ymin]; // bottom right point
        float Q22 = _lattice[xrounded, yrounded]; // top right point
        
        // linear interpolation in the x direction
        float R1 = (xmax - modX) * Q11 + (modX - xmin) * Q21; // x on the line between Q11 and Q21, y = ymin
        float R2 = (xmax - modX) * Q12 + (modX - xmin) * Q22; // x on the line between Q12 and Q22, y = ymax
        
        // linear interpolation in the y direction (between R1 and R2)
        float p = (ymax - modY) * R1 + (modY - ymin) * R2;

        var smooth = Smoothing.Algorithms[_data.simpleNoise.smoothing];
        float psmooth = smooth(p);

        float remapp = Remap(psmooth, 0, 1, _data.remapMin, _data.remapMax);
        
        return remapp * _data.simpleNoise.scale;
    }

    public float SampleHeightMap(int x, int z, LayerData data)
    {
        float y = 0;
        int xSamplePoint = (int)Remap(x, 0, _data.dimension, 0, data.map.width);
        int zSamplePoint = (int)Remap(z, 0, _data.dimension, 0, data.map.height);

        y = data.map.GetPixel(xSamplePoint, zSamplePoint).grayscale;

        float yremap = Remap(y, 0, 1, data.remapMin, data.remapMax);

        return yremap;
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

    public float fbm(Vector2 pos, float hurst)
    {
        float t = 0.0f;
        int numOctaves = 2;

        for (int i = 0; i < numOctaves; ++i)
        {
            float f = Mathf.Pow(2.0f, (float) i);
            float a = Mathf.Pow(f, -hurst);
            t += a * Mathf.PerlinNoise(f * pos.x, f * pos.y);
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

            float heightTo01 = Remap(height, _data.remapMin, _data.remapMax, 0, 1);
            // float height01ToRGB = Remap(heightTo01, 0, 1, 0, 255);
                
            colors[i] = new Color(heightTo01, heightTo01, heightTo01, 1);

            // Debug.Log("Index (" + i  + ") [" + " Height " + height + " remapped to (" + heightTo01 + ", " + height01ToRGB + ")");
        }

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(0, 0, _data.dimension + 1, _data.dimension + 1, colors);
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
}
