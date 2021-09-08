using UnityEngine;
using UnityEngine.Events;

public class CEventListener_BlurMetaData : MonoBehaviour
{
    [SerializeField] private CEvent_BlurMetaData Event;
    [SerializeField] private UnityEvent<BlurMetaData> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(BlurMetaData data)
    {
        Response.Invoke(data);
    }
}