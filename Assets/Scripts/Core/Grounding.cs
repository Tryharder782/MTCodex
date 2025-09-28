// Assets/Scripts/Core/Grounding.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grounding : MonoBehaviour
{
    public GroundingConfig config;
    public MovementConfig movementConfig;
    public MovementDebug debugRef;

    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;
    public float SlopeAngle { get; private set; }
    public Vector3 GroundPoint { get; private set; }

    float groundedTimer = 0f;
    float ungroundedTimer = 0f;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!debugRef) debugRef = GetComponent<MovementDebug>();
    }

    public void UpdateGrounding()
    {
        Vector3 pos = transform.position + Vector3.up * (config.capsuleHeight * 0.5f - config.capsuleRadius);
        Vector3 bottom = transform.position - Vector3.up * (config.capsuleHeight * 0.5f - config.capsuleRadius);

        RaycastHit hit;
        bool hitGround = Physics.CapsuleCast(pos, bottom, config.capsuleRadius,
            Vector3.down, out hit, config.groundCheckDistance, config.groundMask, QueryTriggerInteraction.Ignore);

        if (hitGround)
        {
            GroundNormal = hit.normal;
            GroundPoint = hit.point;
            SlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            groundedTimer += Time.fixedDeltaTime;
            ungroundedTimer = 0f;

            if (!IsGrounded && groundedTimer >= config.groundedEnterDelay)
                IsGrounded = true;
        }
        else
        {
            ungroundedTimer += Time.fixedDeltaTime;
            groundedTimer = 0f;
            if (IsGrounded && ungroundedTimer >= config.groundedExitDelay)
                IsGrounded = false;

            GroundNormal = Vector3.up;
            SlopeAngle = 0f;
        }
    }

    public bool OnTooSteep() => SlopeAngle > movementConfig.maxSlopeAngle;

    public bool TryStepUp(float stepHeight, float checkDistance)
    {
        // Низкий луч
        Vector3 fwd = Mathx.Flatten(transform.forward).normalized;
        Vector3 originLow = transform.position + Vector3.up * config.stepLowerRay;
        if (Physics.Raycast(originLow, fwd, out RaycastHit lowHit, checkDistance, config.groundMask, QueryTriggerInteraction.Ignore))
        {
            // Верхний луч
            Vector3 originHigh = transform.position + Vector3.up * (config.stepUpperRay + stepHeight);
            if (!Physics.Raycast(originHigh, fwd, checkDistance, config.groundMask, QueryTriggerInteraction.Ignore))
            {
                // Поднимаем
                rb.position += Vector3.up * stepHeight;
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (config == null) return;
        if (debugRef && !debugRef.drawGroundCapsule && !debugRef.drawStepRays && !debugRef.drawSlopeNormal) return;

        Gizmos.color = Color.green;
        Vector3 top = transform.position + Vector3.up * (config.capsuleHeight * 0.5f - config.capsuleRadius);
        Vector3 bottom = transform.position - Vector3.up * (config.capsuleHeight * 0.5f - config.capsuleRadius);
        if (debugRef == null || debugRef.drawGroundCapsule)
        {
            Gizmos.DrawWireSphere(top, config.capsuleRadius);
            Gizmos.DrawWireSphere(bottom, config.capsuleRadius);
            Gizmos.DrawLine(top + Vector3.forward * config.capsuleRadius, bottom + Vector3.forward * config.capsuleRadius);
            Gizmos.DrawLine(top - Vector3.forward * config.capsuleRadius, bottom - Vector3.forward * config.capsuleRadius);
            Gizmos.DrawLine(top + Vector3.right * config.capsuleRadius, bottom + Vector3.right * config.capsuleRadius);
            Gizmos.DrawLine(top - Vector3.right * config.capsuleRadius, bottom - Vector3.right * config.capsuleRadius);
        }

        if (debugRef == null || debugRef.drawStepRays)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * config.stepLowerRay,
                transform.position + Vector3.up * config.stepLowerRay + Mathx.Flatten(transform.forward).normalized * movementConfig.stepCheckDistance);
            Gizmos.DrawLine(transform.position + Vector3.up * (config.stepUpperRay + movementConfig.stepHeight),
                transform.position + Vector3.up * (config.stepUpperRay + movementConfig.stepHeight) + Mathx.Flatten(transform.forward).normalized * movementConfig.stepCheckDistance);
        }

        if (debugRef == null || debugRef.drawSlopeNormal)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(GroundPoint, GroundNormal);
        }
    }
}
