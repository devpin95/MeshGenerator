using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageLayerController : MonoBehaviour
{
    public HeightMapList mapList;
    public int childrenHeight = 55;
    public GameObject layerItemPrefab;
    
    private int _numChildren = 0;
    private RectTransform _rectTransform;

    private int layerCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _numChildren = transform.childCount;
        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _numChildren * childrenHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if (_numChildren != transform.childCount)
        {
            _numChildren = transform.childCount;
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _numChildren * childrenHeight);
        }
    }

    public void AddLayerButton(LayerData data)
    {
        Debug.Log("Adding item for " + data.layerName);
        mapList.AddMap(data);
        
        var item = Instantiate(layerItemPrefab, transform.position, layerItemPrefab.transform.rotation);
        item.transform.SetParent(transform);

        item.transform.Find("Image").GetComponent<Image>().sprite = NewLayerPromptController.Tex2dToSprite(data.map);
        
        if (data.layerName.Equals("")) data.layerName = "Layer " + layerCount;
        item.transform.Find("Layer Name").GetComponent<TextMeshProUGUI>().text = data.layerName;
        
        item.transform.Find("Remap").GetComponent<TextMeshProUGUI>().text =
            "(" + data.remapMin + ", " + data.remapMax + ")";
        
        var capturedData = data;
        item.GetComponent<Button>().onClick
            .AddListener(() =>
            {
                Debug.Log("Removing " + capturedData.layerName);
                RemoveLayerButton(capturedData, item);
            });
        
        ++layerCount;
    }

    public void RemoveLayerButton(LayerData data, GameObject item)
    {
        if (mapList.RemoveMap(data))
        {
            Destroy(item);
        }
    }

    private void OnDestroy()
    {
        mapList.ClearList();
    }
}
