using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/CEvent <Texture2D>")]
public class CEvent_Texture2D : ScriptableObject
{
    private List<CEventListener_Texture2D> listeners = new List<CEventListener_Texture2D>();

    public void Raise(Texture2D tex)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(tex);
        }
    }

    public void RegisterListener(CEventListener_Texture2D listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(CEventListener_Texture2D listener)
    {
        listeners.Remove(listener);
    }
}