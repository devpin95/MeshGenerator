using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CEventListener_OperationMetaData : MonoBehaviour
{
    [SerializeField] private CEvent_OperationMetaData Event;
    [SerializeField] private UnityEvent<OperationMetaData> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(OperationMetaData data)
    {
        Response.Invoke(data);
    }
}