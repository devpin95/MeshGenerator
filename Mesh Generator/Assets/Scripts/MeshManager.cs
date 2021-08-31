using System;
using System.Collections;
using System.Collections.Generic;
using Parameters;
using UnityEditorInternal;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public GameObject limitPrefab;
    
    public GameObject previousMeshContainer;
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
    
    private HeightMap _activeHeightMap = new HeightMap();
    private HeightMap _previousHeightMap = null;
    private bool _usePreviousMap = false;

    // Start is called before the first frame update
    void Start()
    {
        // meshOffest = spacing * 255f;
        _metaData.previousMeshAvailable = false;
        StartCoroutine(StartMeshGeneration());
        _activeHeightMap.globalParams = globalParameters;
        _activeHeightMap.mapRangeMin = _data.remapMin;
        _activeHeightMap.mapRangeMax = _data.remapMax;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartMeshGeneration()
    {
        _startTime = Time.realtimeSinceStartup;

        if (_data.mapType == Enums.HeightMapTypes.SimpleNoise)
        {
            checkpointNotification.Raise("Generating " + _data.simpleNoise.latticeDim + "x" + _data.simpleNoise.latticeDim + " lattice with seed " + _data.simpleNoise.seed + "...");
            yield return new WaitForSeconds(0.1f);
            Lattice.Instance.GenerateLattice(_data.simpleNoise.latticeDim, _data.simpleNoise.seed);
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


        checkpointNotification.Raise("Generating height map...");
        // yield return new WaitForSeconds(1);
        _metaData.heightMap = GenerateHeightMap();

        checkpointNotification.Raise("Updating metadata...");
        // yield return new WaitForSeconds(1);
        
        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _endTime - _startTime;

        int gridarea = meshCount * meshCount;
        int vertcount = gridarea * Constants.meshVerts * Constants.meshVerts;
        _metaData.vertexCount = vertcount;
        _metaData.polyCount = Constants.meshSquares * 2 * 254 * meshCount;
        _metaData.generationTimeMS = _deltaTime * 1000;
        
        _metaData.mapMinVal = _activeHeightMap.mapMinVal;
        _metaData.mapMaxVal = _activeHeightMap.mapMaxVal;
        _metaData.mapRangeMin = _activeHeightMap.mapRangeMin;
        _metaData.mapRangeMax = _activeHeightMap.mapRangeMax;

        meshDataNotification.Raise(_metaData);
        
        _usePreviousMap = false;

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
        _activeHeightMap.InitMap(meshCount);
        _activeHeightMap.InitMinMax();
        
        for (int y = 0; y < meshCount; ++y)
        {
            for (int x = 0; x < meshCount; ++x)
            {
                // Debug.Log("SETTING CHUNK XOFFSET: " + x + " YOFFSET: " + y);
                var meshgrid = Putils.FlatArrayToTwoDArray(
                    _generators[x + y * meshCount].GetComponent<MeshFilter>().mesh.vertices, Constants.meshVerts,
                    Constants.meshVerts);
                _activeHeightMap.SetChunk(meshgrid, x, y);
            }
        }

        return _activeHeightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
    }

    private void ApplyHeightMapToMesh()
    {
        for (int y = 0; y < meshCount; ++y)
        {
            for (int x = 0; x < meshCount; ++x)
            {
                _generators[x + y * meshCount].UpdateVerts(_activeHeightMap, x, y);
            }
        }
    }
    
    public void RegenerateMesh(MeshGenerationData data)
    {
        _data = data;
        meshCount = _data.dimension;

        for (int i = _meshes.Count - 1; i >= 0; --i)
        {
            Destroy(_meshes[i].gameObject);
        }

        MoveMeshToPrevious(true);
        _metaData.previousMeshAvailable = true;
        SavePreviousMap();
        
        _meshes = new List<GameObject>();
        _generators = new List<MeshGenerator>();

        if (data.mapType == Enums.HeightMapTypes.PerlinOctaves)
        {
            data.needsRemap = true;
            float sum = 0;

            for (int i = 0; i < _data.octaveNoise.amplitudes.Length; ++i)
            {
                sum += _data.octaveNoise.amplitudes[i];
            }

            data.octaveNoise.max = sum;
        }
        else data.needsRemap = false;

        _activeHeightMap.InitMinMax();
        _activeHeightMap.mapRangeMin = data.remapMin;
        _activeHeightMap.mapRangeMax = data.remapMax;
        
        StartCoroutine(StartMeshGeneration());
        
    }

    // public void Blur(BlurMetaData data)
    // {
    //     _metaData.previousMeshAvailable = true;
    //     if (_usePreviousMap) SwitchMap();
    //     SavePreviousMap();
    //     
    //     StartCoroutine(RunBlur(data));
    // }

    IEnumerator RunBlur(BlurMetaData data)
    {
        if (!_usePreviousMap)
        {
            checkpointNotification.Raise("Saving current mesh...");
            yield return new WaitForSeconds(.1f);
            MoveMeshToPrevious(true);
        }

        checkpointNotification.Raise("Applying blur with " + data.KernelSize + " kernel size and mode " + data.Mode + "...");

        _startTime = Time.realtimeSinceStartup;
        
        var blurredgrid = GaussianBlur.Blur(_activeHeightMap.GetHeightMap(), _activeHeightMap.WidthAndHeight(), data, _data.remapMin, _data.remapMax, checkpointNotification);
        
        
        checkpointNotification.Raise("Updating height map...");
        yield return new WaitForSeconds(.1f);

        _activeHeightMap.SetMapHeights(blurredgrid, _data.remapMin, _data.remapMax);
        
        checkpointNotification.Raise("Updating meshes...");
        yield return new WaitForSeconds(1);
        ApplyHeightMapToMesh();
        
        checkpointNotification.Raise("Generating height map preview...");
        yield return new WaitForSeconds(.1f);
        _metaData.heightMap = _activeHeightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
        
        checkpointNotification.Raise("Updating metadata...");
        yield return new WaitForSeconds(.1f);

        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _endTime - _startTime;
        _metaData.generationTimeMS = _deltaTime * 1000;
        
        _metaData.mapMinVal = _activeHeightMap.mapMinVal;
        _metaData.mapMaxVal = _activeHeightMap.mapMaxVal;
        _metaData.mapRangeMin = _activeHeightMap.mapRangeMin;
        _metaData.mapRangeMax = _activeHeightMap.mapRangeMax;
        
        meshDataNotification.Raise(_metaData);
        
        _usePreviousMap = false;
    }

    IEnumerator StretchMap(StretchMetaData data)
    {
        if (!_usePreviousMap)
        {
            checkpointNotification.Raise("Saving current mesh...");
            yield return new WaitForSeconds(.01f);
            MoveMeshToPrevious(true);
        }
        
        _startTime = Time.realtimeSinceStartup;
        
        bool workDone = false;
        
        checkpointNotification.Raise("Checking map limits...");
        yield return new WaitForSeconds(.01f);

        // create a tuple for the map, map min val, and map max val
        // then set the map to the height map
        (float[,], float, float) newmap;
        newmap.Item1 = _activeHeightMap.GetHeightMap();

        // check if the min/max values of the map are already close enough to the target map range
        if (!Putils.ValueWithinRange(_activeHeightMap.mapMinVal, _activeHeightMap.mapRangeMin, 0.000001f) &&
            !Putils.ValueWithinRange(_activeHeightMap.mapMaxVal, _activeHeightMap.mapRangeMax, 0.000001f))
        {
            checkpointNotification.Raise("Stretching height map...");
            yield return new WaitForSeconds(.01f);

            _startTime = Time.realtimeSinceStartup;

            // do the stretch
            newmap = MapStretch.Stretch(newmap.Item1, _activeHeightMap.WidthAndHeight(), _activeHeightMap.mapMinVal, _activeHeightMap.mapMaxVal);
            
            // set the height map's new min and max stored in Item2 and Item3
            _activeHeightMap.mapMinVal = newmap.Item2;
            _activeHeightMap.mapMaxVal = newmap.Item3;

            workDone = true;
        }
        else
        {
            // show a message that we are not going to stretch the map
            checkpointNotification.Raise("Map already stretched...");
            yield return new WaitForSeconds(.01f);
        }
        
        // check if we need to remap the height values
        if (data.Remap)
        {
            Debug.Log("Remaping to " + data.RemapMin + ", " + data.RemapMax);
            checkpointNotification.Raise("Remapping height map...");
            yield return new WaitForSeconds(.01f);

            // reset the height map
            _data.remapMin = data.RemapMin;
            _data.remapMax = data.RemapMax;
            _activeHeightMap.mapRangeMin = data.RemapMin;
            _activeHeightMap.mapRangeMax = data.RemapMax;

            newmap.Item1 = MapStretch.RemapHeights(newmap.Item1, _activeHeightMap.WidthAndHeight(), _activeHeightMap.mapMinVal, _activeHeightMap.mapMaxVal, data.RemapMin, data.RemapMax);
            workDone = true;
        }

        // only update the height map if we actually did something to it
        if (workDone)
        {
            checkpointNotification.Raise("No work done...");
            yield return new WaitForSeconds(.01f);

            checkpointNotification.Raise("Updating height map...");
            yield return new WaitForSeconds(.01f);
            _activeHeightMap.SetMapHeights(newmap.Item1, _data.remapMin, _data.remapMax);
        
            checkpointNotification.Raise("Updating meshes...");
            yield return new WaitForSeconds(.01f);
            ApplyHeightMapToMesh();
        
            checkpointNotification.Raise("Generating height map preview...");
            yield return new WaitForSeconds(.01f);
            _metaData.heightMap = _activeHeightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
        }
        
        
        checkpointNotification.Raise("Updating metadata...");
        yield return new WaitForSeconds(.01f);

        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _endTime - _startTime;
        _metaData.generationTimeMS = _deltaTime * 1000;

        _metaData.mapMinVal = _activeHeightMap.mapMinVal;
        _metaData.mapMaxVal = _activeHeightMap.mapMaxVal;
        _metaData.mapRangeMin = _activeHeightMap.mapRangeMin;
        _metaData.mapRangeMax = _activeHeightMap.mapRangeMax;

        meshDataNotification.Raise(_metaData);

    }

    public void Simulate(ErosionMetaData data)
    {
        _metaData.previousMeshAvailable = true;
        if (_usePreviousMap) SwitchMap();
        SavePreviousMap();
        
        switch (data.algorithm)
        {
            case Enums.ErosionAlgorithms.Hydraulic:
                StartCoroutine(RunSimulation(data));
                break;
            default:
                break;
        }
    }

    IEnumerator RunSimulation(ErosionMetaData data)
    {
        if (!_usePreviousMap)
        {
            checkpointNotification.Raise("Saving current mesh...");
            yield return new WaitForSeconds(.1f);
            MoveMeshToPrevious(true);
        }
        
        _startTime = Time.realtimeSinceStartup;
        yield return HydraulicErosion.Simulate(_activeHeightMap, data.HydraulicErosionParameters, checkpointNotification);

        checkpointNotification.Raise("Updating meshes...");
        yield return new WaitForSeconds(.1f);
        ApplyHeightMapToMesh();
        
        checkpointNotification.Raise("Generating height map preview...");
        yield return new WaitForSeconds(.1f);
        _metaData.heightMap = _activeHeightMap.GenerateHeightMapPreview(_data.remapMin, _data.remapMax);
        
        checkpointNotification.Raise("Updating metadata...");
        yield return new WaitForSeconds(.1f);
        meshDataNotification.Raise(_metaData);
        
        _endTime = Time.realtimeSinceStartup;
        _deltaTime = _endTime - _startTime;
        _metaData.generationTimeMS = _deltaTime * 1000;
        
        _metaData.mapMinVal = _activeHeightMap.mapMinVal;
        _metaData.mapMaxVal = _activeHeightMap.mapMaxVal;
        _metaData.mapRangeMin = _activeHeightMap.mapRangeMin;
        _metaData.mapRangeMax = _activeHeightMap.mapRangeMax;

        _usePreviousMap = false;
    }

    public void Operation(OperationMetaData data)
    {
        _metaData.previousMeshAvailable = true;
        if (_usePreviousMap) SwitchMap();
        SavePreviousMap();

        switch (data.OperationType)
        {
            case Enums.OperationTypes.Stretch:
                Debug.Log("Running Stretch operation");
                StartCoroutine(StretchMap(data.stretchData));
                break;
            case Enums.OperationTypes.GaussianBlur:
                StartCoroutine(RunBlur(data.blurData));
                break;
            default:
                Debug.Log("How did you get here?");
                break;
        }
    }

    private void SwitchMap()
    {
        _activeHeightMap = _previousHeightMap;
        _metaData.previousMeshAvailable = false;
    }

    private void SavePreviousMap()
    {
        _previousHeightMap = _activeHeightMap;
    }

    public void SetActiveMap()
    {
        _usePreviousMap = !_usePreviousMap;
        Debug.Log("Use previous map: " + _usePreviousMap);
    }

    private void MoveMeshToPrevious(bool copy = false)
    {
        if (previousMeshContainer.transform.childCount > 0)
        {
            int childcount = previousMeshContainer.transform.childCount;
            for (int i = childcount - 1; i >= 0; --i)
            {
                DestroyImmediate(previousMeshContainer.transform.GetChild(i).gameObject);
            }
        }
        
        foreach (var mesh in _meshes)
        {
            if (copy)
            {
                Debug.Log("Copying mesh...");

                var nmesh = Instantiate(mesh, mesh.transform.position, mesh.transform.rotation);
                nmesh.transform.SetParent(previousMeshContainer.transform);

                Mesh omeshFilter = mesh.GetComponent<MeshFilter>().mesh;
                Mesh nmeshfilter = nmesh.GetComponent<MeshFilter>().mesh;
                
                nmeshfilter = new Mesh();
                nmeshfilter.vertices = omeshFilter.vertices;
                nmeshfilter.triangles = omeshFilter.triangles;
                nmeshfilter.colors = omeshFilter.colors;
                nmeshfilter.RecalculateNormals();
            }
            else
            {
                // the mesh will be regenerated so just move this one
                Debug.Log("Moving mesh...");
                mesh.transform.SetParent(previousMeshContainer.transform);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 upper = new Vector3(0, _data.remapMax, 0);
        Vector3 lower = new Vector3(0, _data.remapMin, 0);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(upper, 1);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(lower, 1);
    }
}
