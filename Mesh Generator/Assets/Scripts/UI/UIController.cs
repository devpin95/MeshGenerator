using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Button perspectiveButton;
    public TextMeshProUGUI perspectiveLabel;
    public Sprite perspectiveSprite;
    public Sprite orthgraphicSprite;
    private SpriteState _perspectiveModeState;
    private SpriteState _orthographicModeState;
    private bool _inPerspectiveMode = true;

    [Header("Mesh Meta Data")] 
    public TextMeshProUGUI vertCount;
    public TextMeshProUGUI polyCount;
    public TextMeshProUGUI genTime;
    public Image previewImage;

    [Header("Input fields")] 
    public Button generateButton;
    public TMP_InputField dimensionField;
    public TMP_Dropdown heightMapTypeField;
    private bool heightMapTypeFieldReady = false;

    [Header("Perlin Noise Inputs")]
    public TMP_InputField perlinNoiseMinField;
    public TMP_InputField perlinNoiseMaxField;
    public Toggle domainWarpToggle;
    
    [Header("Remap Inputs")]
    public TMP_InputField remapMinField;
    public TMP_InputField remapMaxField;

    [Header("Map Type Dropdown")]
    public GameObject imageMapOptions;
    public GameObject planeOptions;
    public GameObject perlinNoiseOptions;
    public GameObject simpleNoiseOptions;
    public GameObject remapOptions;
    private GameObject _activeOptionMenu;
    
    [Header("Events")] 
    public CEvent_MeshGenerationData generateNewMesh;

    [Header("Objects")] 
    public HeightMapList maps;

    // Start is called before the first frame update
    void Start()
    {
        _perspectiveModeState.selectedSprite = perspectiveSprite;
        _perspectiveModeState.highlightedSprite = perspectiveSprite;
        _perspectiveModeState.pressedSprite = perspectiveSprite;
        _perspectiveModeState.disabledSprite = perspectiveSprite;
        
        _orthographicModeState.selectedSprite = orthgraphicSprite;
        _orthographicModeState.highlightedSprite = orthgraphicSprite;
        _orthographicModeState.pressedSprite = orthgraphicSprite;
        _orthographicModeState.disabledSprite = orthgraphicSprite;
        
        generateButton.onClick.AddListener(CollectGenerationData);

        foreach (var map in MeshGenerator.HeightMapTypeNames)
        {
            heightMapTypeField.options.Add(new TMP_Dropdown.OptionData(map.Value));
        }

        heightMapTypeField.value = 1;
        heightMapTypeField.value = 0;
        heightMapTypeFieldReady = true;

        _activeOptionMenu = planeOptions;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPerspectiveButtonClick()
    {
        _inPerspectiveMode = !_inPerspectiveMode;

        if (_inPerspectiveMode)
        {
            perspectiveButton.spriteState = _perspectiveModeState;
            perspectiveButton.GetComponent<Image>().sprite = perspectiveSprite;
            perspectiveLabel.text = "Perspective";
        }
        else
        {
            perspectiveButton.spriteState = _orthographicModeState;
            perspectiveButton.GetComponent<Image>().sprite = orthgraphicSprite;
            perspectiveLabel.text = "Orthographic";
        }
    }

    public void UpdateMeshMetaData(MeshMetaData data)
    {
        vertCount.text = "Vertices: " + data.vertexCount.ToString("n0");
        polyCount.text = "Polygons: " + data.polyCount.ToString("n0");
        genTime.text = data.generationTimeMS.ToString("n2") + "ms";
        previewImage.sprite = NewLayerPromptController.Tex2dToSprite(data.heightMap);
    }

    private void CollectGenerationData()
    {
        MeshGenerationData data = new MeshGenerationData();
        
        if (!ParseDimensions(data)) return;
        
        ParseMapType(data);

        if (data.mapType == MeshGenerator.HeightMapTypes.PerlinNoise)
        {
            if (!ParsePerlinRange(data)) return;

            data.domainWarp = domainWarpToggle.isOn;
            
            if (!ParseRemap(data)) return;
        }
        else if (data.mapType == MeshGenerator.HeightMapTypes.ImageMap && maps.mapList.Count == 0)
        {
            Debug.Log("Must add at least one height map.");
            return;
        }
        else if (data.mapType == MeshGenerator.HeightMapTypes.SimpleNoise) return;

        generateNewMesh.Raise(data);
    }

    private bool ParseDimensions(MeshGenerationData data)
    {
        bool success;
        
        // Dimension ---------------------------------------------------------------------------------------------------
        int dimension;
        success = int.TryParse(dimensionField.text, out dimension);
        
        if (dimension <= 0)
        {
            Debug.Log("Dimension must be positive.");
            return false;
        }
        
        if (!success)
        {
            Debug.Log("Something went wrong - dimension");
            return false;
        }
        
        Debug.Log("Generating with dimensions " + dimension + "x" + dimension);
        data.dimension = dimension;
        
        return true;
    }

    private void ParseMapType(MeshGenerationData data)
    {
        // Map Type ----------------------------------------------------------------------------------------------------
        MeshGenerator.HeightMapTypes maptype = (MeshGenerator.HeightMapTypes)heightMapTypeField.value;
        Debug.Log("Generating height from " + MeshGenerator.HeightMapTypeNames[maptype]);
        data.mapType = maptype;
    }

    private bool ParsePerlinRange(MeshGenerationData data)
    {
        bool success;
        // Perlin Min --------------------------------------------------------------------------------------------------
        float perlinMin;
        success = float.TryParse(perlinNoiseMinField.text, out perlinMin);
        
        if (!success)
        {
            Debug.Log("Something went wrong - perlin min");
            return false;
        }
        
        // Perlin Max --------------------------------------------------------------------------------------------------
        float perlinMax;
        success = float.TryParse(perlinNoiseMaxField.text, out perlinMax);
        
        if (!success)
        {
            Debug.Log("Something went wrong - perlin max");
            return false;
        }

        if (perlinMin >= perlinMax)
        {
            Debug.Log("Perlin sample min must be less than perlin same max");
            return false;
        }

        data.perlinNoiseSampleMin = perlinMin;
        data.perlinNoiseSampleMax = perlinMax;

        return true;
    }

    private bool ParseRemap(MeshGenerationData data)
    {
        bool success;
        // Remap Min --------------------------------------------------------------------------------------------------
        float remapMin;
        success = float.TryParse(remapMinField.text, out remapMin);
        
        if (!success)
        {
            Debug.Log("Something went wrong - remap min");
            return false;
        }
        
        // Remap Max ---------------------------------------------------------------------------------------------------
        float remapMax;
        success = float.TryParse(remapMaxField.text, out remapMax);
        
        if (!success)
        {
            Debug.Log("Something went wrong - remap max");
            return false;
        }

        if (remapMin >= remapMax)
        {
            Debug.Log("Remap min range must be less than the remap max range");
            return false;
        }
        
        data.remapMin = remapMin;
        data.remapMax = remapMax;

        return true;
    }

    public void HeightMapTypeChange(int maptype)
    {
        if (!heightMapTypeFieldReady) return;
        
        MeshGenerator.HeightMapTypes mapType = (MeshGenerator.HeightMapTypes) maptype;
        switch (mapType)
        {
            case MeshGenerator.HeightMapTypes.Plane: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                planeOptions.SetActive(true);
                _activeOptionMenu = planeOptions;
                remapOptions.SetActive(false);
                break;
            case MeshGenerator.HeightMapTypes.ImageMap:
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                imageMapOptions.SetActive(true);
                _activeOptionMenu = imageMapOptions;
                remapOptions.SetActive(false);
                break;
            case MeshGenerator.HeightMapTypes.PerlinNoise: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                perlinNoiseOptions.SetActive(true);
                _activeOptionMenu = perlinNoiseOptions;
                remapOptions.SetActive(true);
                break;
            case MeshGenerator.HeightMapTypes.SimpleNoise: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                simpleNoiseOptions.SetActive(true);
                _activeOptionMenu = simpleNoiseOptions;
                remapOptions.SetActive(true);
                break;
            default: Debug.Log("How did you get here?");
                break;
        }
    }
}
