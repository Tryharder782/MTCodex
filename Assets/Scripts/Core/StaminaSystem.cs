// Assets/Scripts/Core/StaminaSystem.cs
using UnityEngine;
using System;

public class StaminaSystem : MonoBehaviour
{
    public StaminaConfig config;

    public float Current { get; private set; }
    float lastConsumeTime;
    public event Action<float> OnChanged;

    void Awake()
    {
        if (!config) config = ScriptableObject.CreateInstance<StaminaConfig>();
        Current = Mathf.Clamp(config.max, 1f, 10000f);
    }

    public bool Has(float amount) => Current >= amount;

    public bool TryConsume(float amount)
    {
        if (amount <= 0f) return true;
        if (Current < amount) return false;
        Current -= amount;
        lastConsumeTime = Time.time;
        OnChanged?.Invoke(Current);
        return true;
    }

    public void ConsumePerSecond(float rate)
    {
        if (rate <= 0f) return;
        float delta = rate * Time.deltaTime;
        TryConsume(delta);
    }

    public void Tick()
    {
        if (Time.time - lastConsumeTime >= config.regenDelay)
        {
            float regen = config.regenPerSec * Time.deltaTime;
            Current = Mathf.Min(config.max, Current + regen);
            OnChanged?.Invoke(Current);
        }
    }

    public void ForceSet(float value)
    {
        Current = Mathf.Clamp(value, 0f, config.max);
        OnChanged?.Invoke(Current);
    }
}
