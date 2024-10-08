//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/InputSystem/PlayerInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerInput: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""PlayerControls"",
            ""id"": ""93ee60bf-bcf9-4486-8f2d-510defce52be"",
            ""actions"": [
                {
                    ""name"": ""Mouse1"",
                    ""type"": ""Button"",
                    ""id"": ""e81936ff-df1f-46ac-a199-0bb606e6e76b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Mouse2"",
                    ""type"": ""Button"",
                    ""id"": ""a6088127-a3a0-4293-aa98-e0db239813a7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraControlMode"",
                    ""type"": ""Value"",
                    ""id"": ""d7b2c307-d200-4014-8199-d74296b7b9c7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""ShiftButton"",
                    ""type"": ""Value"",
                    ""id"": ""8ecaa37c-53d7-4ff7-8cbf-d6aa89badbd2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""AbilityButton1"",
                    ""type"": ""Button"",
                    ""id"": ""bd868db1-b55a-4198-806c-2634fd10a7a2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AbilityButton2"",
                    ""type"": ""Button"",
                    ""id"": ""eb5ea0f5-0307-4f3d-8c75-c4c73aefbfc0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AbilityButton3"",
                    ""type"": ""Button"",
                    ""id"": ""e0ace250-559d-4053-a732-22c24ecfa01e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AbilityButton4"",
                    ""type"": ""Button"",
                    ""id"": ""13c4e79a-f618-49c9-9039-89a32aa9e070"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ca7ec1f3-c144-484c-9a8a-29d355b72add"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b32bd15c-b38c-498e-98a0-ac9e8e2446d8"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1cc0e5fc-a0e4-4796-beb5-f8e8219e687e"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraControlMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f88bb466-92b4-4daf-a959-5222dc166c61"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""01698943-0736-4110-a4de-78b6fb93ee34"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilityButton1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0e902b21-e8f7-4e6a-936d-eb8ce0904483"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilityButton2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""17549b03-6a15-4f4f-88ed-853864c28513"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilityButton3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7f657940-9892-4ff2-9d53-68b61770707c"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilityButton4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // PlayerControls
        m_PlayerControls = asset.FindActionMap("PlayerControls", throwIfNotFound: true);
        m_PlayerControls_Mouse1 = m_PlayerControls.FindAction("Mouse1", throwIfNotFound: true);
        m_PlayerControls_Mouse2 = m_PlayerControls.FindAction("Mouse2", throwIfNotFound: true);
        m_PlayerControls_CameraControlMode = m_PlayerControls.FindAction("CameraControlMode", throwIfNotFound: true);
        m_PlayerControls_ShiftButton = m_PlayerControls.FindAction("ShiftButton", throwIfNotFound: true);
        m_PlayerControls_AbilityButton1 = m_PlayerControls.FindAction("AbilityButton1", throwIfNotFound: true);
        m_PlayerControls_AbilityButton2 = m_PlayerControls.FindAction("AbilityButton2", throwIfNotFound: true);
        m_PlayerControls_AbilityButton3 = m_PlayerControls.FindAction("AbilityButton3", throwIfNotFound: true);
        m_PlayerControls_AbilityButton4 = m_PlayerControls.FindAction("AbilityButton4", throwIfNotFound: true);
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

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // PlayerControls
    private readonly InputActionMap m_PlayerControls;
    private List<IPlayerControlsActions> m_PlayerControlsActionsCallbackInterfaces = new List<IPlayerControlsActions>();
    private readonly InputAction m_PlayerControls_Mouse1;
    private readonly InputAction m_PlayerControls_Mouse2;
    private readonly InputAction m_PlayerControls_CameraControlMode;
    private readonly InputAction m_PlayerControls_ShiftButton;
    private readonly InputAction m_PlayerControls_AbilityButton1;
    private readonly InputAction m_PlayerControls_AbilityButton2;
    private readonly InputAction m_PlayerControls_AbilityButton3;
    private readonly InputAction m_PlayerControls_AbilityButton4;
    public struct PlayerControlsActions
    {
        private @PlayerInput m_Wrapper;
        public PlayerControlsActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Mouse1 => m_Wrapper.m_PlayerControls_Mouse1;
        public InputAction @Mouse2 => m_Wrapper.m_PlayerControls_Mouse2;
        public InputAction @CameraControlMode => m_Wrapper.m_PlayerControls_CameraControlMode;
        public InputAction @ShiftButton => m_Wrapper.m_PlayerControls_ShiftButton;
        public InputAction @AbilityButton1 => m_Wrapper.m_PlayerControls_AbilityButton1;
        public InputAction @AbilityButton2 => m_Wrapper.m_PlayerControls_AbilityButton2;
        public InputAction @AbilityButton3 => m_Wrapper.m_PlayerControls_AbilityButton3;
        public InputAction @AbilityButton4 => m_Wrapper.m_PlayerControls_AbilityButton4;
        public InputActionMap Get() { return m_Wrapper.m_PlayerControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerControlsActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerControlsActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerControlsActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerControlsActionsCallbackInterfaces.Add(instance);
            @Mouse1.started += instance.OnMouse1;
            @Mouse1.performed += instance.OnMouse1;
            @Mouse1.canceled += instance.OnMouse1;
            @Mouse2.started += instance.OnMouse2;
            @Mouse2.performed += instance.OnMouse2;
            @Mouse2.canceled += instance.OnMouse2;
            @CameraControlMode.started += instance.OnCameraControlMode;
            @CameraControlMode.performed += instance.OnCameraControlMode;
            @CameraControlMode.canceled += instance.OnCameraControlMode;
            @ShiftButton.started += instance.OnShiftButton;
            @ShiftButton.performed += instance.OnShiftButton;
            @ShiftButton.canceled += instance.OnShiftButton;
            @AbilityButton1.started += instance.OnAbilityButton1;
            @AbilityButton1.performed += instance.OnAbilityButton1;
            @AbilityButton1.canceled += instance.OnAbilityButton1;
            @AbilityButton2.started += instance.OnAbilityButton2;
            @AbilityButton2.performed += instance.OnAbilityButton2;
            @AbilityButton2.canceled += instance.OnAbilityButton2;
            @AbilityButton3.started += instance.OnAbilityButton3;
            @AbilityButton3.performed += instance.OnAbilityButton3;
            @AbilityButton3.canceled += instance.OnAbilityButton3;
            @AbilityButton4.started += instance.OnAbilityButton4;
            @AbilityButton4.performed += instance.OnAbilityButton4;
            @AbilityButton4.canceled += instance.OnAbilityButton4;
        }

        private void UnregisterCallbacks(IPlayerControlsActions instance)
        {
            @Mouse1.started -= instance.OnMouse1;
            @Mouse1.performed -= instance.OnMouse1;
            @Mouse1.canceled -= instance.OnMouse1;
            @Mouse2.started -= instance.OnMouse2;
            @Mouse2.performed -= instance.OnMouse2;
            @Mouse2.canceled -= instance.OnMouse2;
            @CameraControlMode.started -= instance.OnCameraControlMode;
            @CameraControlMode.performed -= instance.OnCameraControlMode;
            @CameraControlMode.canceled -= instance.OnCameraControlMode;
            @ShiftButton.started -= instance.OnShiftButton;
            @ShiftButton.performed -= instance.OnShiftButton;
            @ShiftButton.canceled -= instance.OnShiftButton;
            @AbilityButton1.started -= instance.OnAbilityButton1;
            @AbilityButton1.performed -= instance.OnAbilityButton1;
            @AbilityButton1.canceled -= instance.OnAbilityButton1;
            @AbilityButton2.started -= instance.OnAbilityButton2;
            @AbilityButton2.performed -= instance.OnAbilityButton2;
            @AbilityButton2.canceled -= instance.OnAbilityButton2;
            @AbilityButton3.started -= instance.OnAbilityButton3;
            @AbilityButton3.performed -= instance.OnAbilityButton3;
            @AbilityButton3.canceled -= instance.OnAbilityButton3;
            @AbilityButton4.started -= instance.OnAbilityButton4;
            @AbilityButton4.performed -= instance.OnAbilityButton4;
            @AbilityButton4.canceled -= instance.OnAbilityButton4;
        }

        public void RemoveCallbacks(IPlayerControlsActions instance)
        {
            if (m_Wrapper.m_PlayerControlsActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerControlsActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerControlsActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerControlsActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerControlsActions @PlayerControls => new PlayerControlsActions(this);
    public interface IPlayerControlsActions
    {
        void OnMouse1(InputAction.CallbackContext context);
        void OnMouse2(InputAction.CallbackContext context);
        void OnCameraControlMode(InputAction.CallbackContext context);
        void OnShiftButton(InputAction.CallbackContext context);
        void OnAbilityButton1(InputAction.CallbackContext context);
        void OnAbilityButton2(InputAction.CallbackContext context);
        void OnAbilityButton3(InputAction.CallbackContext context);
        void OnAbilityButton4(InputAction.CallbackContext context);
    }
}
