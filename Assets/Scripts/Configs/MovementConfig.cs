// Assets/Scripts/Configs/MovementConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("Speeds (m/s)")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5.0f;
    public float sprintSpeed = 7.5f;

    [Header("Acceleration")]
    public float acceleration = 20f;
    public float deceleration = 25f;
    [Range(0f,1f)] public float airControl = 0.5f;

    [Header("Jump")]
    public float jumpForce = 5.5f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float jumpCooldown = 0.05f; // частота прыжков

    [Header("Gravity")]
    public float gravityMultiplier = 1.6f; // к -9.81f
    public float downforce = 10f;          // доп. прижим к земле

    [Header("Slope/Steps")]
    [Range(0f,89f)] public float maxSlopeAngle = 50f;
    public float stepHeight = 0.35f;
    public float stepCheckDistance = 0.4f;

    [Header("Friction")]
    public PhysicMaterial highFriction;
    public PhysicMaterial lowFriction;

    [Header("Animator params")]
    public string paramSpeed = "Speed";
    public string paramIsGrounded = "IsGrounded";
    public string paramIsSprinting = "IsSprinting";
    public string paramIsDashing = "IsDashing";
    public string paramJumpTrigger = "Jump";
    public string paramLandTrigger = "Land";
}
