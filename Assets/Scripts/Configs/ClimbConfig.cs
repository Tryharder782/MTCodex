// Assets/Scripts/Configs/ClimbConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/ClimbConfig")]
public class ClimbConfig : ScriptableObject
{
    public LayerMask climbableMask;
    public float attachDistance = 0.6f;
    public float maxWallAngle = 75f;
    public float climbSpeed = 2.0f;
}
