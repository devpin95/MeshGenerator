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
    [FormerlySerializedAs("gaussianBlurGroup")] public GameObject operationGroup;

    [Header("Main Action Buttons")] 
    public GameObject actionButtonsContainer;
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
    public GameObject octavePerlinOptions;
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

    [Header("Stretch Inputs")] 
    public Toggle stretchRemapToggle;
    public TMP_InputField stretchRemapMin;
    public TMP_InputField stretchRemapMax;

    [Header("Perlin Octaves Input")]
    public TMP_InputField octaveSampleMinField;
    public TMP_InputField octaveSampleMaxField;
    
    public Toggle octave1Toggle;
    public TMP_InputField octave1FreqField;
    public TMP_InputField octave1AmpField;
    [Space(11)]
    public Toggle octave2Toggle;
    public TMP_InputField octave2FreqField;
    public TMP_InputField octave2AmpField;
    [Space(11)]
    public Toggle octave3Toggle;
    public TMP_InputField octave3FreqField;
    public TMP_InputField octave3AmpField;
    [Space(11)]
    public Toggle octave4Toggle;
    public TMP_InputField octave4FreqField;
    public TMP_InputField octave4AmpField;
    [Space(11)]
    public Toggle octave5Toggle;
    public TMP_InputField octave5FreqField;
    public TMP_InputField octave5AmpField;
    
    
    [Header("Overlay")] 
    public GameObject overlay;
    public TextMeshProUGUI overlayText;
    
    [Header("Simulations Menu")]
    public TMP_Dropdown simulationsTypeDropdown;

    [Header("Operations Menu")] 
    public TMP_Dropdown operationsTypeDropdown;
    public GameObject gaussianBlurGroup;
    public GameObject stretchGroup;
    public GameObject _activeOperationsMenu;
    
    [Header("Events")] 
    public CEvent_MeshGenerationData generateNewMesh;
    public CEvent_ErosionMetaData erosionSim;
    public CEvent_BlurMetaData blurHeightMap;
    public CEvent_OperationMetaData mapOperation;
    public CEvent switchMap;

    [Header("Objects")] 
    public HeightMapList maps;

    [Header("Previous Mesh")] 
    public CanvasGroup toggleMeshCanvasGroup;
    public GameObject previousMeshContainer;
    public GameObject currentMeshContainer;
    public Button toggleMeshButton;
    private Image toggleMeshButtonImage;
    public bool _previousMeshOn = false;
    public Sprite currentMeshSprite;
    public Sprite previousMeshSprite;
    private bool _previousMeshAvailable = false;
    private Sprite _currentMapPreview = null;
    private Sprite _previousMapPreview = null;

    [Header("Mesh Range Line")] 
    public RectTransform rangeLineContainer;
    public RectTransform rangeLineMinContainer;
    public TextMeshProUGUI rangeLineMinValue;
    public RectTransform rangeLineMaxContainer;
    public TextMeshProUGUI rangeLineMaxValue;
    public TextMeshProUGUI rangeLineMaxRange;
    public TextMeshProUGUI rangeLineMinRange;

    private float inactiveGroupAlpha = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        generateButton.onClick.AddListener(CollectGenerationData);
        runSimulationButton.onClick.AddListener(CollectSimulationData);
        blurMapButton.onClick.AddListener(CollectGaussianBlurData);

        heightMapTypeField.onValueChanged.AddListener(HeightMapTypeChange);
        InitializeDropdown(heightMapTypeField, Enums.HeightMapTypeNames);
        heightMapTypeFieldReady = true;

        InitializeDropdown(simpleNoiseSmoothingField, Enums.SmoothingAlgorithmNames);
        _activeOptionMenu = planeOptions;

        InitializeDropdown(simulationsTypeDropdown, Enums.ErosionSimulationNames);
        InitializeDropdown(gaussianBlurBorderModeDropdown, Enums.GaussianBlurBorderModeNames);
        
        InitializeDropdown(operationsTypeDropdown, Enums.OperationTypeNames);
        
        hydraulicErosionFrictionSlider.onValueChanged.AddListener(arg0 => hydraulicErosionFrictionSliderValue.text = (arg0/100f).ToString("n2"));
        hydraulicErosionDepositionSlider.onValueChanged.AddListener(arg0 => hydraulicErosionDepositionSliderValue.text = (arg0/999f).ToString("n3"));
        hydraulicErosionEvaporationSlider.onValueChanged.AddListener(arg0 => hydraulicErosionEvaporationSliderValue.text = (arg0/999f).ToString("n3"));
        
        errorMessage.text = "";

        toggleMeshButtonImage = toggleMeshButton.GetComponent<Image>();
        toggleMeshButtonImage.sprite = currentMeshSprite;
        toggleMeshCanvasGroup.interactable = false;
        toggleMeshCanvasGroup.alpha = inactiveGroupAlpha;
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
        _currentMapPreview = data.heightMap;
        
        _previousMeshAvailable = data.previousMeshAvailable;
        
        toggleMeshCanvasGroup.interactable = true;
        toggleMeshCanvasGroup.alpha = 1;
        
        SetChildrenActiveStatus(previousMeshContainer, false);
        SetChildrenActiveStatus(currentMeshContainer, true);
        
        // if we were looking at the previous mesh, set the button back to the current mesh values
        if (_previousMeshOn)
        {
            _previousMeshOn = false;
            toggleMeshButtonImage.sprite = currentMeshSprite;
        }

        UpdateRangeLine(data.mapRangeMax, data.mapRangeMin, data.mapMaxVal, data.mapMinVal);
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
        }
        else if ( data.mapType == Enums.HeightMapTypes.PerlinOctaves )
        {
            List<float> freqs = new List<float>();
            List<float> amps = new List<float>();
            float fval;
            bool success;
            int octaveCount = 0;

            success = float.TryParse(octaveSampleMinField.text, out fval);
            if (!success)
            {
                Debug.Log("Sample range min must be a real");
                errorMessage.text = "Sample range min must be a real";
                return;    
            }
            data.octaveNoise.sampleMin = fval;
            
            success = float.TryParse(octaveSampleMaxField.text, out fval);
            if (!success)
            {
                Debug.Log("Sample range max must be a real");
                errorMessage.text = "Sample range max must be a real";
                return;    
            }
            data.octaveNoise.sampleMax = fval;

            if (data.octaveNoise.sampleMax <= data.octaveNoise.sampleMin)
            {
                Debug.Log("Sample max must be greater than sample min");
                errorMessage.text = "Sample max must be greater than sample min";
                return;
            }

            // octave 1 --------------------------------------------------------------------------------------------
            if (octave1Toggle.isOn)
            {
                ++octaveCount;
                
                success = float.TryParse(octave1FreqField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 1 frequency must be a non-negative real");
                    errorMessage.text = "Octave 1 frequency must be a non-negative real";
                    return;
                }
                freqs.Add(fval);
                
                success = float.TryParse(octave1AmpField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 1 amplitude must be a non-negative real");
                    errorMessage.text = "Octave 1 amplitude must be a non-negative real";
                    return;
                }
                amps.Add(fval);
            }
            
            // octave 2 --------------------------------------------------------------------------------------------
            if (octave2Toggle.isOn)
            {
                ++octaveCount;
                
                success = float.TryParse(octave2FreqField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 2 frequency must be a non-negative real");
                    errorMessage.text = "Octave 2 frequency must be a non-negative real";
                    return;
                }
                freqs.Add(fval);
                
                success = float.TryParse(octave2AmpField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 2 amplitude must be a non-negative real");
                    errorMessage.text = "Octave 2 amplitude must be a non-negative real";
                    return;
                }
                amps.Add(fval);
            }
            
            // octave 3 --------------------------------------------------------------------------------------------
            if (octave3Toggle.isOn)
            {
                ++octaveCount;
                
                success = float.TryParse(octave3FreqField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 3 frequency must be a non-negative real");
                    errorMessage.text = "Octave 3 frequency must be a non-negative real";
                    return;
                }
                freqs.Add(fval);
                
                success = float.TryParse(octave3AmpField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 3 amplitude must be a non-negative real");
                    errorMessage.text = "Octave 3 amplitude must be a non-negative real";
                    return;
                }
                amps.Add(fval);
            }
            
            // octave 4 --------------------------------------------------------------------------------------------
            if (octave4Toggle.isOn)
            {
                ++octaveCount;
                
                success = float.TryParse(octave4FreqField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 4 frequency must be a non-negative real");
                    errorMessage.text = "Octave 4 frequency must be a non-negative real";
                    return;
                }
                freqs.Add(fval);
                
                success = float.TryParse(octave4AmpField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 4 amplitude must be a non-negative real");
                    errorMessage.text = "Octave 4 amplitude must be a non-negative real";
                    return;
                }
                amps.Add(fval);
            }
            
            // octave 5 --------------------------------------------------------------------------------------------
            if (octave5Toggle.isOn)
            {
                ++octaveCount;
                
                success = float.TryParse(octave5FreqField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 5 frequency must be a non-negative real");
                    errorMessage.text = "Octave 5 frequency must be a non-negative real";
                    return;
                }
                freqs.Add(fval);
                
                success = float.TryParse(octave5AmpField.text, out fval);
                if (!success || fval < 0)
                {
                    Debug.Log("Octave 5 amplitude must be a non-negative real");
                    errorMessage.text = "Octave 5 amplitude must be a non-negative real";
                    return;
                }
                amps.Add(fval);
            }
            
            if (octaveCount <= 1)
            {
                Debug.Log("Octave noise must have at least 2 active octaves");
                errorMessage.text = "Octave noise must have at least 2 active octaves";
                return;
            }
            
            if (!ParseRemap(data)) return;

            data.octaveNoise.frequencies = freqs.ToArray();
            data.octaveNoise.amplitudes = amps.ToArray();
        }

        errorMessage.text = "";
        generateNewMesh.Raise(data);
        _previousMapPreview = _currentMapPreview;
    }

    private void CollectSimulationData()
    {
        ErosionMetaData data = new ErosionMetaData();
        Enums.ErosionAlgorithms algo = (Enums.ErosionAlgorithms)simulationTypeDropdown.value;

        data.algorithm = algo;
        float fval;
        int ival;
        bool success = false;

        
        Debug.Log("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$   " + algo);
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
        Debug.Log("------------------------------------------------------------------------------");
        errorMessage.text = "";

        SetCurrentContainerActive();
        
        erosionSim.Raise(data);
        _previousMapPreview = _currentMapPreview;
    }

    private void CollectGaussianBlurData()
    {
        OperationMetaData operation = new OperationMetaData();

        Enums.OperationTypes optype = (Enums.OperationTypes) operationsTypeDropdown.value;
        operation.OperationType = optype;

        int oint;
        float ofloat;
        bool success;

        switch (optype)
        {
            // Gaussian blur -------------------------------------------------------------------------------------------
            case Enums.OperationTypes.GaussianBlur:
                BlurMetaData bdata = new BlurMetaData();
                
                success = int.TryParse(gaussianBlurKernelField.text, out oint);

                if (!success || oint <= 0 || oint % 2 == 0)
                {
                    Debug.Log("Kernel size must be a positive, odd integer");
                    errorMessage.text = "Kernel size must be a positive, odd integer";
                    return;
                }
                bdata.KernelSize = oint;
                
                success = float.TryParse(gaussianBlurStandardDevField.text, out ofloat);

                if (!success || ofloat <= 0)
                {
                    Debug.Log("Standard deviation must be positive real");
                    errorMessage.text = "Standard deviation must be position real";
                    return;
                }
                bdata.StandardDeviation = ofloat;
                
                Enums.GaussianBlurBorderModes mode = (Enums.GaussianBlurBorderModes) gaussianBlurBorderModeDropdown.value;
                bdata.Mode = mode;

                operation.blurData = bdata;
                
                break;
            
            // Stretch -------------------------------------------------------------------------------------------------
            case Enums.OperationTypes.Stretch:
                StretchMetaData sdata = new StretchMetaData();

                if (stretchRemapToggle.isOn)
                {
                    Debug.Log("STRETCH REMAP");
                    sdata.Remap = true;
                    
                    success = float.TryParse(stretchRemapMin.text, out ofloat);
                    if (!success)
                    {
                        Debug.Log("Remap min must be a real");
                        errorMessage.text = "Remap min must be a real";
                        return;
                    }
                    sdata.RemapMin = ofloat;
                    
                    success = float.TryParse(stretchRemapMax.text, out ofloat);
                    if (!success)
                    {
                        Debug.Log("Remap max must be a real");
                        errorMessage.text = "Remap max must be a real";
                        return;
                    }
                    sdata.RemapMax = ofloat;

                    if (sdata.RemapMax <= sdata.RemapMin)
                    {
                        Debug.Log("Remap max must be less than remap min");
                        errorMessage.text = "Remap max must be less than remap min";
                        return;
                    }
                }
                else
                {
                    Debug.Log("DONT STRETCH REMAP");
                }

                operation.stretchData = sdata;

                break;
            
            default:
                Debug.Log("Operation not supported");
                errorMessage.text = "Operation not supported";
                return;
        }
        
        
        SetCurrentContainerActive();
        
        mapOperation.Raise(operation);
        _previousMapPreview = _currentMapPreview;
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
            case Enums.HeightMapTypes.Plane: ShowNoisePanel(planeOptions, false);
                break;
            
            case Enums.HeightMapTypes.ImageMap: ShowNoisePanel(imageMapOptions, false);
                break;
            
            case Enums.HeightMapTypes.PerlinNoise: ShowNoisePanel(perlinNoiseOptions, true);
                break;
            
            case Enums.HeightMapTypes.PerlinOctaves: ShowNoisePanel(octavePerlinOptions, true);
                break;
            
            case Enums.HeightMapTypes.SimpleNoise: ShowNoisePanel(simpleNoiseOptions, true);
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
            actionButtonsContainer.SetActive(true);
            // generateButton.gameObject.SetActive(true);
        }
        else
        {
            mainUIContainer.SetActive(false);
            generationStatsContainer.SetActive(false);
            debugContainer.SetActive(false);
            actionButtonsContainer.SetActive(false);
            // generateButton.gameObject.SetActive(false);
        }
    }

    public void OpacitySliderChange(float val)
    {
        float a = Putils.Remap(val, 0, 255, 0, 1);
        mainMenuBg.color = new Color(.8f, .8f, .8f, a);
    }
    
    public void MenuSelectionDropdownChange(int menu)
    {
        if (menu == 0)
        {
            // menus 
            meshGenerationMenuGroup.SetActive(true);
            simulationMenuGroup.SetActive(false);
            operationGroup.SetActive(false);
            
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
            operationGroup.SetActive(false);
            
            // action buttons
            generateButton.gameObject.SetActive(false);
            runSimulationButton.gameObject.SetActive(true);
            blurMapButton.gameObject.SetActive(false);
        }
        else if (menu == 2)
        {
            simulationMenuGroup.SetActive(false);
            meshGenerationMenuGroup.SetActive(false);
            operationGroup.SetActive(true);
            
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

    public void TogglePreviousMesh()
    {
        // if (!_previousMeshAvailable) return;
        
        _previousMeshOn = !_previousMeshOn;
        
        switchMap.Raise();

        if (_previousMeshOn)
        {
            SetChildrenActiveStatus(previousMeshContainer, true);
            SetChildrenActiveStatus(currentMeshContainer, false);
            // previousMeshContainer.SetActive(true);
            // currentMeshContainer.SetActive(false);
            toggleMeshButtonImage.sprite = previousMeshSprite;
            previewImage.sprite = _previousMapPreview;
        }
        else
        {
            SetChildrenActiveStatus(previousMeshContainer, false);
            SetChildrenActiveStatus(currentMeshContainer, true);
            // previousMeshContainer.SetActive(false);
            // currentMeshContainer.SetActive(true);
            toggleMeshButtonImage.sprite = currentMeshSprite;
            previewImage.sprite = _currentMapPreview;
        }
    }

    private void SetCurrentContainerActive()
    {
        // the container is not active but it needs to be to get the event
        if (!currentMeshContainer.activeInHierarchy)
        {
            // deactivate all it's children so that they dont appear
            for (int i = 0; i < currentMeshContainer.transform.childCount; ++i)
            {
                currentMeshContainer.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    private void SetChildrenActiveStatus(GameObject parent, bool status)
    {
        parent.SetActive(true);
        
        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            parent.transform.GetChild(i).gameObject.SetActive(status);
        }
    }

    private void ShowNoisePanel(GameObject panel, bool showRemapOptions)
    {
        if ( _activeOptionMenu ) _activeOptionMenu.SetActive(false);
        panel.SetActive(true);
        _activeOptionMenu = panel;
        remapOptions.SetActive(showRemapOptions);
    }

    private void ShowOperationPanel(GameObject panel)
    {
        if ( _activeOperationsMenu ) _activeOperationsMenu.SetActive(false);
        panel.SetActive(true);
        _activeOperationsMenu = panel;
    }

    public void OperationTypeChange(int val)
    {
        Enums.OperationTypes op = (Enums.OperationTypes) val;
        switch (op)
        {
            case Enums.OperationTypes.Stretch:
                ShowOperationPanel(stretchGroup);
                break;
            case Enums.OperationTypes.GaussianBlur:
                ShowOperationPanel(gaussianBlurGroup);
                break;
        }
    }

    public void EnlargePreview()
    {
        previewImage.rectTransform.localScale = new Vector3(4, 4, 4);
    }

    public void ShrinkPreview()
    {
        previewImage.rectTransform.localScale = new Vector3(1, 1, 1);
    }

    private void UpdateRangeLine(float max, float min, float upper, float lower)
    {
        rangeLineMaxRange.text = max.ToString("n2");
        rangeLineMinRange.text = min.ToString("n2");

        rangeLineMaxValue.text = upper.ToString("n2");
        rangeLineMinValue.text = lower.ToString("n2");

        float containerHeight = rangeLineContainer.rect.height;
        float lowerpos = Putils.Remap(lower, min, max, 0, containerHeight);
        lowerpos -= containerHeight / 2f;
        rangeLineMinContainer.anchoredPosition = new Vector2(0, lowerpos);

        float upperpos = Putils.Remap(upper, min, max, 0, containerHeight);
        upperpos -= containerHeight / 2f;
        rangeLineMaxContainer.anchoredPosition = new Vector2(0, upperpos);
    }
}
