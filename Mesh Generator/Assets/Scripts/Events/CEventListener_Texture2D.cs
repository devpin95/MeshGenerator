using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CEventListener_Texture2D : MonoBehaviour
{
    [SerializeField] private CEvent_Texture2D Event;
    [SerializeField] private UnityEvent<Texture2D> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(Texture2D tex)
    {
        Response.Invoke(tex);
    }
}