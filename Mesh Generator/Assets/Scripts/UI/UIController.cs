using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Parameters;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Menu Groups")] 
    public Image mainMenuBg;
    public GameObject meshGenerationMenuGroup;
    public GameObject simulationMenuGroup;
    public GameObject gaussianBlurGroup;
    
    [Header("Main Action Buttons")]
    public Button generateButton;
    public Button runSimulationButton;
    public Button blurMapButton;
    public TextMeshProUGUI errorMessage;

    [Header("Mesh Meta Data")] 
    public TextMeshProUGUI vertCount;
    public TextMeshProUGUI polyCount;
    public TextMeshProUGUI genTime;
    public Image previewImage;

    [Header("Input fields")]
    public TMP_InputField dimensionField;
    public TMP_Dropdown heightMapTypeField;
    private bool heightMapTypeFieldReady = false;
    public Toggle invertToggle;
    public GameObject mainUIContainer;
    public GameObject generationStatsContainer;
    public GameObject debugContainer;

    [Header("Perlin Noise Inputs")]
    public TMP_InputField perlinNoiseMinField;
    public TMP_InputField perlinNoiseMaxField;
    public Toggle domainWarpToggle;
    public CanvasGroup domainWarpOptions;
    public Slider hurstSlider;
    public TextMeshProUGUI hurstValueLabel;
    public Toggle perlinRidgedToggle;

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

    [Header("Simulations Inputs")] 
    public TMP_Dropdown simulationTypeDropdown;

    [Header("Hydraulic Erosion Inputs")] 
    public TMP_InputField hydraulicErosionTimeStepField;
    public TMP_InputField hydraulicErosionDropCountField;
    public TMP_InputField hydraulicErosionStartingVolumeField;
    public TMP_InputField hydraulicErosionMinVolumeField;
    public TMP_InputField hydraulicErosionDensityField;
    public Slider hydraulicErosionDepositionSlider;
    public TextMeshProUGUI hydraulicErosionDepositionSliderValue;
    public Slider hydraulicErosionEvaporationSlider;
    public TextMeshProUGUI hydraulicErosionEvaporationSliderValue;
    public Toggle hydraulicErosionFlipXYToggle;
    public Slider hydraulicErosionFrictionSlider;
    public TextMeshProUGUI hydraulicErosionFrictionSliderValue;

    [Header("Gaussian Blur Inputs")] 
    public TMP_InputField gaussianBlurKernelField;
    public TMP_InputField gaussianBlurStandardDevField;
    public TMP_Dropdown gaussianBlurBorderModeDropdown;

    [Header("Overlay")] 
    public GameObject overlay;
    public TextMeshProUGUI overlayText;
    
    [Header("Simulations Menu")]
    public TMP_Dropdown simulationsTypeDropdown;
    
    [Header("Events")] 
    public CEvent_MeshGenerationData generateNewMesh;
    public CEvent_ErosionMetaData erosionSim;
    public CEvent_BlurMetaData blurHeightMap;

    [Header("Objects")] 
    public HeightMapList maps;

    private float inactiveGroupAlpha = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        generateButton.onClick.AddListener(CollectGenerationData);
        runSimulationButton.onClick.AddListener(CollectSimulationData);
        blurMapButton.onClick.AddListener(CollectGaussianBlurData);

        InitializeDropdown(heightMapTypeField, Enums.HeightMapTypeNames);
        heightMapTypeFieldReady = true;

        InitializeDropdown(simpleNoiseSmoothingField, Enums.SmoothingAlgorithmNames);
        _activeOptionMenu = planeOptions;

        InitializeDropdown(simulationsTypeDropdown, Enums.ErosionSimulationNames);
        InitializeDropdown(gaussianBlurBorderModeDropdown, Enums.GaussianBlurBorderModeNames);
        
        hydraulicErosionFrictionSlider.onValueChanged.AddListener(arg0 => hydraulicErosionFrictionSliderValue.text = (arg0/100f).ToString("n2"));
        hydraulicErosionDepositionSlider.onValueChanged.AddListener(arg0 => hydraulicErosionDepositionSliderValue.text = (arg0/999f).ToString("n3"));
        hydraulicErosionEvaporationSlider.onValueChanged.AddListener(arg0 => hydraulicErosionEvaporationSliderValue.text = (arg0/999f).ToString("n3"));
        
        errorMessage.text = "";
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void UpdateMeshMetaData(MeshMetaData data)
    {
        overlay.SetActive(false);
        vertCount.text = "Vertices: " + data.vertexCount.ToString("n0");
        polyCount.text = "Polygons: " + data.polyCount.ToString("n0");
        genTime.text = data.generationTimeMS.ToString("n2") + "ms";
        previewImage.sprite = data.heightMap;
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
            data.perlin.ridged = perlinRidgedToggle.isOn;
            
            if (!ParseRemap(data)) return;
        }
        
        // image map ---------------------------------------------------------------------------------------------------
        else if (data.mapType == Enums.HeightMapTypes.ImageMap && maps.mapList.Count == 0)
        {
            Debug.Log("Must add at least one height map.");
            errorMessage.text = "Must add at least one height map.";
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

        errorMessage.text = "";
        generateNewMesh.Raise(data);
    }

    private void CollectSimulationData()
    {
        ErosionMetaData data = new ErosionMetaData();
        Enums.ErosionAlgorithms algo = (Enums.ErosionAlgorithms)simulationTypeDropdown.value;

        data.algorithm = algo;
        float fval;
        int ival;
        bool success = false;

        switch (algo)
        {
            case Enums.ErosionAlgorithms.Hydraulic:
                HydraulicErosionParameters hydparams = new HydraulicErosionParameters();
                
                success = float.TryParse(hydraulicErosionTimeStepField.text, out fval);
                if (!success || fval <= 0)
                {
                    Debug.Log("Time step must be a positive, non-zero integer");
                    errorMessage.text = "Time step must be a positive, non-zero integer";
                    return;
                }
                hydparams.DT = fval;
                
                success = int.TryParse(hydraulicErosionDropCountField.text, out ival);
                if (!success || ival < 0)
                {
                    Debug.Log("Drop count must be a positive, non-zero integer");
                    errorMessage.text = "Drop count must be a positive, non-zero integer";
                    return;
                }
                hydparams.DropCount = ival;

                success = float.TryParse(hydraulicErosionStartingVolumeField.text, out fval);
                if (!success || fval <= 0)
                {
                    Debug.Log("Starting drop volume must be greater than 0");
                    errorMessage.text = "Starting drop volume must be greater than 0";
                    return;
                }
                hydparams.StartingVolume = fval;
                
                success = float.TryParse(hydraulicErosionMinVolumeField.text, out fval);
                if (!success || fval <= 0)
                {
                    Debug.Log("Minimum drop volume must be greater than 0");
                    errorMessage.text = "Minimum drop volume must be greater than 0";
                    return;
                }
                hydparams.MINVolume = fval;
                
                success = float.TryParse(hydraulicErosionDensityField.text, out fval);
                if (!success || fval <= 0)
                {
                    Debug.Log("Density must be greater than 0");
                    errorMessage.text = "Density must be greater than 0";
                    return;
                }
                hydparams.Density = fval;

                hydparams.DepositeRate = hydraulicErosionDepositionSlider.value/999f;
                hydparams.EvaporationRate = hydraulicErosionEvaporationSlider.value/999f;
                hydparams.Friction = hydraulicErosionFrictionSlider.value/100f;
                hydparams.FlipXY = hydraulicErosionFlipXYToggle.isOn;
                
                data.HydraulicErosionParameters = hydparams;

                break;
            
            default:
                Debug.Log("Must select a simulation to run...");
                errorMessage.text = "Must select a simulation to run...";
                return;
        }
        
        errorMessage.text = "";
        erosionSim.Raise(data);
    }

    private void CollectGaussianBlurData()
    {
        BlurMetaData data = new BlurMetaData();
        
        bool success;
        int kernelSize = 1;
        success = int.TryParse(gaussianBlurKernelField.text, out kernelSize);

        if (!success || kernelSize <= 0 || kernelSize % 2 == 0)
        {
            Debug.Log("Kernel size must be a positive, odd integer");
            errorMessage.text = "Kernel size must be a positive, odd integer";
            return;
        }

        data.KernelSize = kernelSize;
        
        float stdev = 1;
        success = float.TryParse(gaussianBlurStandardDevField.text, out stdev);

        if (!success || stdev <= 0)
        {
            Debug.Log("Standard deviation must be positive real");
            errorMessage.text = "Standard deviation must be position real";
            return;
        }

        data.StandardDeviation = stdev;
        

        Enums.GaussianBlurBorderModes mode = (Enums.GaussianBlurBorderModes) gaussianBlurBorderModeDropdown.value;
        data.Mode = mode;
        
        blurHeightMap.Raise(data);
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
            errorMessage.text = "Dimension must be positive.";
            return false;
        }
        
        if (!success)
        {
            Debug.Log("Something went wrong - dimension");
            errorMessage.text = "Something went wrong - dimension";
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
            errorMessage.text = "Something went wrong - perlin min";
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
            errorMessage.text = "Something went wrong - remap min";
            return false;
        }
        
        // Remap Max ---------------------------------------------------------------------------------------------------
        float remapMax;
        success = float.TryParse(remapMaxField.text, out remapMax);
        
        if (!success)
        {
            Debug.Log("Something went wrong - remap max");
            errorMessage.text = "Something went wrong - remap max";
            return false;
        }

        if (remapMin >= remapMax)
        {
            Debug.Log("Remap min range must be less than the remap max range");
            errorMessage.text = "Remap min range must be less than the remap max range";
            return false;
        }
        
        data.remapMin = remapMin;
        data.remapMax = remapMax;

        return true;
    }
    
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

    public void ToggleHud(bool s)
    {
        if (s)
        {
            mainUIContainer.SetActive(true);
            generationStatsContainer.SetActive(true);
            debugContainer.SetActive(true);
            generateButton.gameObject.SetActive(true);
        }
        else
        {
            mainUIContainer.SetActive(false);
            generationStatsContainer.SetActive(false);
            debugContainer.SetActive(false);
            generateButton.gameObject.SetActive(false);
        }
    }

    public void OpacitySliderChange(float val)
    {
        float a = Putils.Remap(val, 0, 255, 0, 1);
        mainMenuBg.color = new Color(1f, 1f, 1f, a);
    }
    
    public void MenuSelectionDropdownChange(int menu)
    {
        if (menu == 0)
        {
            // menus 
            meshGenerationMenuGroup.SetActive(true);
            simulationMenuGroup.SetActive(false);
            gaussianBlurGroup.SetActive(false);
            
            // action buttons
            generateButton.gameObject.SetActive(true);
            runSimulationButton.gameObject.SetActive(false);
            blurMapButton.gameObject.SetActive(false);
        } 
        else if (menu == 1)
        {
            // menus
            meshGenerationMenuGroup.SetActive(false);
            simulationMenuGroup.SetActive(true);
            gaussianBlurGroup.SetActive(false);
            
            // action buttons
            generateButton.gameObject.SetActive(false);
            runSimulationButton.gameObject.SetActive(true);
            blurMapButton.gameObject.SetActive(false);
        }
        else if (menu == 2)
        {
            simulationMenuGroup.SetActive(false);
            meshGenerationMenuGroup.SetActive(false);
            gaussianBlurGroup.SetActive(true);
            
            // action buttons
            generateButton.gameObject.SetActive(false);
            runSimulationButton.gameObject.SetActive(false);
            blurMapButton.gameObject.SetActive(true);
        }
    }

    private void InitializeDropdown<T>(TMP_Dropdown dropdown, Dictionary<T, string> list)
    {
        foreach (var mode in list)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(mode.Value));
        }
        dropdown.value = 1;
        dropdown.value = 0;
    }
}
