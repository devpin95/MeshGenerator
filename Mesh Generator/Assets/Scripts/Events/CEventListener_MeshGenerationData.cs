using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CEventListener_MeshGenerationData : MonoBehaviour
{
    [SerializeField] private CEvent_MeshGenerationData Event;
    [SerializeField] private UnityEvent<MeshGenerationData> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(MeshGenerationData data)
    {
        Response.Invoke(data);
    }
}