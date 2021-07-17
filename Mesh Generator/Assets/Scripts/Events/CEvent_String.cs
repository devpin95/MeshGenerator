using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CEvent <String>")]
public class CEvent_String : ScriptableObject
{
    private List<CEventListener_String> listeners = new List<CEventListener_String>();

    public void Raise(string message)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(message);
        }
    }

    public void RegisterListener(CEventListener_String listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_String listener)
    {
        listeners.Remove(listener);
    }
}