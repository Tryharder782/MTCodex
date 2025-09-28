// Assets/Scripts/Core/AbilityBase.cs
using UnityEngine;

public interface IAbility
{
    bool IsActive { get; }
    void Initialize(MovementController owner);
    bool CanStart();
    void StartAbility();
    void StopAbility();
    void Tick(float dt);
    void FixedTick(float fdt);
}

public abstract class AbilityBase : MonoBehaviour, IAbility
{
    protected MovementController owner;
    public bool IsActive { get; protected set; }

    public virtual void Initialize(MovementController ownerRef) { owner = ownerRef; }
    public abstract bool CanStart();
    public abstract void StartAbility();
    public abstract void StopAbility();
    public virtual void Tick(float dt) { }
    public virtual void FixedTick(float fdt) { }
}
