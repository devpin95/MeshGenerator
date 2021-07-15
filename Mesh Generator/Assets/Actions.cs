// GENERATED AUTOMATICALLY FROM 'Assets/Actions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Actions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Actions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Actions"",
    ""maps"": [
        {
            ""name"": ""Camera Control"",
            ""id"": ""affd4eea-8151-4e22-b5a2-b03614a79de4"",
            ""actions"": [
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""a7659a8f-b7c6-410f-848b-68439fe5d179"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""872c94bc-d216-4d8f-a194-c501de13a00f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Drag Orbit"",
                    ""type"": ""Value"",
                    ""id"": ""a7e37a3f-b191-4017-b0c8-2c89e845a058"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Track Mouse"",
                    ""type"": ""Value"",
                    ""id"": ""f143805a-743b-4b86-94ad-4f965d3199c2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""803cb326-fe2e-45d7-bbb6-6424417fac25"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d5f942b7-a510-46ea-beec-54bf356ccb4e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""eacc10de-3970-4d10-af68-52a25267fd30"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drag Orbit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""58b77e7c-c7d2-4b13-bd66-8a8e98dfd73d"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Track Mouse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Camera Control
        m_CameraControl = asset.FindActionMap("Camera Control", throwIfNotFound: true);
        m_CameraControl_Zoom = m_CameraControl.FindAction("Zoom", throwIfNotFound: true);
        m_CameraControl_Click = m_CameraControl.FindAction("Click", throwIfNotFound: true);
        m_CameraControl_DragOrbit = m_CameraControl.FindAction("Drag Orbit", throwIfNotFound: true);
        m_CameraControl_TrackMouse = m_CameraControl.FindAction("Track Mouse", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Camera Control
    private readonly InputActionMap m_CameraControl;
    private ICameraControlActions m_CameraControlActionsCallbackInterface;
    private readonly InputAction m_CameraControl_Zoom;
    private readonly InputAction m_CameraControl_Click;
    private readonly InputAction m_CameraControl_DragOrbit;
    private readonly InputAction m_CameraControl_TrackMouse;
    public struct CameraControlActions
    {
        private @Actions m_Wrapper;
        public CameraControlActions(@Actions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Zoom => m_Wrapper.m_CameraControl_Zoom;
        public InputAction @Click => m_Wrapper.m_CameraControl_Click;
        public InputAction @DragOrbit => m_Wrapper.m_CameraControl_DragOrbit;
        public InputAction @TrackMouse => m_Wrapper.m_CameraControl_TrackMouse;
        public InputActionMap Get() { return m_Wrapper.m_CameraControl; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraControlActions set) { return set.Get(); }
        public void SetCallbacks(ICameraControlActions instance)
        {
            if (m_Wrapper.m_CameraControlActionsCallbackInterface != null)
            {
                @Zoom.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnZoom;
                @Click.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnClick;
                @DragOrbit.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnDragOrbit;
                @DragOrbit.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnDragOrbit;
                @DragOrbit.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnDragOrbit;
                @TrackMouse.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnTrackMouse;
                @TrackMouse.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnTrackMouse;
                @TrackMouse.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnTrackMouse;
            }
            m_Wrapper.m_CameraControlActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @DragOrbit.started += instance.OnDragOrbit;
                @DragOrbit.performed += instance.OnDragOrbit;
                @DragOrbit.canceled += instance.OnDragOrbit;
                @TrackMouse.started += instance.OnTrackMouse;
                @TrackMouse.performed += instance.OnTrackMouse;
                @TrackMouse.canceled += instance.OnTrackMouse;
            }
        }
    }
    public CameraControlActions @CameraControl => new CameraControlActions(this);
    public interface ICameraControlActions
    {
        void OnZoom(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnDragOrbit(InputAction.CallbackContext context);
        void OnTrackMouse(InputAction.CallbackContext context);
    }
}
