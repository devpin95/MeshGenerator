using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    public GameObject target;
    public float zoomSpeed = 2;
    public float zoomVal = 2;

    private Actions _actions;
    [SerializeField] private float _mouseScrollY = 0;
    private Vector3 _cameraTarget;

    private CameraController _cameraController;

    public TextMeshProUGUI scrollVal;
    

    private void Awake()
    {
        _actions = new Actions();
        _actions.CameraControl.Zoom.performed += ZoomCamera;
        _actions.CameraControl.LeftClick.performed += Click;

        _mouseScrollY = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 meshCenter = target.transform.position + _cameraTarget;
        Vector3 dirVector = transform.position - meshCenter; // a vector from the center of the mesh to the camera
        Vector3 normalDirVector = dirVector.normalized;
        Vector3 extendedDirVector = normalDirVector * zoomVal;
        transform.position = extendedDirVector;

        _cameraController = GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        _cameraTarget = GetComponent<CameraTarget>().GetOffset();
        
        scrollVal.text = _mouseScrollY.ToString("f2");
        if (_mouseScrollY > 0)
        {
            if (_cameraController.GetCameraType() == CameraController.CameraType.Perspective)
            {
                Debug.Log("Perspective");
                zoomVal += 1; 
                UpdateZoom();
            } 
            else if (_cameraController.GetCameraType() == CameraController.CameraType.Orthographic)
            {
                Debug.Log("Ortho");
                Camera.main.orthographicSize += zoomVal * .2f;
                if (Camera.main.orthographicSize < 0) Camera.main.orthographicSize = 0;
            }

            Debug.Log("Zoom in");
        }
        else if (_mouseScrollY < 0)
        {
            if (_cameraController.GetCameraType() == CameraController.CameraType.Perspective)
            {
                Debug.Log("Perspective");
                zoomVal -= 1;
                UpdateZoom();
            } 
            else if (_cameraController.GetCameraType() == CameraController.CameraType.Orthographic)
            {
                Debug.Log("Ortho");
                Camera.main.orthographicSize -= zoomVal * .2f;
            }
            

            Debug.Log("Zoom out");
        }
    }

    private void ZoomCamera(InputAction.CallbackContext context)
    {
        _mouseScrollY = context.ReadValue<float>();
    }

    private void Click(InputAction.CallbackContext context)
    {
        Debug.Log("CLICKED");
    }

    private void OnEnable()
    {
        _actions.CameraControl.Zoom.Enable();
        _actions.CameraControl.LeftClick.Enable();
    }

    private void OnDisable()
    {
        _actions.CameraControl.Zoom.Disable();
        _actions.CameraControl.LeftClick.Disable();
    }

    private void OnDrawGizmos()
    {
        Vector3 meshCenter = target.transform.position + _cameraTarget;
        Vector3 dirVector = transform.position - meshCenter; // a vector from the center of the mesh to the camera
        Vector3 normalDirVector = dirVector;
        Vector3 extendedDirVector = normalDirVector * zoomVal;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(meshCenter, dirVector);
        Gizmos.DrawWireSphere(meshCenter, .5f);
    }

    private void UpdateZoom()
    {
        Vector3 meshCenter = target.transform.position + _cameraTarget;
        Vector3 dirVector = transform.position - meshCenter; // a vector from the center of the mesh to the camera
        Vector3 normalDirVector = dirVector.normalized;
        Vector3 extendedDirVector = normalDirVector * zoomVal;
        
        transform.position = extendedDirVector;
    }
}
