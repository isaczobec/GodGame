using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;

    public static PlayerInputHandler Instance;




    // --------- EVENTS ----------

    public EventHandler<UnityEngine.InputSystem.InputAction.CallbackContext> OnMouse1;
    public EventHandler<UnityEngine.InputSystem.InputAction.CallbackContext> OnMouse1Released;

    public EventHandler<UnityEngine.InputSystem.InputAction.CallbackContext> OnMouse2;
    public EventHandler<UnityEngine.InputSystem.InputAction.CallbackContext> OnMouse2Released;


    public EventHandler<int> OnAbilityPressed;
    public EventHandler<int> OnAbilityReleased;


    // methods

    public void Awake() {
        // Singleton pattern
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }
    }
    void Start()
    {
        playerInput = new PlayerInput();
        playerInput.PlayerControls.Enable();

        // Subscribe to input events

        playerInput.PlayerControls.Mouse1.performed += ctx => OnMouse1?.Invoke(this, ctx);
        playerInput.PlayerControls.Mouse1.canceled += ctx => OnMouse1Released?.Invoke(this, ctx);

        playerInput.PlayerControls.Mouse2.performed += ctx => OnMouse2?.Invoke(this, ctx);
        playerInput.PlayerControls.Mouse2.canceled += ctx => OnMouse2Released?.Invoke(this, ctx);

        playerInput.PlayerControls.AbilityButton1.performed += ctx => OnAbilityPressed?.Invoke(this, 1);
        playerInput.PlayerControls.AbilityButton1.canceled += ctx => OnAbilityReleased?.Invoke(this, 1);
        playerInput.PlayerControls.AbilityButton2.performed += ctx => OnAbilityPressed?.Invoke(this, 2);
        playerInput.PlayerControls.AbilityButton2.canceled += ctx => OnAbilityReleased?.Invoke(this, 2);
        playerInput.PlayerControls.AbilityButton3.performed += ctx => OnAbilityPressed?.Invoke(this, 3);
        playerInput.PlayerControls.AbilityButton3.canceled += ctx => OnAbilityReleased?.Invoke(this, 3);
        playerInput.PlayerControls.AbilityButton4.performed += ctx => OnAbilityPressed?.Invoke(this, 4);
        playerInput.PlayerControls.AbilityButton4.canceled += ctx => OnAbilityReleased?.Invoke(this, 4);
    }

    public bool GetCameraControlButtonPressed() {
        return playerInput.PlayerControls.CameraControlMode.ReadValue<float>() != 0;
    }

    public bool GetShiftButtonPressed() {
        return playerInput.PlayerControls.ShiftButton.ReadValue<float>() != 0;
    }

}
