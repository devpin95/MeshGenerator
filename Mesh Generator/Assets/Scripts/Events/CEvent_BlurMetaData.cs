using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CEvent <BlurMetaData>")]
public class CEvent_BlurMetaData : ScriptableObject
{
    private List<CEventListener_BlurMetaData> listeners = new List<CEventListener_BlurMetaData>();

    public void Raise(BlurMetaData data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(data);
        }
    }

    public void RegisterListener(CEventListener_BlurMetaData listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_BlurMetaData listener)
    {
        listeners.Remove(listener);
    }
}