using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class CEventListener_LayerData : MonoBehaviour
{
    [SerializeField] private CEvent_LayerData Event;
    [SerializeField] private UnityEvent<LayerData> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(LayerData data)
    {
        Response.Invoke(data);
    }
}