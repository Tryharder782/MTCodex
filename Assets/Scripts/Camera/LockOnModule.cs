// Assets/Scripts/Camera/LockOnModule.cs
using UnityEngine;
using System.Collections.Generic;

public class LockOnModule : MonoBehaviour
{
    public float lockRange = 15f;
    public LayerMask targetMask;
    public Transform currentTarget;

    public bool IsLocked => currentTarget != null;

    public void ToggleLock(Transform origin)
    {
        if (currentTarget) { currentTarget = null; return; }
        currentTarget = FindClosestTarget(origin.position, origin.forward);
    }

    public void SwitchTarget(Transform origin, float dir)
    {
        // TODO: реализовать выбор соседней цели по углу
        currentTarget = FindClosestTarget(origin.position, origin.forward);
    }

    Transform FindClosestTarget(Vector3 pos, Vector3 forward)
    {
        Collider[] hits = Physics.OverlapSphere(pos, lockRange, targetMask, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        Transform bestT = null;
        foreach (var h in hits)
        {
            Vector3 to = h.transform.position - pos; to.y = 0f;
            float ang = Vector3.Angle(forward, to);
            float d = to.magnitude + ang * 0.1f;
            if (d < best) { best = d; bestT = h.transform; }
        }
        return bestT;
    }
}
