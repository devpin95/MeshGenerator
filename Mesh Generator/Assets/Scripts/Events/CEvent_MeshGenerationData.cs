using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Generator/Meta Generation Notification")]
public class CEvent_MeshGenerationData : ScriptableObject
{
    private List<CEventListener_MeshGenerationData> listeners = new List<CEventListener_MeshGenerationData>();

    public void Raise(MeshGenerationData data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(data);
        }
    }

    public void RegisterListener(CEventListener_MeshGenerationData listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_MeshGenerationData listener)
    {
        listeners.Remove(listener);
    }
}