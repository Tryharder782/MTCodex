// Assets/Scripts/Configs/GroundingConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/GroundingConfig")]
public class GroundingConfig : ScriptableObject
{
    [Header("Capsule Ground Check")]
    public float capsuleRadius = 0.25f;
    public float capsuleHeight = 1.8f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Debounce")]
    public float groundedEnterDelay = 0.02f;
    public float groundedExitDelay = 0.05f;

    [Header("Slope")]
    public float slopeSnapSpeed = 20f;

    [Header("Step Casts")]
    public float stepLowerRay = 0.1f;
    public float stepUpperRay = 0.5f;
}
