using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public enum HeightMapTypes
    {
        Plane,
        SimpleNoise,
        PerlinNoise,
        ImageMap
    }

    public static Dictionary<HeightMapTypes, string> HeightMapTypeNames = new Dictionary<HeightMapTypes, string>()
    {
        {HeightMapTypes.Plane, "Plane"},
        {HeightMapTypes.SimpleNoise, "Simple Noise"},
        {HeightMapTypes.PerlinNoise, "Perlin Noise"},
        {HeightMapTypes.ImageMap, "Image Map"}
    };
    
    public static Dictionary<string, HeightMapTypes> HeightMapNameTypes = new Dictionary<string, HeightMapTypes>()
    {
        {"Plane", HeightMapTypes.Plane},
        {"Simple Noise", HeightMapTypes.SimpleNoise},
        {"Perlin Noise", HeightMapTypes.PerlinNoise},
        {"Image Map", HeightMapTypes.ImageMap}
    };
    
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    private List<GameObject> _visualVerts = new List<GameObject>();

    public int xSize = 2;
    public int zSize = 2;

    public HeightMapList maps;

    private HeightMapTypes _mapType = HeightMapTypes.Plane;

    private float _remapMin = -5;
    private float _remapMax = 5;

    private float _perlinNoiseMin = 0;
    private float _perlinNoiseMax = 1;
    
    private bool _domainWarp = false;
    private int _octaves = 1;
    private float _hurstExponent = 0.5f;

    private float startTime;
    private float endTime;
    private float deltaTime;

    private Texture2D _generatedMap;

    [Header("Events")] 
    public CEvent_MeshMetaData meshDataNotification;
    private MeshMetaData _metaData = new MeshMetaData();

    // Start is called before the first frame update
    void Start()
    {
       
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        startTime = Time.realtimeSinceStartup;
        CreateShape();
        endTime = Time.realtimeSinceStartup;
        deltaTime = endTime - startTime;

        _metaData.vertexCount = _vertices.Length;
        _metaData.polyCount = _triangles.Length / 3;
        _metaData.generationTimeMS = deltaTime * 1000;

        _metaData.heightMap = GenerateHeightMapTexture();
        
        meshDataNotification.Raise(_metaData);
        
        UpdateMesh();
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
        
        // create vertices
        for (int z = 0; z <= zSize; ++z)
        {
            for (int x = 0; x <= xSize; ++x)
            {
                Vector3 newvert = new Vector3(x, 0, z);

                float y = 0;
                
                switch (_mapType)
                {
                    case HeightMapTypes.PerlinNoise:
                        y = SamplePerlinNoise(x, z);
                        break;
                    case HeightMapTypes.ImageMap:
                        foreach (var map in maps.mapList)
                        {
                            y += SampleHeightMap(x, z, map);
                        }
                        break;
                    case HeightMapTypes.Plane:
                    case HeightMapTypes.SimpleNoise:
                    default:
                        newvert.y = 0;
                        break;
                }
                
                newvert.y = y;
                vertlist.Add(newvert);
            }
        }

        _vertices = vertlist.ToArray();
    }

    private void CreateTris()
    {
        List<int> trilist = new List<int>();
        
        for( int z = 0; z < zSize; ++z )
        {
            int y = z * (xSize + 1); // offset
            for (int x = 0; x < xSize; ++x)
            {
                int bl = x + y;
                int tl = x + xSize + y + 1;
                int tr = x + xSize + y + 2;
                int br = x + y + 1;
                
                // left tri
                trilist.Add(bl);
                trilist.Add(tl);
                trilist.Add(br);
                
                // right tri
                trilist.Add(br);
                trilist.Add(tl);
                trilist.Add(tr);
                
                // Debug.Log("Creating triangles at <" + bl + ", " + tl + ", " + br + "> and <" + br + ", " + tl + ", " + tr + ">");
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

        xSize = data.dimension;
        zSize = data.dimension;

        _mapType = data.mapType;

        if (_mapType == HeightMapTypes.ImageMap)
        {
            Debug.Log("Extracting height from " + maps.mapList.Count + " maps");
        }
        
        _perlinNoiseMin = data.perlinNoiseSampleMin;
        _perlinNoiseMax = data.perlinNoiseSampleMax;

        _domainWarp = data.domainWarp;
        _octaves = data.octaves;
        _hurstExponent = data.hurst;
        
        Debug.Log("PERLIN " + _perlinNoiseMin + " " + _perlinNoiseMax);

        _remapMin = data.remapMin;
        _remapMax = data.remapMax;
        
        _mesh.Clear();
        Array.Clear(_triangles, 0, _triangles.Length);
        Array.Clear(_vertices, 0, _vertices.Length);
        
        GenerateMesh();
        
        if ( needToShowVisualVerts /*&& _vertices.Length < visualVerticeThreshold*/ ) ShowVisualVertices(true);
    }

    public float SamplePerlinNoise(int x, int z)
    {
        float mapRange = _perlinNoiseMax - _perlinNoiseMin;
        
        float xSampleProportion = x / (float)xSize + 1;
        float xMapProportion = mapRange * xSampleProportion;
        float xSamplePoint = _perlinNoiseMin + xMapProportion;
        
        float zSampleProportion = z / (float)zSize + 1;
        float zMapProportion = mapRange * zSampleProportion;
        float zSamplePoint = _perlinNoiseMin + zMapProportion;

        float sample;
        
        if (_domainWarp)
        {
            Vector2 pos = new Vector2(xSamplePoint, zSamplePoint);
            
            Vector2 q = new Vector2( fbm( pos + new Vector2(0.0f,0.0f), _hurstExponent ),
                fbm( pos + new Vector2(5.2f,1.3f), _hurstExponent ) );

            Vector2 r = new Vector2( fbm( pos + 4.0f * q + new Vector2(1.7f,9.2f), _hurstExponent ),
                fbm( pos + 4.0f * q + new Vector2(8.3f,2.8f), _hurstExponent ) );

            sample = fbm( pos + 4.0f * r, _hurstExponent );
        }
        else
        {
            float perlin = Mathf.PerlinNoise(xSamplePoint, zSamplePoint);
            sample = Mathf.Clamp01(perlin); // make sure that the value is actually between 0 and 1
        }
        
        float rmsample = Remap(sample, 0, 1, _remapMin, _remapMax);
        
        // Debug.Log("(" + x + ", " + z + "), " + "(" + _remapMin + ", " + _remapMax + "), (" + _perlinNoiseMin + ", " + _perlinNoiseMax + "), (" + xSamplePoint + ", " + zSamplePoint + ") = " + rmperlin);

        return rmsample;
    }

    public float SampleHeightMap(int x, int z, LayerData data)
    {
        float y = 0;
        int xSamplePoint = (int)Remap(x, 0, xSize, 0, data.map.width);
        int zSamplePoint = (int)Remap(z, 0, zSize, 0, data.map.height);

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

    private Texture2D GenerateHeightMapTexture()
    {
        Texture2D tex = new Texture2D(xSize, zSize);

        for (int z = 0; z < zSize; ++z)
        {
            for (int x = 0; x < xSize; ++x)
            {
                float height = _vertices[x + z * zSize].y;

                float heightTo01 = Remap(height, _remapMin, _remapMax, 0, 1);
                float height01ToRGB = Remap(heightTo01, 0, 1, 0, 255);

                Color color = new Color(height01ToRGB, height01ToRGB, height01ToRGB);
                tex.SetPixel(x, z, color);
            }
        }

        return tex;
    }
}
