// Assets/Scripts/Abilities/DashAbility.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class DashAbility : AbilityBase
{
    public DashConfig config;
    Rigidbody rb;
    Coroutine dashRoutine;
    int layerNormal, layerInvuln;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Initialize(MovementController o)
    {
        base.Initialize(o);
        if (config == null) config = ScriptableObject.CreateInstance<DashConfig>();
        layerNormal = LayersAndTags.Layer(config.normalLayer);
        layerInvuln = LayersAndTags.Layer(config.invulnLayer);
    }

    public override bool CanStart()
    {
        if (IsActive) return false;
        if (!owner.IsControllable) return false;
        // Стоимость берём из StaminaConfig (по спецификации)
        return owner.Stamina.Has(owner.StaminaCfg.dashCost);
    }

    public override void StartAbility()
    {
        if (!CanStart()) return;

        // Списываем стамину по стоимости из StaminaConfig
        owner.Stamina.TryConsume(owner.StaminaCfg.dashCost);

        if (dashRoutine != null) StopCoroutine(dashRoutine);
        dashRoutine = StartCoroutine(DoDash());
    }

    IEnumerator DoDash()
    {
        IsActive = true;
        owner.SetDashing(true);

        Vector3 dir = owner.GetDashDirection();
        if (dir.sqrMagnitude < 0.0001f) dir = owner.transform.forward;
        dir.Normalize();

        float speed = config.dashDistance / Mathf.Max(0.01f, config.dashDuration);
        Vector3 dashVelocity = dir * speed;

        float t = 0f;
        float iStart = Mathf.Max(0f, config.iFramesWindow.x);
        float iEnd = Mathf.Max(iStart, config.iFramesWindow.y);

        // Включаем i-frames (у нас окно [0..iEnd], при желании можно сдвинуть на iStart)
        ToggleInvulnerability(true);

        while (t < config.dashDuration)
        {
            rb.velocity = new Vector3(dashVelocity.x, 0f, dashVelocity.z); // плоский рывок
            t += Time.fixedDeltaTime;

            if (t >= iEnd)
                ToggleInvulnerability(false);

            yield return new WaitForFixedUpdate();
        }

        ToggleInvulnerability(false);
        owner.SetDashing(false);
        IsActive = false;
    }

    public override void StopAbility()
    {
        if (dashRoutine != null) StopCoroutine(dashRoutine);
        ToggleInvulnerability(false);
        owner.SetDashing(false);
        IsActive = false;
    }

    void ToggleInvulnerability(bool on)
    {
        if (config.useLayerSwitch && layerInvuln >= 0 && layerNormal >= 0)
            gameObject.layer = on ? layerInvuln : layerNormal;

        if (config.hurtboxColliders != null)
            foreach (var c in config.hurtboxColliders)
                if (c) c.enabled = !on;

        owner.SetInvulnerable(on);
    }
}
