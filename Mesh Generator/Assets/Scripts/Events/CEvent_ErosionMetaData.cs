using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CEvent <ErosionMetaData>")]
public class CEvent_ErosionMetaData : ScriptableObject
{
    private List<CEventListener_ErosionMetaData> listeners = new List<CEventListener_ErosionMetaData>();

    public void Raise(ErosionMetaData data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(data);
        }
    }

    public void RegisterListener(CEventListener_ErosionMetaData listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_ErosionMetaData listener)
    {
        listeners.Remove(listener);
    }
}