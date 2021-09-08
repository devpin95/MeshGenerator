using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewLayerPromptController : MonoBehaviour
{
    [Header("Image Layer Options")] 
    public GameObject newLayerPromptContainer;
    public Button createLayerButton;
    
    [Header("Image Preview")]
    public Button promptOpenExplorerButton;
    public Image imagePreview;
    
    [Header("Actions")]
    public Button promptCancelButton;
    public Button promptAddButton;
    public TextMeshProUGUI warningText;
    
    [Header("Remap")]
    public TMP_InputField promptRemapMin;
    public TMP_InputField promptRemapMax;
    public TMP_InputField layerName;

    [Header("Events")]
    public CEvent_LayerData addLayerNotification;

    private Texture2D map = null;
    private LayerData _layerData = new LayerData();
    private LayerData _oldLayerData;
    private string path = "";
    
    // Start is called before the first frame update
    void Start()
    {
        createLayerButton.onClick.AddListener(ShowNewLayerPrompt);
        promptOpenExplorerButton.onClick.AddListener(OpenExplorer);
        promptCancelButton.onClick.AddListener(HideNewLayerPrompt);
        promptAddButton.onClick.AddListener(AddMap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OpenExplorer()
    {
        // https://github.com/gkngkc/UnityStandaloneFileBrowser
        
        // Open file with filter
        var extensions = new [] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length > 0)
        {
            path = paths[0];
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            imagePreview.color = new Color(255, 255, 255, 255);
            imagePreview.sprite = Tex2dToSprite(tex);
            map = tex;
        }
    }

    
    public void ShowNewLayerPrompt()
    {
        if (newLayerPromptContainer.activeInHierarchy) return;

        promptRemapMin.text = "-1";
        promptRemapMax.text = "1";

        map = null;
        imagePreview.sprite = null;
        imagePreview.color = new Color(0, 0, 0, 0);

        warningText.text = "";
        
        newLayerPromptContainer.SetActive(true);
    }
    
    public void HideNewLayerPrompt()
    {
        newLayerPromptContainer.SetActive(false);
    }

    public void AddMap()
    {
        LayerData newLayer = new LayerData();
        
        if (map == null)
        {
            warningText.text = "Select an image to import";
            return;
        }

        float remapMin = float.Parse(promptRemapMin.text);
        float remapMax = float.Parse(promptRemapMax.text);

        if (remapMin >= remapMax)
        {
            warningText.text = "Min must be less than max";
            return;
        }
        
        newLayer.layerName = layerName.text;
        newLayer.map = map;
        newLayer.remapMin = remapMin;
        newLayer.remapMax = remapMax;
        newLayer.queryType = LayerData.QueryType.Add;
        
        addLayerNotification.Raise(newLayer);
        
        // Debug.Log("Adding texture map from " + path);

        HideNewLayerPrompt();
    }

    public static Sprite Tex2dToSprite(Texture2D tex)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
