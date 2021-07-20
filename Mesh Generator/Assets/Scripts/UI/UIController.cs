using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
    public Toggle invertToggle;

    [Header("Perlin Noise Inputs")]
    public TMP_InputField perlinNoiseMinField;
    public TMP_InputField perlinNoiseMaxField;
    public Toggle domainWarpToggle;
    public CanvasGroup domainWarpOptions;
    public Slider hurstSlider;
    public TextMeshProUGUI hurstValueLabel;

    [Header("Simple Noise Inputs")] 
    public TMP_InputField simpleNoiseLatticeSize;
    public TMP_InputField simpleNoiseMinField;
    public TMP_InputField simpleNoiseMaxField;
    public TMP_Dropdown simpleNoiseSmoothingField;
    public TMP_InputField simpleNoiseFrequencyField;
    [FormerlySerializedAs("simpleNoiseScaleField")] public Slider simpleNoiseScaleSlider;
    public TextMeshProUGUI simpleNoiseScaleLabel;
    public Toggle simpleNoiseRandomToggle;
    public CanvasGroup simpleNoiseRandomCG;
    public TMP_InputField simpleNoiseSeedField;
    
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

    [Header("Overlay")] 
    public GameObject overlay;
    public TextMeshProUGUI overlayText;
    
    [Header("Events")] 
    public CEvent_MeshGenerationData generateNewMesh;

    [Header("Objects")] 
    public HeightMapList maps;

    private float inactiveGroupAlpha = 0.3f;

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

        foreach (var map in Enums.HeightMapTypeNames)
        {
            heightMapTypeField.options.Add(new TMP_Dropdown.OptionData(map.Value));
        }

        heightMapTypeField.value = 1;
        heightMapTypeField.value = 0;
        heightMapTypeFieldReady = true;

        foreach (var algo in Enums.SmoothingAlgorithmNames)
        {
            simpleNoiseSmoothingField.options.Add(new TMP_Dropdown.OptionData(algo.Value));
        }
        
        simpleNoiseSmoothingField.value = 1;
        simpleNoiseSmoothingField.value = 0;

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
        overlay.SetActive(false);
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
        data.invert = invertToggle.isOn;

        // perlin noise ------------------------------------------------------------------------------------------------
        if (data.mapType == Enums.HeightMapTypes.PerlinNoise)
        {
            if (!ParsePerlinRange(data)) return;

            data.perlin.domainWarp = domainWarpToggle.isOn;
            data.perlin.hurst = hurstSlider.value;
            
            if (!ParseRemap(data)) return;
        }
        
        // image map ---------------------------------------------------------------------------------------------------
        else if (data.mapType == Enums.HeightMapTypes.ImageMap && maps.mapList.Count == 0)
        {
            Debug.Log("Must add at least one height map.");
            return;
        }
        
        // simple noise ------------------------------------------------------------------------------------------------
        else if (data.mapType == Enums.HeightMapTypes.SimpleNoise)
        {
            data.simpleNoise.latticeDim = int.Parse(simpleNoiseLatticeSize.text);
            data.simpleNoise.smoothing = (Enums.SmoothingAlgorithms)simpleNoiseSmoothingField.value;
            data.simpleNoise.frequency = int.Parse(simpleNoiseFrequencyField.text);
            data.simpleNoise.scale = simpleNoiseScaleSlider.value;
            data.simpleNoise.random = simpleNoiseRandomToggle.isOn;
            data.simpleNoise.seed = int.Parse(simpleNoiseSeedField.text);
            data.simpleNoise.sampleMin = float.Parse(simpleNoiseMinField.text);
            data.simpleNoise.sampleMax = float.Parse(simpleNoiseMaxField.text);
            
            if (!ParseRemap(data)) return;
        };

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
        Enums.HeightMapTypes maptype = (Enums.HeightMapTypes)heightMapTypeField.value;
        Debug.Log("Generating height from " + Enums.HeightMapTypeNames[maptype]);
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

        data.perlin.sampleMin = perlinMin;
        data.perlin.sampleMax = perlinMax;

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

    // private bool ParseSimpleNoiseLatticeDim(MeshGenerationData data)
    // {
    //     data.simpleNoise.latticeDim = int.Parse(simpleNoiseLatticeSize.text);
    // }
    
    public void HeightMapTypeChange(int maptype)
    {
        if (!heightMapTypeFieldReady) return;
        
        Enums.HeightMapTypes mapType = (Enums.HeightMapTypes) maptype;
        switch (mapType)
        {
            case Enums.HeightMapTypes.Plane: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                planeOptions.SetActive(true);
                _activeOptionMenu = planeOptions;
                remapOptions.SetActive(false);
                break;
            case Enums.HeightMapTypes.ImageMap:
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                imageMapOptions.SetActive(true);
                _activeOptionMenu = imageMapOptions;
                remapOptions.SetActive(false);
                break;
            case Enums.HeightMapTypes.PerlinNoise: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                perlinNoiseOptions.SetActive(true);
                _activeOptionMenu = perlinNoiseOptions;
                remapOptions.SetActive(true);
                break;
            case Enums.HeightMapTypes.SimpleNoise: 
                if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
                simpleNoiseOptions.SetActive(true);
                _activeOptionMenu = simpleNoiseOptions;
                remapOptions.SetActive(true);
                break;
            default: Debug.Log("How did you get here?");
                break;
        }
    }

    public void ShowGenerationMessage(string message)
    {
        overlay.SetActive(true);
        overlayText.text = message;
    }

    public void UpdateHurstExponentLabel(float val)
    {
        hurstValueLabel.text = val.ToString("f2");
    }

    public void ToggleDomainWarpOptions(bool state)
    {
        if (state)
        {
            domainWarpOptions.alpha = 1;
            domainWarpOptions.interactable = true;
        }
        else
        {
            domainWarpOptions.alpha = inactiveGroupAlpha;
            domainWarpOptions.interactable = false;
        }
    }

    public void ValidateSimpleNoiseLatticeDim(string str)
    {
        if (str == "")
        {
            str = "1";
        }
        else
        {
            int val = 0;
            bool success = int.TryParse(str, out val);

            if (success)
            {
                if (val == 0)
                {
                    simpleNoiseLatticeSize.text = "1";
                }
                else if (val < 0)
                {
                    val = -val;
                    simpleNoiseLatticeSize.text = val.ToString();
                }
            }
        }
    }
    
    public void ValidateSimpleNoiseFrequency(string str)
    {
        if (str == "")
        {
            str = "1";
        }
        else
        {
            str = MakeStringPositiveInteger(str);
        }

        simpleNoiseFrequencyField.text = str;
    }

    public void ValidateMeshDim(string str)
    {
        const int min = 2;
        const int max = 255;
        
        int val;
        bool success = int.TryParse(str, out val);

        if (success)
        {
            if (val < 1 && val >= 0)
            {
                dimensionField.text = min.ToString();
            } else if (val < 0)
            {
                val = -val;
                dimensionField.text = val.ToString();
            } else if (val > max)
            {
                dimensionField.text = max.ToString();
            }
        }
        else
        {
            dimensionField.text = "4";
        }
    }

    private string MakeStringPositiveInteger(string str)
    {
        int val = 1;
        bool success = int.TryParse(str, out val);

        if (success)
        {
            if (val < 0)
            {
                val = -val;
            }   
            return val.ToString();
        }
        else
        {
            return "";
        }
    }

    public void ToggleSimpleNoiseRandom(bool s)
    {
        if (s)
        {
            simpleNoiseRandomCG.interactable = false;
            simpleNoiseRandomCG.alpha = inactiveGroupAlpha;   
        }
        else
        {
            simpleNoiseRandomCG.interactable = true;
            simpleNoiseRandomCG.alpha = 1;
        }
    }

    public void UpdateSimpleNoiseScaleLabel(float val)
    {
        simpleNoiseScaleLabel.text = val.ToString("n2");
    }
}