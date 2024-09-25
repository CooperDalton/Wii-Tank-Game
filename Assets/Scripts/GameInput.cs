using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{ 

    private PlayerInputActions playerInputActions;

    public static GameInput Instance { get; private set; }

    public event EventHandler OnShootAction;
    public event EventHandler OnShootCanceledAction;
    public event EventHandler OnAltFireAction;

    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Fire.performed += FirePerformed;
        playerInputActions.Player.Fire.canceled += FireCanceledPerformed;

        playerInputActions.Player.AltFire.performed += AltFirePerformed;
    }

    private void AltFirePerformed(InputAction.CallbackContext context)
    {
        OnAltFireAction?.Invoke(this, EventArgs.Empty);
    }

    private void FireCanceledPerformed(InputAction.CallbackContext context)
    {
        OnShootCanceledAction?.Invoke(this, EventArgs.Empty);
    }

    private void FirePerformed(InputAction.CallbackContext context)
    {
        OnShootAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized(){
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>().normalized;
        return inputVector.normalized;
    }


}
