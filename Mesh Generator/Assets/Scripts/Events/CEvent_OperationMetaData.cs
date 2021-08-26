using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CEvent <Operation Meta Data>")]
public class CEvent_OperationMetaData : ScriptableObject
{
    private List<CEventListener_OperationMetaData> listeners = new List<CEventListener_OperationMetaData>();

    public void Raise(OperationMetaData data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(data);
        }
    }

    public void RegisterListener(CEventListener_OperationMetaData listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_OperationMetaData listener)
    {
        listeners.Remove(listener);
    }
}