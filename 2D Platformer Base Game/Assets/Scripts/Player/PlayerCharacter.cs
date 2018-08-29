using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CharacterController2D))]
// [RequireComponent(typeof(Animator))]

public class PlayerCharacter : MonoBehaviour {

    static protected PlayerCharacter playerInstance;
    static public PlayerCharacter PlayerInstance
    {
        get { return playerInstance; }
    }

    protected InventoryController inventoryController;
    public InventoryController InventoryController
    {
        get { return inventoryController; }
    }

    // Sprites
    public SpriteRenderer spriteRenderer;
    public bool spriteOriginallyFacesLeft;
    protected bool startingFacingLeft = false;

    // Damage
    public Damageable damageable;
    public Damager meleeDamager;
    public float meleeAttackDashSpeed = 5f;
    public bool dashWhileAirborne = false;

    // Flicker
    protected WaitForSeconds flickeringWait;
    protected Coroutine flickerCoroutine;
    public float flickeringDuration = 0.1f;

    // Hurt jump angle
    protected const float minHurtJumpAngle = 0.001f;
    protected const float maxHurtJumpAngle = 89.999f;
    protected float tanHurtJumpAngle;
    [Range(minHurtJumpAngle, maxHurtJumpAngle)] public float hurtJumpAngle = 45f;
    public float hurtJumpSpeed = 5f;

    // Movement
    protected CharacterController2D characterController2D;
    // protected CapsuleCollider2D _collider;
    protected BoxCollider2D _collider;
    protected Vector2 startingPosition = Vector2.zero;
    protected Vector2 moveVector;
    public float maxSpeed = 10f;
    public float groundAccel = 100f;
    public float groundDecel = 100f;
    public float gravity = 50f;
    public float jumpSpeed = 20f;
    public float jumpAbortSpeedReduction = 100f;
    [Range(0f, 1f)] public float airborneAccelProportion;
    [Range(0f, 1f)] public float airborneDecelProportion;
    protected const float groundStickVelocityMultiplier = 3f;

    // Camera follow
    public Transform cameraFollowTarget;
    protected float camFollowHorizontalSpeed;
    protected float camFollowVerticalSpeed;
    protected float verticalCameraOffsetTimer;
    public float cameraHorizontalFacingOffset = 2f;
    public float cameraHorizontalSpeedOffset = 0.2f;
    public float cameraVerticalInputOffset = 2f;
    public float maxHorizontalDeltaDampTime = 0.4f;
    public float maxVerticalDeltaDampTime = 0.6f;
    public float verticalCameraOffsetDelay = 1f;

    // Animator
    protected Animator animator;
    protected readonly int hashHorizontalSpeedPara = Animator.StringToHash("HorizontalSpeed");
    protected readonly int hashVerticalSpeedPara = Animator.StringToHash("VerticalSpeed");
    protected readonly int hashGroundedPara = Animator.StringToHash("OnGround");
    // protected readonly int hashCrouchingPara = Animator.StringToHash("Crouching");
    // protected readonly int hashPushingPara = Animator.StringToHash("Pushing");
    // protected readonly int hashTimeoutPara = Animator.StringToHash("Timeout");
    protected readonly int hashRespawnPara = Animator.StringToHash("Respawn");
    protected readonly int hashDeadPara = Animator.StringToHash("Dead");
    protected readonly int hashHurtPara = Animator.StringToHash("Hurt");
    protected readonly int hashForcedRespawnPara = Animator.StringToHash("ForcedRespawn");
    protected readonly int hashMeleeAttackPara = Animator.StringToHash("MeleeAttack");
    // protected readonly int hashHoldingGunPara = Animator.StringToHash("HoldingGun");

    protected Checkpoint lastCheckpoint;
    protected bool inPause = false;

    void Awake()
    {
        playerInstance = this;
        characterController2D = GetComponent<CharacterController2D>();
        inventoryController = GetComponent<InventoryController>();
        _collider = GetComponent<BoxCollider2D>();
        // _collider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        hurtJumpAngle = Mathf.Clamp(hurtJumpAngle, minHurtJumpAngle, maxHurtJumpAngle);
        tanHurtJumpAngle = Mathf.Tan(Mathf.Deg2Rad * hurtJumpAngle);
        flickeringWait = new WaitForSeconds(flickeringDuration);

        meleeDamager.DisableDamage();

        if (!Mathf.Approximately(maxHorizontalDeltaDampTime, 0f))
        {
            float maxHorizontalDelta = maxSpeed * cameraHorizontalSpeedOffset + cameraHorizontalFacingOffset;
            camFollowHorizontalSpeed = maxHorizontalDelta / maxHorizontalDeltaDampTime;
        }

        if (!Mathf.Approximately(maxVerticalDeltaDampTime, 0f))
        {
            float maxVerticalDelta = cameraVerticalInputOffset;
            camFollowVerticalSpeed = maxVerticalDelta / maxVerticalDeltaDampTime;
        }

        SceneLinkedSMB<PlayerCharacter>.Initialise(animator, this);

        startingPosition = transform.position;
        startingFacingLeft = (GetFacing() < 0.0f);
    }

    void Update()
    {
        if (PlayerInput.Instance.Pause.Down)
        {
            if (!inPause)
            {
                if (ScreenFader.IsFading)
                {
                    return;
                }

                PlayerInput.Instance.ReleaseControl(false);
                PlayerInput.Instance.Pause.GainControl();
                inPause = true;
                Time.timeScale = 0;
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("UIMenus", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            else
            {
                Unpause();
            }
        }
    }

    void FixedUpdate()
    {
        characterController2D.Move(moveVector * Time.deltaTime);
        animator.SetFloat(hashHorizontalSpeedPara, moveVector.x);
        animator.SetFloat(hashVerticalSpeedPara, moveVector.y);
        UpdateCameraFollowTargetPosition();
    }


    // PAUSE

    public void Unpause()
    {
        if (Time.timeScale > 0)
            return;

        StartCoroutine(UnpauseCoroutine());
    }

    protected IEnumerator UnpauseCoroutine()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("UIMenus");
        PlayerInput.Instance.GainControl();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        inPause = false;
    }

    protected void UpdateCameraFollowTargetPosition()
    {
        float newLocalPosX;
        float newLocalPosY = 0f;

        float desiredLocalPosX = (spriteOriginallyFacesLeft ^ spriteRenderer.flipX ? -1f : 1f) * cameraHorizontalFacingOffset;
        desiredLocalPosX += moveVector.x * cameraHorizontalSpeedOffset;
        if (Mathf.Approximately(camFollowHorizontalSpeed, 0f))
        {
            newLocalPosX = desiredLocalPosX;
        }
        else
        {
            newLocalPosX = Mathf.Lerp(cameraFollowTarget.localPosition.x, desiredLocalPosX, camFollowHorizontalSpeed * Time.deltaTime);
        }
        bool moveVertically = false;
        if (!Mathf.Approximately(PlayerInput.Instance.Vertical.Value, 0f))
        {
            verticalCameraOffsetTimer += Time.deltaTime;

            if (verticalCameraOffsetTimer >= verticalCameraOffsetDelay)
            {
                moveVertically = true;
            }
        }
        else
        {
            moveVertically = true;
            verticalCameraOffsetTimer = 0f;
        }

        if (moveVertically)
        {
            float desiredLocalPosY = PlayerInput.Instance.Vertical.Value * cameraVerticalInputOffset;
            if (Mathf.Approximately(camFollowVerticalSpeed, 0f))
            {
                newLocalPosY = desiredLocalPosY;
            }
            else
            {
                newLocalPosY = Mathf.MoveTowards(cameraFollowTarget.localPosition.y, desiredLocalPosY, camFollowVerticalSpeed * Time.deltaTime);
            }
        }

        cameraFollowTarget.localPosition = new Vector2(newLocalPosX, newLocalPosY);
    }


    // MOVEMENT

    public void SetMoveVector(Vector2 newMoveVector)
    {
        moveVector = newMoveVector;
    }

    public void SetHorizontalMovement(float newHorizontalMovement)
    {
        moveVector.x = newHorizontalMovement;
    }

    public void SetVerticalMovement(float newVerticalMovement)
    {
        moveVector.y = newVerticalMovement;
    }

    public void IncrementMovement(Vector2 additionalMovement)
    {
        moveVector += additionalMovement;
    }

    public void IncrementHorizontalMovement(float additionalHorizontalMovement)
    {
        moveVector.x += additionalHorizontalMovement;
    }

    public void IncrementVerticalMovement(float additionalVerticalMovement)
    {
        moveVector.y += additionalVerticalMovement;
    }

    public Vector2 GetMoveVector()
    {
        return moveVector;
    }

    public void UpdateFacing()
    {
        bool faceLeft = PlayerInput.Instance.Horizontal.Value < 0f;
        bool faceRight = PlayerInput.Instance.Horizontal.Value > 0f;

        if (faceLeft)
        {
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
        }
        else if (faceRight)
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
        }
    }

    public void UpdateFacing(bool faceLeft)
    {
        if (faceLeft)
        {
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
        }
        else
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
        }
    }

    public float GetFacing()
    {
        return spriteRenderer.flipX != spriteOriginallyFacesLeft ? -1f : 1f;
    }

    public void GroundHorizontalMovement(bool useInput, float speedScale = 1f)
    {
        float desiredSpeed = useInput ? PlayerInput.Instance.Horizontal.Value * maxSpeed * speedScale : 0f;
        float acceleration = useInput && PlayerInput.Instance.Horizontal.ReceivingInput ? groundAccel : groundDecel;
        moveVector.x = Mathf.MoveTowards(moveVector.x, desiredSpeed, acceleration * Time.deltaTime);
    }

    public void GroundVerticalMovement()
    {
        moveVector.y -= gravity * Time.deltaTime;

        if (moveVector.y < -gravity * Time.deltaTime * groundStickVelocityMultiplier)
        {
            moveVector.y = -gravity * Time.deltaTime * groundStickVelocityMultiplier;
        }
    }

    public bool CheckOnGround()
    {
        bool wasOnGround = animator.GetBool(hashGroundedPara);
        bool onGround = characterController2D.OnGround;

        animator.SetBool(hashGroundedPara, onGround);

        return onGround;
    }

    public void AirHorizontalMovement()
    {
        float desiredSpeed = PlayerInput.Instance.Horizontal.Value * maxSpeed;
        float acceleration;

        if (PlayerInput.Instance.Horizontal.ReceivingInput)
        {
            acceleration = groundAccel * airborneAccelProportion;
        }
        else
        {
            acceleration = groundDecel * airborneDecelProportion;
        }

        moveVector.x = Mathf.MoveTowards(moveVector.x, desiredSpeed, acceleration * Time.deltaTime);
    }

    public void AirVerticalMovement()
    {
        if (Mathf.Approximately(moveVector.y, 0f) || characterController2D.OnCeiling && moveVector.y > 0f)
        {
            moveVector.y = 0f;
        }
        moveVector.y -= gravity * Time.deltaTime;
    }

    public bool IsFalling()
    {
        return moveVector.y < 0f && !animator.GetBool(hashGroundedPara);
    }

    // JUMP

    public bool CheckForJumpInput()
    {
        return PlayerInput.Instance.Jump.Down;
    }

    public bool CheckForFallInput()
    {
        return (PlayerInput.Instance.Vertical.Value < -float.Epsilon && PlayerInput.Instance.Jump.Down);
    }

    public void UpdateJump()
    {
        if (!PlayerInput.Instance.Jump.Held && moveVector.y > 0.0f)
        {
            moveVector.y -= jumpAbortSpeedReduction * Time.deltaTime;
        }
    }


    // MELEE ATTACK

    public bool CheckForMeleeAttackInput()
    {
        return PlayerInput.Instance.MeleeAttack.Down;
    }

    public void MeleeAttack()
    {
        animator.SetTrigger(hashMeleeAttackPara);
    }

    public void EnableMeleeAttack()
    {
        meleeDamager.EnableDamage();
        meleeDamager.disableDamageAfterHit = true;
    }

    public void DisableMeleeAttack()
    {
        meleeDamager.DisableDamage();
    }

    public void TeleportToColliderBottom()
    {
        Vector2 colliderBottom = characterController2D.Rigidbody.position + _collider.offset + Vector2.down * _collider.size.y * 0.5f;
        characterController2D.Teleport(colliderBottom);
    }

    // INVULNERABILITY

    public void EnableInvulnerability()
    {
        damageable.EnableInvulnerability();
    }

    public void DisableInvulnerability()
    {
        damageable.DisableInvulnerability();
    }


    // FLICKER

    protected IEnumerator Flicker()
    {
        float timer = 0f;

        while (timer < damageable.invulnerabilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return flickeringWait;
            timer += flickeringDuration;
        }

        spriteRenderer.enabled = true;
    }

    public void StartFlickering()
    {
        flickerCoroutine = StartCoroutine(Flicker());
    }

    public void StopFlickering()
    {
        StopCoroutine(flickerCoroutine);
        spriteRenderer.enabled = true;
    }


    // TAKE DAMAGE

    public Vector2 GetHurtDirection()
    {
        Vector2 damageDirection = damageable.GetDamageDirection();

        if (damageDirection.y < 0f)
        {
            return new Vector2(Mathf.Sign(damageDirection.x), 0f);
        }

        float y = Mathf.Abs(damageDirection.x) * tanHurtJumpAngle;

        return new Vector2(damageDirection.x, y).normalized;
    }

    public void OnHurt(Damager damager, Damageable damageable)
    {
        if (!PlayerInput.Instance.HaveControl)
        {
            return;
        }

        UpdateFacing(damageable.GetDamageDirection().x > 0f);
        damageable.EnableInvulnerability();

        animator.SetTrigger(hashHurtPara);

        if (damageable.CurrentHealth > 0 && damager.forceRespawn)
        {
            animator.SetTrigger(hashForcedRespawnPara);
        }

        animator.SetBool(hashGroundedPara, false);

        if (damager.forceRespawn && damageable.CurrentHealth > 0)
        {
            StartCoroutine(DieRespawnCoroutine(false, true));
        }
    }


    // DIE

    public void OnDie()
    {
        animator.SetTrigger(hashDeadPara);

        StartCoroutine(DieRespawnCoroutine(true, false));
    }

    IEnumerator DieRespawnCoroutine(bool resetHealth, bool useCheckPoint)
    {
        PlayerInput.Instance.ReleaseControl(true);
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(ScreenFader.FadeSceneOut(useCheckPoint ? ScreenFader.FadeType.Black : ScreenFader.FadeType.GameOver));
        if (!useCheckPoint)
        {
            yield return new WaitForSeconds(2.0f);
        }
        Respawn(resetHealth, useCheckPoint);
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(ScreenFader.FadeSceneIn());
        PlayerInput.Instance.GainControl();
    }


    // RESPAWN

    public void Respawn(bool resetHealth, bool useCheckpoint)
    {
        if (resetHealth)
        {
            damageable.SetHealth(damageable.startingHealth);
        }

        
        animator.ResetTrigger(hashHurtPara);
        if (flickerCoroutine != null)
        {
            StopFlickering();
        }
        animator.SetTrigger(hashRespawnPara);
        

        if (useCheckpoint && lastCheckpoint != null)
        {
            UpdateFacing(lastCheckpoint.respawnFacingLeft);
            GameObjectTeleporter.Teleport(gameObject, lastCheckpoint.transform.position);
        }
        else
        {
            UpdateFacing(startingFacingLeft);
            GameObjectTeleporter.Teleport(gameObject, startingPosition);
        }
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        lastCheckpoint = checkpoint;
    }

    /*
    public void KeyInventoryEvent()
    {
        if (KeyUI.Instance != null) KeyUI.Instance.ChangeKeyUI(inventoryController);
    }
    */
}
