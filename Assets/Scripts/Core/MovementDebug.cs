// Assets/Scripts/Core/MovementDebug.cs
using UnityEngine;

public class MovementDebug : MonoBehaviour
{
    [Header("Debug Toggles")]
    public bool drawGroundCapsule = true;
    public bool drawStepRays = true;
    public bool drawSlopeNormal = true;
    public bool verboseLogs = false;

    public void Log(string msg, Object ctx)
    {
        if (verboseLogs) Debug.Log(msg, ctx);
    }
}
