using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Input fields")] 
    public Button generateButton;
    public TMP_InputField dimensionField;
    public TMP_Dropdown heightMapTypeField;
    private bool heightMapTypeFieldReady = false;

    [Header("Perlin Noise Inputs")]
    public TMP_InputField perlinNoiseMinField;
    public TMP_InputField perlinNoiseMaxField;
    
    [Header("Remap Inputs")]
    public TMP_InputField remapMinField;
    public TMP_InputField remapMaxField;

    [Header("Map Type Dropdown")]
    public GameObject imageMapOptions;
    public GameObject planeOptions;
    public GameObject perlinNoiseOptions;
    public GameObject simpleNoiseOptions;
    private GameObject _activeOptionMenu;

    [Header("Events")] 
    public CEvent_MeshGenerationData generateNewMesh;

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
    }

    private void CollectGenerationData()
    {
        MeshGenerationData data = new MeshGenerationData();
        
        bool success;
        
        // Dimension ---------------------------------------------------------------------------------------------------
        int dimension;
        success = int.TryParse(dimensionField.text, out dimension);
        
        if (dimension <= 0)
        {
            Debug.Log("Dimension must be positive.");
            return;
        }
        
        if (!success)
        {
            Debug.Log("Something went wrong - dimension");
            return;
        }
        
        Debug.Log("Generating with dimensions " + dimension + "x" + dimension);
        data.dimension = dimension;

        // Map Type ----------------------------------------------------------------------------------------------------
        MeshGenerator.HeightMapTypes maptype = (MeshGenerator.HeightMapTypes)heightMapTypeField.value;
        Debug.Log("Generating height from " + MeshGenerator.HeightMapTypeNames[maptype]);
        data.mapType = maptype;
        
        // Perlin Min --------------------------------------------------------------------------------------------------
        float perlinMin;
        success = float.TryParse(perlinNoiseMinField.text, out perlinMin);
        
        // if (perlinMin < 0 || perlinMin > 1)
        // {
        //     Debug.Log("Perlin minimum sample must be between 0 and 1");
        //     return;
        // }
        if (!success)
        {
            Debug.Log("Something went wrong - perlin min");
            return;
        }
        
        // Perlin Max --------------------------------------------------------------------------------------------------
        float perlinMax;
        success = float.TryParse(perlinNoiseMaxField.text, out perlinMax);
        
        // if (perlinMax < 0 || perlinMax > 1)
        // {
        //     Debug.Log("Perlin maximum sample must be between 0 and 1");
        //     return;
        // }
        if (!success)
        {
            Debug.Log("Something went wrong - perlin max");
            return;
        }

        if (perlinMin >= perlinMax)
        {
            Debug.Log("Perlin sample min must be less than perlin same max");
            return;
        }

        data.perlinNoiseSampleMin = perlinMin;
        data.perlinNoiseSampleMax = perlinMax;
        
        // Remap Min --------------------------------------------------------------------------------------------------
        float remapMin;
        success = float.TryParse(remapMinField.text, out remapMin);
        
        if (!success)
        {
            Debug.Log("Something went wrong - remap min");
            return;
        }
        
        // Remap Max ---------------------------------------------------------------------------------------------------
        float remapMax;
        success = float.TryParse(remapMaxField.text, out remapMax);
        
        if (!success)
        {
            Debug.Log("Something went wrong - remap max");
            return;
        }

        if (remapMin >= remapMax)
        {
            Debug.Log("Remap min range must be less than the remap max range");
            return;
        }
        
        data.remapMin = remapMin;
        data.remapMax = remapMax;

        generateNewMesh.Raise(data);
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
                break;
            case MeshGenerator.HeightMapTypes.ImageMap:
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                imageMapOptions.SetActive(true);
                _activeOptionMenu = imageMapOptions;
                break;
            case MeshGenerator.HeightMapTypes.PerlinNoise: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                perlinNoiseOptions.SetActive(true);
                _activeOptionMenu = perlinNoiseOptions;
                break;
            case MeshGenerator.HeightMapTypes.SimpleNoise: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                simpleNoiseOptions.SetActive(true);
                _activeOptionMenu = simpleNoiseOptions;
                break;
            default: Debug.Log("How did you get here?");
                break;
        }
    }
}
