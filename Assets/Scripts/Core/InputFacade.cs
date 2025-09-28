// Assets/Scripts/Core/InputFacade.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputFacade : MonoBehaviour
{
    [Header("Input Action References")]
    public InputActionReference move;      // Vector2
    public InputActionReference look;      // Vector2 (delta)
    public InputActionReference jump;      // Button
    public InputActionReference sprint;    // Button (hold)
    public InputActionReference dash;      // Button
    public InputActionReference walkToggle;// Button
    public InputActionReference lockOn;    // Button
    public InputActionReference glide;     // Button (hold)
    public InputActionReference climb;     // Button (hold)
    public InputActionReference dive;      // Button (press)

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action<bool> OnSprint;
    public event Action OnDash;
    public event Action OnWalkToggle;
    public event Action OnLockOn;
    public event Action<bool> OnGlide;
    public event Action<bool> OnClimb;
    public event Action OnDive;

    void OnEnable()
    {
        Enable(move, v => OnMove?.Invoke(v.ReadValue<Vector2>()));
        Enable(look, v => OnLook?.Invoke(v.ReadValue<Vector2>()));

        Enable(jump,
            started: _ => OnJumpPressed?.Invoke(),
            canceled: _ => OnJumpReleased?.Invoke());

        Enable(sprint,
            started: _ => OnSprint?.Invoke(true),
            canceled: _ => OnSprint?.Invoke(false));

        Enable(dash, started: _ => OnDash?.Invoke());
        Enable(walkToggle, started: _ => OnWalkToggle?.Invoke());
        Enable(lockOn, started: _ => OnLockOn?.Invoke());

        Enable(glide,
            started: _ => OnGlide?.Invoke(true),
            canceled: _ => OnGlide?.Invoke(false));

        Enable(climb,
            started: _ => OnClimb?.Invoke(true),
            canceled: _ => OnClimb?.Invoke(false));

        Enable(dive, started: _ => OnDive?.Invoke());
    }

    void OnDisable()
    {
        Disable(move); Disable(look); Disable(jump);
        Disable(sprint); Disable(dash); Disable(walkToggle);
        Disable(lockOn); Disable(glide); Disable(climb); Disable(dive);
    }

    void Enable(InputActionReference actionRef, Action<InputAction.CallbackContext> performed = null,
        Action<InputAction.CallbackContext> started = null,
        Action<InputAction.CallbackContext> canceled = null)
    {
        if (actionRef == null || actionRef.action == null) return;
        if (performed != null) actionRef.action.performed += performed;
        if (started != null) actionRef.action.started += started;
        if (canceled != null) actionRef.action.canceled += canceled;
        actionRef.action.Enable();
    }

    void Disable(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null) return;
        actionRef.action.Disable();
    }
}
