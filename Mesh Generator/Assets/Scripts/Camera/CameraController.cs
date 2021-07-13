using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public GameObject target;
    private MeshGenerator _targetMG;
    private Vector3 _targetOffset;
    private Vector3 _targetPos;
    
    [Header("Actions")]
    public float zoomVal = 2;

    private Actions _actions;
    [SerializeField] private float _mouseScrollY = 1;
    private Vector3 _cameraTarget;
    
    // camera settings
    [SerializeField] private bool _inPerspectiveMode = true;

    public enum CameraType
    {
        Perspective,
        Orthographic
    }
    
    private void Awake()
    {
        _actions = new Actions();
        _actions.CameraControl.Zoom.performed += ZoomCamera;

        _mouseScrollY = 0;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _targetMG = target.GetComponent<MeshGenerator>();
        _targetOffset = new Vector3(_targetMG.xSize / 2f, 0, _targetMG.zSize / 2f);
        _targetPos = target.transform.position + _targetOffset;

        UpdateZoom();
    }

    // Update is called once per frame
    void Update()
    {
        if (_mouseScrollY > 0)
        {
            if (GetCameraType() == CameraType.Perspective)
            {
                if (Vector3.Distance(_targetPos, transform.position) > 1) transform.position += transform.forward;
            } 
            else if (GetCameraType() == CameraType.Orthographic)
            {
                Camera.main.orthographicSize -= 0.2f;
                if (Camera.main.orthographicSize < 0) Camera.main.orthographicSize = 0;
            }

            Debug.Log("Zoom out");
        }
        else if (_mouseScrollY < 0)
        {
            if (GetCameraType() == CameraType.Perspective)
            {
                transform.position -= transform.forward;
            } 
            else if (GetCameraType() == CameraType.Orthographic)
            {
                Camera.main.orthographicSize += 0.2f;
            }
        }
        
        transform.LookAt(_targetPos);
    }
    
    private void OnEnable()
    {
        _actions.CameraControl.Zoom.Enable();
        _actions.CameraControl.Click.Enable();
    }

    private void OnDisable()
    {
        _actions.CameraControl.Zoom.Disable();
        _actions.CameraControl.Click.Disable();
    }
    
    private void ZoomCamera(InputAction.CallbackContext context)
    {
        _mouseScrollY = context.ReadValue<float>();
    }
    
    private void OnDrawGizmos()
    {
        Vector3 meshCenter = target.transform.position + _targetOffset;
        Vector3 dirVector = meshCenter + transform.position; // a vector from the center of the mesh to the camera
        Vector3 normalDirVector = dirVector.normalized;
        Vector3 extendedDirVector = normalDirVector * zoomVal;

        Gizmos.color = Color.green;
        // Gizmos.DrawLine(meshCenter, normalDirVector);
        Gizmos.DrawWireSphere(meshCenter, .5f);
    }

    private void UpdateZoom()
    {
        
    }

    public void ChangeCameraPerspective()
    {
        _inPerspectiveMode = !_inPerspectiveMode;

        if (_inPerspectiveMode)
        {
            Camera.main.orthographic = false;
        }
        else
        {
            Camera.main.orthographic = true;
        }
    }

    public CameraType GetCameraType()
    {
        if (_inPerspectiveMode) return CameraType.Perspective;
        else return CameraType.Orthographic;
    }
}
