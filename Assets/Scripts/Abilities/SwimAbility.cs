// Assets/Scripts/Abilities/SwimAbility.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimAbility : AbilityBase
{
    public SwimConfig config;
    Rigidbody rb;
    bool inWater;

    public void SetInWater(bool on)
    {
        inWater = on;
        if (on && !IsActive && owner.IsControllable) StartAbility();
        if (!on && IsActive) StopAbility();
    }

    public override void Initialize(MovementController o)
    {
        base.Initialize(o);
        rb = GetComponent<Rigidbody>();
        if (!config) config = ScriptableObject.CreateInstance<SwimConfig>();
    }

    public override bool CanStart() => inWater;

    public override void StartAbility()
    {
        IsActive = true;
        owner.SetSwimming(true);
        rb.drag = config.waterDrag;
    }

    public override void StopAbility()
    {
        IsActive = false;
        owner.SetSwimming(false);
        rb.drag = 0f;
    }

    public override void Tick(float dt)
    {
        if (!IsActive) return;
        // Здесь стамина не уходит по умолчанию
    }

    public override void FixedTick(float fdt)
    {
        if (!IsActive) return;

        // Простая плавучесть
        rb.AddForce(Vector3.up * config.buoyancy, ForceMode.Acceleration);

        // Движение
        Vector2 move = owner.LastMoveInput;
        Vector3 camF = owner.CameraPlanarForward;
        Vector3 camR = owner.CameraPlanarRight;
        Vector3 wish = (camF * move.y + camR * move.x) * config.swimSpeed;
        Vector3 vel = rb.velocity;
        Vector3 velPlanar = new Vector3(wish.x, vel.y, wish.z);
        rb.velocity = Vector3.Lerp(vel, velPlanar, 0.2f);
    }
}
