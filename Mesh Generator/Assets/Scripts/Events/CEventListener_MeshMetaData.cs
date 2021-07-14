using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CEventListener_MeshMetaData : MonoBehaviour
{
    [SerializeField] private CEvent_MeshMetaData Event;
    [SerializeField] private UnityEvent<MeshMetaData> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(MeshMetaData data)
    {
        Response.Invoke(data);
    }
}