// Assets/Scripts/Utils/Mathx.cs
using UnityEngine;

public static class Mathx
{
    public static Vector3 Flatten(Vector3 v) => new Vector3(v.x, 0f, v.z);

    public static Vector3 ProjectOnPlane(Vector3 v, Vector3 normal)
    {
        return v - Vector3.Project(v, normal);
    }

    public static float SignedAngleOnPlane(Vector3 from, Vector3 to, Vector3 planeNormal)
    {
        var f = Vector3.ProjectOnPlane(from, planeNormal);
        var t = Vector3.ProjectOnPlane(to, planeNormal);
        return Vector3.SignedAngle(f, t, planeNormal);
    }
}
