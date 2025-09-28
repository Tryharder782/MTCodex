// Assets/Scripts/Core/MovementController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    public enum MovementState { Grounded, Airborne, Sprinting, Dashing, Climbing, Swimming, Gliding, LockOn }

    [Header("Refs")]
    public InputFacade input;
    public Animator animator;
    public Grounding Ground;
    public StaminaSystem Stamina;
    public MovementDebug debugRef;
    public LockOnModule lockOnModule;

    [Header("Configs")]
    public MovementConfig MoveCfg;
    public GroundingConfig GroundCfg;
    public StaminaConfig StaminaCfg;

    [Header("Abilities")]
    public DashAbility dash;
    public GlideAbility glide;
    public ClimbAbility climb;
    public SwimAbility swim;

    Rigidbody rb;
    CapsuleCollider capsule;
    Transform cam;

    // State
    public MovementState State { get; private set; } = MovementState.Grounded;
    public bool IsControllable { get; private set; } = true;
    public bool IsSprinting { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsGliding { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsSwimming { get; private set; }
    public bool IsLockedOn => lockOnModule && lockOnModule.IsLocked;

    bool walkToggled = false;
    bool sprintHeld = false;

    // Timers
    float lastGroundedTime = -999f; // для coyote
    float lastJumpPressedTime = -999f; // jump buffer
    float lastJumpTime = -999f;

    // Input snapshots
    public Vector2 LastMoveInput { get; private set; }
    public Vector2 LastLookInput { get; private set; }

    // Cached camera planar axes
    public Vector3 CameraPlanarForward { get; private set; }
    public Vector3 CameraPlanarRight { get; private set; }

    // Invulnerability
    bool invulnerable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        if (!debugRef) debugRef = GetComponent<MovementDebug>();
        if (!Ground) Ground = GetComponent<Grounding>();
        if (!Stamina) Stamina = GetComponent<StaminaSystem>();

        if (!MoveCfg) MoveCfg = ScriptableObject.CreateInstance<MovementConfig>();
        if (!StaminaCfg) StaminaCfg = ScriptableObject.CreateInstance<StaminaConfig>();
        if (!GroundCfg) GroundCfg = ScriptableObject.CreateInstance<GroundingConfig>();

        cam = Camera.main ? Camera.main.transform : null;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (dash) dash.Initialize(this);
        if (glide) glide.Initialize(this);
        if (climb) climb.Initialize(this);
        if (swim) swim.Initialize(this);
    }

    void OnEnable()
    {
        if (input == null) return;
        input.OnMove += HandleMove;
        input.OnLook += HandleLook;
        input.OnJumpPressed += HandleJumpPressed;
        input.OnJumpReleased += HandleJumpReleased;
        input.OnSprint += HandleSprint;
        input.OnDash += HandleDash;
        input.OnWalkToggle += HandleWalkToggle;
        input.OnLockOn += HandleLockOn;
        input.OnGlide += HandleGlide;
        input.OnClimb += HandleClimb;
        input.OnDive += HandleDive;
    }

    void OnDisable()
    {
        if (input == null) return;
        input.OnMove -= HandleMove;
        input.OnLook -= HandleLook;
        input.OnJumpPressed -= HandleJumpPressed;
        input.OnJumpReleased -= HandleJumpReleased;
        input.OnSprint -= HandleSprint;
        input.OnDash -= HandleDash;
        input.OnWalkToggle -= HandleWalkToggle;
        input.OnLockOn -= HandleLockOn;
        input.OnGlide -= HandleGlide;
        input.OnClimb -= HandleClimb;
        input.OnDive -= HandleDive;
    }

    void Update()
    {
        CacheCameraAxes();
        Stamina.Tick();

        UpdateAnimator();

        // FSM view (облегчённо)
        if (IsDashing) State = MovementState.Dashing;
        else if (IsClimbing) State = MovementState.Climbing;
        else if (IsSwimming) State = MovementState.Swimming;
        else if (IsGliding) State = MovementState.Gliding;
        else if (!Ground.IsGrounded) State = MovementState.Airborne;
        else if (IsSprinting) State = MovementState.Sprinting;
        else State = MovementState.Grounded;

        if (glide) glide.Tick(Time.deltaTime);
        if (climb) climb.Tick(Time.deltaTime);
        if (swim) swim.Tick(Time.deltaTime);
    }

    void FixedUpdate()
    {
        Ground.UpdateGrounding();

        if (Ground.IsGrounded) lastGroundedTime = Time.time;

        if (IsDashing)
        {
            // управление отдаёт DashAbility
            dash.FixedTick(Time.fixedDeltaTime);
            return;
        }

        if (IsClimbing)
        {
            climb.FixedTick(Time.fixedDeltaTime);
            return;
        }

        if (IsSwimming)
        {
            swim.FixedTick(Time.fixedDeltaTime);
            return;
        }

        // Гравитация
        ApplyCustomGravity();

        // Движение
        HandleMovementPhysics();

        // Прыжок по buffer/coyote
        TryPerformBufferedJump();

        if (glide && IsGliding) glide.FixedTick(Time.fixedDeltaTime);
    }

    void ApplyCustomGravity()
    {
        if (IsClimbing || IsSwimming) return;

        float g = Physics.gravity.y * MoveCfg.gravityMultiplier;
        Vector3 v = rb.velocity;
        v.y += g * Time.fixedDeltaTime;

        if (Ground.IsGrounded && v.y < 0f) // прижим к земле
            v.y -= MoveCfg.downforce * Time.fixedDeltaTime;

        rb.velocity = v;
    }

    void HandleMovementPhysics()
    {
        Vector2 move = LastMoveInput;
        Vector3 camF = CameraPlanarForward;
        Vector3 camR = CameraPlanarRight;
        Vector3 wishDir = (camF * move.y + camR * move.x);
        wishDir = wishDir.sqrMagnitude > 1e-3f ? wishDir.normalized : Vector3.zero;

        float targetSpeed = MoveCfg.runSpeed;
        if (walkToggled) targetSpeed = MoveCfg.walkSpeed;
        if (sprintHeld && Stamina.Has(StaminaCfg.sprintDrainPerSec * 0.1f) && Ground.IsGrounded)
        {
            targetSpeed = MoveCfg.sprintSpeed;
            IsSprinting = true;
            Stamina.ConsumePerSecond(StaminaCfg.sprintDrainPerSec);
        }
        else IsSprinting = false;

        // Проекция по поверхности, если на земле и не слишком круто
        if (Ground.IsGrounded && !Ground.OnTooSteep())
            wishDir = Mathx.ProjectOnPlane(wishDir, Ground.GroundNormal).normalized;

        Vector3 current = rb.velocity;
        Vector3 currentPlanar = new Vector3(current.x, 0f, current.z);
        Vector3 targetPlanar = wishDir * targetSpeed;

        float accel = Ground.IsGrounded ? MoveCfg.acceleration : MoveCfg.acceleration * MoveCfg.airControl;
        float decel = Ground.IsGrounded ? MoveCfg.deceleration : MoveCfg.deceleration * MoveCfg.airControl;

        Vector3 diff = targetPlanar - currentPlanar;
        Vector3 change;

        if (diff.magnitude > 0.1f)
            change = Vector3.ClampMagnitude(diff.normalized * accel * Time.fixedDeltaTime, diff.magnitude);
        else
            change = Vector3.ClampMagnitude(-currentPlanar.normalized * decel * Time.fixedDeltaTime, currentPlanar.magnitude);

        Vector3 newPlanar = currentPlanar + change;

        // Скольжение на крутом склоне
        if (Ground.IsGrounded && Ground.OnTooSteep())
        {
            Vector3 slide = Mathx.ProjectOnPlane(Vector3.down, Ground.GroundNormal);
            newPlanar += slide.normalized * MoveCfg.acceleration * 0.25f * Time.fixedDeltaTime;
        }

        rb.velocity = new Vector3(newPlanar.x, rb.velocity.y, newPlanar.z);

        // Step up
        if (Ground.IsGrounded && wishDir.sqrMagnitude > 0.1f)
            Ground.TryStepUp(MoveCfg.stepHeight, MoveCfg.stepCheckDistance);

        // Поворот персонажа
        if (wishDir.sqrMagnitude > 0.001f)
            FaceDirection(wishDir);
        else if (IsLockedOn && lockOnModule.currentTarget)
            FaceDirection((lockOnModule.currentTarget.position - transform.position).normalized);
    }

    void TryPerformBufferedJump()
    {
        bool buffered = (Time.time - lastJumpPressedTime) <= MoveCfg.jumpBuffer;
        bool coyote = (Time.time - lastGroundedTime) <= MoveCfg.coyoteTime;
        bool cooldownReady = (Time.time - lastJumpTime) >= MoveCfg.jumpCooldown;

        if (buffered && coyote && cooldownReady && Ground.IsGrounded && !IsDashing && !IsClimbing && !IsSwimming)
        {
            if (Stamina.TryConsume(StaminaCfg.jumpCost))
            {
                Vector3 v = rb.velocity;
                v.y = MoveCfg.jumpForce;
                rb.velocity = v;
                lastJumpPressedTime = -999f;
                lastJumpTime = Time.time;
                animator?.SetTrigger(MoveCfg.paramJumpTrigger);
            }
        }
    }

    void UpdateAnimator()
    {
        if (!animator || MoveCfg == null) return;
        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        animator.SetFloat(MoveCfg.paramSpeed, speed);
        animator.SetBool(MoveCfg.paramIsGrounded, Ground.IsGrounded);
        animator.SetBool(MoveCfg.paramIsSprinting, IsSprinting);
        animator.SetBool(MoveCfg.paramIsDashing, IsDashing);
    }

    // === Inputs ===
    void HandleMove(Vector2 v) => LastMoveInput = v;
    void HandleLook(Vector2 v) => LastLookInput = v;

    void HandleJumpPressed() => lastJumpPressedTime = Time.time;
    void HandleJumpReleased() { /* можно для variable jump */ }

    void HandleSprint(bool on) => sprintHeld = on;

    void HandleDash()
    {
        if (dash && dash.CanStart()) dash.StartAbility();
    }

    void HandleWalkToggle() => walkToggled = !walkToggled;

    void HandleLockOn()
    {
        if (!lockOnModule) return;
        lockOnModule.ToggleLock(transform);
    }

    void HandleGlide(bool on)
    {
        if (!glide) return;
        if (on && glide.CanStart()) glide.StartAbility();
        if (!on && glide.IsActive) glide.StopAbility();
    }

    void HandleClimb(bool on)
    {
        if (!climb) return;
        if (on && climb.CanStart()) climb.StartAbility();
        if (!on && climb.IsActive) climb.StopAbility();
    }

    void HandleDive()
    {
        if (swim && swim.IsActive)
            rb.AddForce(Vector3.down * swim.config.diveImpulse, ForceMode.VelocityChange);
    }

    // === Public helpers ===
    public void SetDashing(bool on) { IsDashing = on; IsControllable = !on; }
    public void SetGliding(bool on) { IsGliding = on; }
    public void SetClimbing(bool on) { IsClimbing = on; IsControllable = on; }
    public void SetSwimming(bool on) { IsSwimming = on; }

    public void SetInvulnerable(bool on) { invulnerable = on; }
    public bool IsInvulnerable() => invulnerable;

    public Vector3 GetDashDirection()
    {
        Vector2 m = LastMoveInput;
        Vector3 dir = (CameraPlanarForward * m.y + CameraPlanarRight * m.x);
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
        return dir.normalized;
    }

    public void FaceDirection(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, 0.2f);
    }

    void CacheCameraAxes()
    {
        if (!cam) { var c = Camera.main; if (c) cam = c.transform; }
        if (!cam) { CameraPlanarForward = transform.forward; CameraPlanarRight = transform.right; return; }
        Vector3 f = cam.forward; f.y = 0f; f = f.sqrMagnitude > 0.001f ? f.normalized : transform.forward;
        Vector3 r = cam.right; r.y = 0f; r = r.sqrMagnitude > 0.001f ? r.normalized : transform.right;
        CameraPlanarForward = f; CameraPlanarRight = r;
    }

    // Triggers для воды (повесьте Water trigger с соответствующим слоем/тегом)
    void OnTriggerEnter(Collider other)
    {
        if (!swim || swim.config == null) return;
        if (((1 << other.gameObject.layer) & swim.config.waterMask) != 0)
            swim.SetInWater(true);
    }
    void OnTriggerExit(Collider other)
    {
        if (!swim || swim.config == null) return;
        if (((1 << other.gameObject.layer) & swim.config.waterMask) != 0)
            swim.SetInWater(false);
    }
}
