using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CEventListener_String : MonoBehaviour
{
    [SerializeField] private CEvent_String Event;
    [SerializeField] private UnityEvent<string> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(string message)
    {
        Response.Invoke(message);
    }
}