// Assets/Scripts/Abilities/GlideAbility.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GlideAbility : AbilityBase
{
    public GlideConfig config;
    Rigidbody rb;

    public override void Initialize(MovementController o)
    {
        base.Initialize(o);
        rb = GetComponent<Rigidbody>();
        if (!config) config = ScriptableObject.CreateInstance<GlideConfig>();
    }

    public override bool CanStart()
    {
        return !owner.Ground.IsGrounded && owner.Stamina.Has(owner.StaminaCfg.glideDrainPerSec * 0.2f);
    }

    public override void StartAbility()
    {
        IsActive = true;
        owner.SetGliding(true);
    }

    public override void StopAbility()
    {
        IsActive = false;
        owner.SetGliding(false);
    }

    public override void Tick(float dt)
    {
        if (!IsActive) return;

        owner.Stamina.ConsumePerSecond(owner.StaminaCfg.glideDrainPerSec);
        if (!owner.Stamina.Has(0.1f)) StopAbility();
    }

    public override void FixedTick(float fdt)
    {
        if (!IsActive) return;

        // ослабленная гравитация
        Vector3 v = rb.velocity;
        float g = Physics.gravity.y * owner.MoveCfg.gravityMultiplier;
        v.y += -g * (1f - config.gravityScaleWhileGlide) * fdt; // частично компенсируем
        rb.velocity = v;

        // немного поддержим горизонтальную скорость
        rb.AddForce(Mathx.Flatten(rb.velocity).normalized * config.horizontalBoost, ForceMode.Acceleration);
    }
}
