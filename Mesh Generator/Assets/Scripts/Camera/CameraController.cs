using System;
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
    
    [Header("Orbit Values")]
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
 
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = .5f;
    public float distanceMax = 15f;
 
    private Rigidbody _rigidbody;

    private Vector2 mouseAxisDelta;
    private Vector2 previousMouseAxisDelta = Vector2.zero;
    private float x = 0.0f;
    private float y = 0.0f;
 
    private bool orbitable;

    public enum CameraType
    {
        Perspective,
        Orthographic
    }
    
    private void Awake()
    {
        _actions = new Actions();
        _actions.CameraControl.Zoom.performed += ZoomCamera;
        _actions.CameraControl.Click.performed += (ctx => orbitable = true);
        _actions.CameraControl.Click.canceled += (ctx => orbitable = false);
        _actions.CameraControl.DragOrbit.performed += MouseDrag;

        _mouseScrollY = 0;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody) _rigidbody.freezeRotation = true;
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

            // Debug.Log("Zoom out");
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

    private void LateUpdate()
    {
        if (orbitable)
        {
            if (previousMouseAxisDelta == mouseAxisDelta) return;

            x += mouseAxisDelta.x * xSpeed * 0.02f;
            y -= mouseAxisDelta.y * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
            
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            // distance = Mathf.Clamp(distance, distanceMin, distanceMax);
            distance = Vector3.Distance(transform.position, _targetPos);

            RaycastHit hit;
            if (Physics.Raycast(_targetPos, transform.position, out hit))
            {
                distance -= hit.distance;
            }
            
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + _targetPos;
 
            transform.rotation = rotation;
            transform.position = position;
        }

        previousMouseAxisDelta = mouseAxisDelta;
    } 

    private void OnEnable()
    {
        _actions.CameraControl.Zoom.Enable();
        _actions.CameraControl.Click.Enable();
        _actions.CameraControl.DragOrbit.Enable();
    }

    private void OnDisable()
    {
        _actions.CameraControl.Zoom.Disable();
        _actions.CameraControl.Click.Disable();
        _actions.CameraControl.DragOrbit.Disable();
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

    public void MouseDrag(InputAction.CallbackContext context)
    {
        mouseAxisDelta = context.ReadValue<Vector2>();
    }
    
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void ReaquireTarget(Vector3 pos)
    {
        _targetPos = pos;
    }
}
