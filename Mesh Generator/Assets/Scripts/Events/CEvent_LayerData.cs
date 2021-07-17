using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CEvent <LayerData>")]
public class CEvent_LayerData : ScriptableObject
{
    private List<CEventListener_LayerData> listeners = new List<CEventListener_LayerData>();

    public void Raise(LayerData data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(data);
        }
    }

    public void RegisterListener(CEventListener_LayerData listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_LayerData listener)
    {
        listeners.Remove(listener);
    }
}