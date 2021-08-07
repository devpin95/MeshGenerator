using System;
using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public int meshCount = 1; // meshCount^2 is the number of meshes that will be generated
    public GameObject meshPrefab;
    public CEvent_Vector3 updateCameraEvent;
    
    // [Header("Mesh Variables")] 
    // public int spacing = 1;
    private int meshOffest;

    private List<GameObject> _meshes = new List<GameObject>();
    private List<MeshGenerator> _generators = new List<MeshGenerator>();

    private MeshGenerationData _data = new MeshGenerationData();
    
    [Header("Events")]
    public CEvent_MeshMetaData meshDataNotification;
    public CEvent_String checkpointNotification;

    [Header("Parameters")] 
    public GlobalParameters globalParameters;
    
    private float _startTime;
    private float _endTime;
    private float _deltaTime;
    private MeshMetaData _metaData = new MeshMetaData();
    private HeightMap _heightMap = new HeightMap();

    // Start is called before the first frame update
    void Start()
    {
        // meshOffest = spacing * 255f;
        StartCoroutine(StartMeshGeneration());
        _heightMap.globalParams = globalParameters;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartMeshGeneration()
    {
        float totalDelay = 1.5f;
        _startTime = Time.realtimeSinceStartup;

        if (_data.mapType == Enums.HeightMapTypes.SimpleNoise)
        {
            checkpointNotification.Raise("Generating " + _data.simpleNoise.latticeDim + "x" + _data.simpleNoise.latticeDim + " lattice with seed " + _data.simpleNoise.seed + "...");
            yield return new WaitForSeconds(0.25f);
            Lattice.Instance.GenerateLattice(_data.simpleNoise.latticeDim, _data.simpleNoise.seed);
            totalDelay += .25f;
        }
        
        checkpointNotification.Raise("Creating and initializing " + meshCount + "x" + meshCount + " mesh grid...");
        // yield return new WaitForSeconds(0.25f);
        InitMeshes();
        yield return null;
        
        checkpointNotification.Raise("Generated mesh 0/" + meshCount * meshCount);
        // yield return new WaitForSeconds(.1f);

        int count = 1;
        for (int z = 0; z < meshCount; ++z)
        {
            for (int x = 0; x < meshCount; ++x)
            {
                int index = x + z * meshCount;
                _generators[index].GenerateMesh();
                checkpointNotification.Raise("Generated mesh " + count + "/" + meshCount * meshCount);
                // yield return new WaitForSeconds(.01f);
                ++count;
            }
        }
        
        // GenerateMesh();
        
        checkpointNotification.Raise("Generating height map...");
        // yield return new WaitForSeconds(1);
        _metaData.heightMap = GenerateHeightMap();
        
        checkpointNotification.Raise("Updating metadata...");
        // yield return new WaitForSeconds(1);
        
        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _endTime - _startTime;

        int gridarea = meshCount * meshCount;
        int vertcount = gridarea * Constants.meshSquares;
        _metaData.vertexCount = vertcount;
        _metaData.polyCount = 5;
        _metaData.generationTimeMS = _deltaTime * 1000 - (totalDelay * 1000);

        meshDataNotification.Raise(_metaData);

        yield return null;
    }

    private void InitMeshes()
    {
        _data.dimension = meshCount * Constants.meshSquares;
        
        for (int z = 0; z < meshCount; ++z)
        {
            int zoffset = z * (Constants.meshSquares);
            for (int x = 0; x < meshCount; ++x)
            {
                int xoffset = x * (Constants.meshSquares);
                var newmesh = Instantiate(meshPrefab, new Vector3(xoffset, 0, zoffset), meshPrefab.transform.rotation);
                newmesh.transform.SetParent(transform);
                
                _meshes.Add(newmesh);
                var generator = newmesh.GetComponent<MeshGenerator>();

                generator.xMeshOffset = xoffset;
                generator.zMeshOffset = zoffset;
                generator.row = z;
                generator.column = x;
                
                // generator.whole = meshCount;
                generator.InitData(_data);
                
                _generators.Add(generator);
            }
        }

        float center = (meshCount * Constants.meshSquares) / 2f;
        updateCameraEvent.Raise(new Vector3(center, 0, center));
    }
    
    private void GenerateMesh()
    {
        for (int z = 0; z < meshCount; ++z)
        {
            for (int x = 0; x < meshCount; ++x)
            {
                int index = x + z * meshCount;
                _generators[index].GenerateMesh();
            }
        }
    }

    private Sprite GenerateHeightMap()
    {
        _heightMap.InitMap(meshCount);
        
        for (int y = 0; y < meshCount; ++y)
        {
            for (int x = 0; x < meshCount; ++x)
            {
                // Debug.Log("SETTING CHUNK XOFFSET: " + x + " YOFFSET: " + y);
                var meshgrid = Putils.FlatArrayToTwoDArray(
                    _generators[x + y * meshCount].GetComponent<MeshFilter>().mesh.vertices, Constants.meshVerts,
                    Constants.meshVerts);
                _heightMap.SetChunk(meshgrid, x, y);
            }
        }

        return _heightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
    }

    private void ApplyHeightMapToMesh()
    {
        for (int y = 0; y < meshCount; ++y)
        {
            for (int x = 0; x < meshCount; ++x)
            {
                _generators[x + y * meshCount].UpdateVerts(_heightMap, x, y);
            }
        }
    }
    
    public void RegenerateMesh(MeshGenerationData data)
    {
        Debug.Log(data.ToString());
        
        // bool needToShowVisualVerts = _visualVerts.Count > 0;
        // ShowVisualVertices(false);

        _data = data;
        meshCount = _data.dimension;

        for (int i = _meshes.Count - 1; i >= 0; --i)
        {
            Destroy(_meshes[i].gameObject);
        }
        
        _meshes = new List<GameObject>();
        _generators = new List<MeshGenerator>();

        StartCoroutine(StartMeshGeneration());

        // StartCoroutine(GenerateMesh());

        // if ( needToShowVisualVerts /*&& _vertices.Length < visualVerticeThreshold*/ ) ShowVisualVertices(true);
    }

    public void Blur(BlurMetaData data)
    {
        StartCoroutine(RunBlur(data));
    }

    IEnumerator RunBlur(BlurMetaData data)
    {
        checkpointNotification.Raise("Applying blur with " + data.KernelSize + " kernel size and mode " + data.Mode + "...");
        yield return new WaitForSeconds(0.2f);
        
        _startTime = Time.realtimeSinceStartup;
        
        GaussianBlur.Blur(_heightMap, data, _data.remapMin, _data.remapMax, checkpointNotification);
        
        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _startTime - _endTime;
        _metaData.generationTimeMS = _deltaTime / 1000;
        
        checkpointNotification.Raise("Updating meshes...");
        yield return new WaitForSeconds(1);
        ApplyHeightMapToMesh();
        
        checkpointNotification.Raise("Generating height map preview...");
        yield return new WaitForSeconds(.2f);
        _metaData.heightMap = _heightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
        
        checkpointNotification.Raise("Updating metadata...");
        yield return new WaitForSeconds(.2f);
        meshDataNotification.Raise(_metaData);
    }

    public void RunSimulation(ErosionMetaData data)
    {
        switch (data.algorithm)
        {
            case Enums.ErosionAlgorithms.Hydraulic:
                StartCoroutine(Simulation(data));
                break;
            default:
                break;
        }
    }

    IEnumerator Simulation(ErosionMetaData data)
    {
        _startTime = Time.realtimeSinceStartup;
        yield return HydraulicErosion.Simulate(_heightMap, data.HydraulicErosionParameters, checkpointNotification);
        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _startTime - _endTime;
        _metaData.generationTimeMS = _deltaTime / 1000;

        checkpointNotification.Raise("Updating meshes...");
        yield return new WaitForSeconds(1);
        ApplyHeightMapToMesh();
        
        checkpointNotification.Raise("Generating height map preview...");
        yield return new WaitForSeconds(.2f);
        _metaData.heightMap = _heightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
        
        checkpointNotification.Raise("Updating metadata...");
        yield return new WaitForSeconds(.2f);
        meshDataNotification.Raise(_metaData);
    }
}
