using UnityEngine;
using UnityEngine.Events;

public class CEventListener_ErosionMetaData : MonoBehaviour
{
    [SerializeField] private CEvent_ErosionMetaData Event;
    [SerializeField] private UnityEvent<ErosionMetaData> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(ErosionMetaData data)
    {
        Response.Invoke(data);
    }
}