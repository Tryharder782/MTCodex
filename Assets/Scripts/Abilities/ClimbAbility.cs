// Assets/Scripts/Abilities/ClimbAbility.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ClimbAbility : AbilityBase
{
    public ClimbConfig config;
    Rigidbody rb;
    Vector3 wallNormal;

    public override void Initialize(MovementController o)
    {
        base.Initialize(o);
        rb = GetComponent<Rigidbody>();
        if (!config) config = ScriptableObject.CreateInstance<ClimbConfig>();
    }

    public override bool CanStart()
    {
        if (owner.Ground.IsGrounded) return false;
        return HitClimbable(out _);
    }

    public override void StartAbility()
    {
        if (!HitClimbable(out wallNormal)) return;
        IsActive = true;
        owner.SetClimbing(true);
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    public override void StopAbility()
    {
        IsActive = false;
        owner.SetClimbing(false);
        rb.useGravity = true;
    }

    public override void Tick(float dt)
    {
        if (!IsActive) return;

        owner.Stamina.ConsumePerSecond(owner.StaminaCfg.climbDrainPerSec);
        if (!owner.Stamina.Has(0.1f) || !HitClimbable(out wallNormal))
        {
            StopAbility(); 
            return;
        }
    }

    public override void FixedTick(float fdt)
    {
        if (!IsActive) return;

        Vector2 move = owner.LastMoveInput;
        Vector3 up = Vector3.up * move.y * config.climbSpeed;
        Vector3 lateral = Vector3.Cross(wallNormal, Vector3.up).normalized * move.x * config.climbSpeed * 0.5f;

        // держим у стены
        Vector3 stick = -wallNormal * 3f;

        rb.velocity = up + lateral + stick;

        // разворачиваем лицом к стене
        Vector3 faceDir = -wallNormal; faceDir.y = 0f;
        if (faceDir.sqrMagnitude > 0.001f)
            owner.FaceDirection(faceDir);
    }

    bool HitClimbable(out Vector3 normal)
    {
        normal = Vector3.zero;
        if (!config) return false;
        Vector3 origin = owner.transform.position + Vector3.up * 1.0f;
        if (Physics.Raycast(origin, owner.transform.forward, out RaycastHit hit, config.attachDistance, config.climbableMask, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle >= (90f - config.maxWallAngle)) // почти вертикально
            {
                normal = hit.normal;
                return true;
            }
        }
        return false;
    }
}
