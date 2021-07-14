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

    private HeightMapTypes _mapType = HeightMapTypes.Plane;

    private float _remapMin = -5;
    private float _remapMax = 5;

    private float _perlinNoiseMin = 0;
    private float _perlinNoiseMax = 1;

    public int visualVerticeThreshold = 10000;

    private float startTime;
    private float endTime;
    private float deltaTime;

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

                switch (_mapType)
                {
                    case HeightMapTypes.PerlinNoise:
                        float y = SamplePerlinNoise(x, z);
                        newvert.y = y;
                        break;
                    case HeightMapTypes.Plane:
                    case HeightMapTypes.SimpleNoise:
                    case HeightMapTypes.ImageMap:
                    default:
                        newvert.y = 0;
                        break;
                }
                
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
        
        _perlinNoiseMin = data.perlinNoiseSampleMin;
        _perlinNoiseMax = data.perlinNoiseSampleMax;
        
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

        float perlin = Mathf.PerlinNoise(xSamplePoint, zSamplePoint);
        float cperlin = Mathf.Clamp01(perlin); // make sure that the value is actually between 0 and 1
        float rmperlin = Remap(cperlin, 0, 1, _remapMin, _remapMax);
        
        // Debug.Log("(" + x + ", " + z + "), " + "(" + _remapMin + ", " + _remapMax + "), (" + _perlinNoiseMin + ", " + _perlinNoiseMax + "), (" + xSamplePoint + ", " + zSamplePoint + ") = " + rmperlin);

        return rmperlin;
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
