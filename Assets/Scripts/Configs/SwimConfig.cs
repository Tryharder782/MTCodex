// Assets/Scripts/Configs/SwimConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/SwimConfig")]
public class SwimConfig : ScriptableObject
{
    public LayerMask waterMask;
    public float swimSpeed = 3f;
    public float buoyancy = 12f;
    public float waterDrag = 3f;
    public float diveImpulse = 4f;
}
