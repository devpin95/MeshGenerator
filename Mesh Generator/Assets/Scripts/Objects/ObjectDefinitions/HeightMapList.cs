using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Objects/Height Map List")]
public class HeightMapList : ScriptableObject
{
    [FormerlySerializedAs("maps")] public List<LayerData> mapList = new List<LayerData>();

    public void AddMap(LayerData data)
    {
        mapList.Add(data);
    }

    public bool RemoveMap(LayerData data)
    {
        return mapList.Remove(data);
    }

    public void ClearList()
    {
        mapList.Clear();
    }
}
