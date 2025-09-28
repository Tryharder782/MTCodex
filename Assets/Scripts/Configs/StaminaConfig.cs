// Assets/Scripts/Configs/StaminaConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/StaminaConfig")]
public class StaminaConfig : ScriptableObject
{
    public float max = 100f;
    public float sprintDrainPerSec = 12f;
    public float dashCost = 20f;
    public float jumpCost = 0f; // при желании можно взять 5f
    public float climbDrainPerSec = 18f;
    public float glideDrainPerSec = 8f;
    public float regenPerSec = 10f;
    public float regenDelay = 0.75f;
}
