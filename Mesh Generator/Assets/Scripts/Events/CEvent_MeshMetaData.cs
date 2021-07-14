using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Generator/Meta Data Notification")]
public class CEvent_MeshMetaData : ScriptableObject
{
    private List<CEventListener_MeshMetaData> listeners = new List<CEventListener_MeshMetaData>();

    public void Raise(MeshMetaData data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(data);
        }
    }

    public void RegisterListener(CEventListener_MeshMetaData listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_MeshMetaData listener)
    {
        listeners.Remove(listener);
    }
}