using System;
using System.Collections;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;
using Screen = UnityEngine.Screen;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
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

    [Header("Screenshot")]
    public Image screenshotPreview;
    public CanvasGroup screenshotCG;
    public CEvent_String errorEvent;

    private bool _clicked = false;
    private bool _freedrag = false;
 
    private Rigidbody _rigidbody;

    private Vector2 mouseAxisDelta;
    private Vector2 previousMouseAxisDelta = Vector2.zero;
    private float x = 0.0f;
    private float y = 0.0f;
 
    private bool orbitable;

    private Camera _camera;

    private int _windowWidth;
    private int _windowHeight;

    public enum CameraType
    {
        Perspective,
        Orthographic
    }
    
    private void Awake()
    {
        _actions = new Actions();
        // zoom event
        _actions.CameraControl.Zoom.performed += ZoomCamera;
        
        // starting drag event
        _actions.CameraControl.LeftClick.performed += (ctx =>
        {
            if (EventSystem.current.IsPointerOverGameObject()) orbitable = false;
            else
            {
                orbitable = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        });
        
        // stopping drag event
        _actions.CameraControl.LeftClick.canceled += (ctx =>
        {
            orbitable = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        });
        
        // drag performed event
        _actions.CameraControl.DragOrbit.performed += MouseDrag;

        _mouseScrollY = 0;

        _windowWidth = Screen.width;
        _windowHeight = Screen.height;
        screenshotPreview.rectTransform.sizeDelta = new Vector2(_windowWidth, _windowHeight);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody) _rigidbody.freezeRotation = true;
        _camera = GetComponent<Camera>();
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
        _actions.CameraControl.LeftClick.Enable();
        _actions.CameraControl.DragOrbit.Enable();
    }

    private void OnDisable()
    {
        _actions.CameraControl.Zoom.Disable();
        _actions.CameraControl.LeftClick.Disable();
        _actions.CameraControl.DragOrbit.Disable();
    }
    
    private void ZoomCamera(InputAction.CallbackContext context)
    {
        _mouseScrollY = context.ReadValue<float>();
    }
    
    // private void OnDrawGizmos()
    // {
    //     Vector3 meshCenter = target.transform.position + _targetOffset;
    //     Vector3 dirVector = meshCenter + transform.position; // a vector from the center of the mesh to the camera
    //     Vector3 normalDirVector = dirVector.normalized;
    //     Vector3 extendedDirVector = normalDirVector * zoomVal;
    //
    //     Gizmos.color = Color.green;
    //     // Gizmos.DrawLine(meshCenter, normalDirVector);
    //     Gizmos.DrawWireSphere(meshCenter, .5f);
    // }
    

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
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!orbitable) return;
        
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

    public void TakeScreenshot()
    {
        Debug.Log("Screenshot (" + _windowWidth + "x" + _windowHeight + ")");

        Rect rect = new Rect(0, 0, _windowWidth, _windowHeight);
        RenderTexture rt = new RenderTexture(_windowWidth, _windowHeight, 24);
        Texture2D ss = new Texture2D(_windowWidth, _windowHeight, TextureFormat.RGBA32, false);
        
        _camera.targetTexture = rt;
        _camera.Render();
        
        RenderTexture.active = rt;
        ss.ReadPixels(rect, 0, 0);
        ss.Apply();
        byte[] bytes = ss.EncodeToPNG();
        
        _camera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(rt);
        rt = null;

        string filename = "MeshCapture" + Putils.DateTimeString();
        string windowTitle = "Mesh Capture (" + _windowWidth + "x" + _windowHeight + ")";
        var path = StandaloneFileBrowser.SaveFilePanel(windowTitle, "", filename, "png");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                File.WriteAllBytes(path, bytes);
                
                screenshotPreview.gameObject.SetActive(true);
                screenshotPreview.sprite = Putils.Tex2dToSprite(ss);
                
                StartCoroutine(HideScreenshotPreview());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                errorEvent.Raise("Something went wrong while saving file.");
                throw;
            }
        }
    }

    IEnumerator HideScreenshotPreview()
    {
        Debug.Log("Starting screenshot coroutine");
        while (screenshotCG.alpha < 1)
        {
            screenshotCG.alpha += 0.15f;
            yield return new WaitForSeconds(0.018f);
        }
        
        yield return new WaitForSeconds(4);

        while (screenshotCG.alpha > 0)
        {
            screenshotCG.alpha -= 0.15f;
            yield return new WaitForSeconds(0.018f);
        }
        
        screenshotCG.alpha = 0;
        screenshotPreview.gameObject.SetActive(false);
    }
}
