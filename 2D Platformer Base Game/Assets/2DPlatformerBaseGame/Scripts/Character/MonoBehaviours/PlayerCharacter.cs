using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(Animator))]

public class PlayerCharacter : MonoBehaviour {

    static protected PlayerCharacter playerInstance;
    static public PlayerCharacter PlayerInstance
    { get { return playerInstance; } }

    public InventoryController InventoryController
    {
        get { return inventoryController; }
    }
    
    public SpriteRenderer spriteRenderer;
    public Damageable damageable;
    public Damager meleeDamager;
    public Transform facingLeftBulletSpawnPoint;
    public Transform facingRightBulletSpawnPoint;
    public BulletPool bulletPool;
    public Transform cameraFollowTarget;

    public float maxSpeed = 10f;
    public float groundAcceleration = 100f;
    public float groundDeceleration = 100f;
    [Range(0f, 1f)] public float pushingSpeedProportion;
    [Range(0f, 1f)] public float airborneAccelProportion;
    [Range(0f, 1f)] public float airborneDecelProportion;
    public float gravity = 50f;
    public float jumpSpeed = 20f;
    public float jumpAbortSpeedReduction = 100f;

    public float wallSlideVertSpeedReduction = 0f;
    public float wallSlideJumpAngle = 45f;
    public bool canWallSlideUp = false;
    public float wallSlideUpSpeed = 0f;

    [Range(minHurtJumpAngle, maxHurtJumpAngle)] public float hurtJumpAngle = 45f;
    public float hurtJumpSpeed = 5f;
    public float flickeringDuration = 0.1f;

    public float meleeAttackDashSpeed = 5f;
    public bool dashWhileAirborne = false;

    public RandomAudioPlayer footstepAudioPlayer;
    public RandomAudioPlayer landingAudioPlayer;
    public RandomAudioPlayer hurtAudioPlayer;
    public RandomAudioPlayer meleeAttackAudioPlayer;
    public RandomAudioPlayer rangedAttackAudioPlayer;

    public float shotsPerSecond = 1f;
    public float bulletSpeed = 5f;
    public float holdingGunTimeoutDuration = 10f;
    public bool rightBulletSpawnPointAnimated = true;

    public float cameraHorizontalFacingOffset = 2f;
    public float cameraHorizontalSpeedOffset = 0.2f;
    public float cameraVerticalInputOffset = 2f;
    public float maxHorizontalDeltaDampTime = 0.4f;
    public float maxVerticalDeltaDampTime = 0.6f;
    public float verticalCameraOffsetDelay = 1f;

    public bool spriteOriginallyFacesLeft;

    protected CharacterController2D characterController2D;
    protected Animator animator;
    protected BoxCollider2D _collider;
    protected Vector2 moveVector;
    protected List<Pushable> currentPushables = new List<Pushable>(4);
    protected Pushable currentPushable;
    protected float tanHurtJumpAngle;
    protected WaitForSeconds flickeringWait;
    protected Coroutine flickerCoroutine;
    protected Transform currentBulletSpawnPoint;
    protected float shotSpawnGap;
    protected WaitForSeconds shotSpawnWait;
    protected Coroutine shootingCoroutine;
    protected float nextShotTime;
    protected bool isFiring;
    protected float shotTimer;
    protected float rangedAttackTimeRemaining;
    protected TileBase currentSurface;
    protected float camFollowHorizontalSpeed;
    protected float camFollowVerticalSpeed;
    protected float verticalCameraOffsetTimer;
    protected InventoryController inventoryController;

    protected Checkpoint lastCheckpoint = null;
    protected Vector2 startingPosition = Vector2.zero;
    protected bool startingFacingLeft = false;

    protected bool inPause = false;

    protected readonly int hashHorizontalSpeedParameter = Animator.StringToHash("HorizontalSpeed");
    protected readonly int hashVerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
    protected readonly int hashGroundedParameter = Animator.StringToHash("Grounded");
    // protected readonly int hashCrouchingPara = Animator.StringToHash("Crouching");
    protected readonly int hashPushingPara = Animator.StringToHash("Pushing");
    protected readonly int hashTimeoutParameter = Animator.StringToHash("Timeout");
    protected readonly int hashRespawnParameter = Animator.StringToHash("Respawn");
    protected readonly int hashDeadParameter = Animator.StringToHash("Dead");
    protected readonly int hashHurtParameter = Animator.StringToHash("Hurt");
    protected readonly int hashForcedRespawnParameter = Animator.StringToHash("ForcedRespawn");
    protected readonly int hashMeleeAttackParameter = Animator.StringToHash("MeleeAttack");
    protected readonly int hashRangedAttackParameter = Animator.StringToHash("RangedAttack");
    protected readonly int hashWallSlideParameter = Animator.StringToHash("WallSlide");

    protected const float minHurtJumpAngle = 0.001f;
    protected const float maxHurtJumpAngle = 89.999f;
    protected const float groundStickVelocityMultiplier = 3f;

    void Awake()
    {
        playerInstance = this;
        characterController2D = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
        // _collider = GetComponent<CapsuleCollider2D>();
        inventoryController = GetComponent<InventoryController>();

        currentBulletSpawnPoint = spriteOriginallyFacesLeft ? facingLeftBulletSpawnPoint : facingRightBulletSpawnPoint;
    }

    void Start()
    {
        hurtJumpAngle = Mathf.Clamp(hurtJumpAngle, minHurtJumpAngle, maxHurtJumpAngle);
        tanHurtJumpAngle = Mathf.Tan(Mathf.Deg2Rad * hurtJumpAngle);
        flickeringWait = new WaitForSeconds(flickeringDuration);

        meleeDamager.DisableDamage();

        shotSpawnGap = 1f / shotsPerSecond;
        nextShotTime = Time.time;
        shotSpawnWait = new WaitForSeconds(shotSpawnGap);

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        Pushable pushable = other.GetComponent<Pushable>();
        if (pushable != null)
        {
            currentPushables.Add(pushable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Pushable pushable = other.GetComponent<Pushable>();
        if (pushable != null)
        {
            if (currentPushables.Contains(pushable))
                currentPushables.Remove(pushable);
        }
    }

    void Update()
    {
        if (PlayerInput.Instance.Pause.Down)
        {
            if (!inPause)
            {
                if (ScreenFader.IsFading)
                    return;

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
        animator.SetFloat(hashHorizontalSpeedParameter, moveVector.x);
        animator.SetFloat(hashVerticalSpeedParameter, moveVector.y);
        UpdateBulletSpawnPointPositions();
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


    // CAMERA FOLLOW

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
            currentBulletSpawnPoint = facingLeftBulletSpawnPoint;
        }
        else if (faceRight)
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingRightBulletSpawnPoint;
        }
    }

    public void UpdateFacing(bool faceLeft)
    {
        if (faceLeft)
        {
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingLeftBulletSpawnPoint;
        }
        else
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingRightBulletSpawnPoint;
        }
    }

    public float GetFacing()
    {
        return spriteRenderer.flipX != spriteOriginallyFacesLeft ? -1f : 1f;
    }

    public void GroundHorizontalMovement(bool useInput, float speedScale = 1f)
    {
        float desiredSpeed = useInput ? PlayerInput.Instance.Horizontal.Value * maxSpeed * speedScale : 0f;
        float acceleration = useInput && PlayerInput.Instance.Horizontal.ReceivingInput ? groundAcceleration : groundDeceleration;
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

    public bool CheckGrounded()
    {
        bool wasGrounded = animator.GetBool(hashGroundedParameter);
        bool grounded = characterController2D.Grounded;

        /*
        if (grounded)
        {
            FindCurrentSurface();

            if (!wasGrounded && moveVector.y < -1.0f)
            {
                landingAudioPlayer.PlayRandomSound(currentSurface);
            }
        }
        else
        {
            currentSurface = null;
        }
        */

        animator.SetBool(hashGroundedParameter, grounded);

        return grounded;
    }

    public void FindCurrentSurface()
    {
        Collider2D groundCollider = characterController2D.GroundColliders[0];

        if (groundCollider == null)
        {
            groundCollider = characterController2D.GroundColliders[1];
        }

        if (groundCollider == null)
        {
            return;
        }

        TileBase b = PhysicsHelper.FindTileForOverride(groundCollider, transform.position, Vector2.down);
        if (b != null)
        {
            currentSurface = b;
        }
    }

    public void CheckForPushing()
    {
        bool pushableOnCorrectSide = false;
        Pushable previousPushable = currentPushable;

        currentPushable = null;

        if (currentPushables.Count > 0)
        {
            bool movingRight = PlayerInput.Instance.Horizontal.Value > float.Epsilon;
            bool movingLeft = PlayerInput.Instance.Horizontal.Value < -float.Epsilon;

            for (int i = 0; i < currentPushables.Count; i++)
            {
                float pushablePosX = currentPushables[i].pushablePosition.position.x;
                float playerPosX = transform.position.x;
                if (pushablePosX < playerPosX && movingLeft || pushablePosX > playerPosX && movingRight)
                {
                    pushableOnCorrectSide = true;
                    currentPushable = currentPushables[i];
                    break;
                }
            }

            if (pushableOnCorrectSide)
            {
                Vector2 moveToPosition = movingRight ? currentPushable.playerPushingRightPosition.position : currentPushable.playerPushingLeftPosition.position;
                moveToPosition.y = characterController2D.Rigidbody.position.y;
                characterController2D.Teleport(moveToPosition);
            }
        }

        if (previousPushable != null && currentPushable != previousPushable)
        {
            previousPushable.EndPushing();
        }

        animator.SetBool(hashPushingPara, pushableOnCorrectSide);
    }

    public void MovePushable()
    {
        if (currentPushable && currentPushable.Grounded)
        {
            currentPushable.Move(moveVector * Time.deltaTime);
        }
    }

    public void StartPushing()
    {
        if (currentPushable)
        {
            currentPushable.StartPushing();
        }
    }

    public void StopPushing()
    {
        if (currentPushable)
        {
            currentPushable.EndPushing();
        }
    }

    public void AirHorizontalMovement()
    {
        float desiredSpeed = PlayerInput.Instance.Horizontal.Value * maxSpeed;
        float acceleration;

        if (PlayerInput.Instance.Horizontal.ReceivingInput)
        {
            acceleration = groundAcceleration * airborneAccelProportion;
        }
        else
        {
            acceleration = groundDeceleration * airborneDecelProportion;
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
        return moveVector.y < 0f && !animator.GetBool(hashGroundedParameter);
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


    // WALL SLIDE

    public bool CheckForWallSlideInput()
    {
        return (GetFacing() == -1 && PlayerInput.Instance.Horizontal.Value < 0 || GetFacing() == 1 && PlayerInput.Instance.Horizontal.Value > 0);
    }
    public void WallSlide()
    {
        animator.SetBool(hashWallSlideParameter, true);
    }


    // MELEE ATTACK

    public bool CheckForMeleeAttackInput()
    {
        return PlayerInput.Instance.MeleeAttack.Down;
    }

    public void MeleeAttack()
    {
        animator.SetTrigger(hashMeleeAttackParameter);
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


    // RANGED ATTACK

    protected void UpdateBulletSpawnPointPositions()
    {
        if (rightBulletSpawnPointAnimated)
        {
            Vector2 leftPosition = facingRightBulletSpawnPoint.localPosition;
            leftPosition.x *= -1f;
            facingLeftBulletSpawnPoint.localPosition = leftPosition;
        }
        else
        {
            Vector2 rightPosition = facingLeftBulletSpawnPoint.localPosition;
            rightPosition.x *= -1f;
            facingRightBulletSpawnPoint.localPosition = rightPosition;
        }
    }

    protected IEnumerator Shoot()
    {
        while (PlayerInput.Instance.RangedAttack.Held)
        {
            if (Time.time >= nextShotTime)
            {
                SpawnBullet();
                nextShotTime = Time.time + shotSpawnGap;
            }
            yield return null;
        }
    }

    protected void SpawnBullet()
    {
        Vector2 testPosition = transform.position;
        testPosition.y = currentBulletSpawnPoint.position.y;
        Vector2 direction = (Vector2)currentBulletSpawnPoint.position - testPosition;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit2D[] results = new RaycastHit2D[12];
        if (Physics2D.Raycast(testPosition, direction, characterController2D.ContactFilter, results, distance) > 0)
        {
            return;
        }

        BulletObject bullet = bulletPool.Pop(currentBulletSpawnPoint.position);
        bool facingLeft = currentBulletSpawnPoint == facingLeftBulletSpawnPoint;
        bullet.rigidbody2D.velocity = new Vector2(facingLeft ? -bulletSpeed : bulletSpeed, 0f);
        bullet.spriteRenderer.flipX = facingLeft ^ bullet.bullet.spriteOriginallyFacesLeft;
    }

    public bool CheckForRangedAttackOut()
    {
        bool rangedAttack = false;

        if (PlayerInput.Instance.RangedAttack.Held)
        {
            rangedAttack = true;
            animator.SetBool(hashRangedAttackParameter, true);
            rangedAttackTimeRemaining = holdingGunTimeoutDuration;
        }
        else
        {
            rangedAttackTimeRemaining -= Time.deltaTime;

            if (rangedAttackTimeRemaining <= 0f)
            {
                animator.SetBool(hashRangedAttackParameter, false);
            }
        }

        return rangedAttack;
    }

    public void CheckAndFireGun()
    {
        if (PlayerInput.Instance.RangedAttack.Held && animator.GetBool(hashRangedAttackParameter))
        {
            if (shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(Shoot());
            }
        }

        if ((PlayerInput.Instance.RangedAttack.Up || !animator.GetBool(hashRangedAttackParameter)) && shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    public void ForceNotRangedAttack()
    {
        animator.SetBool(hashRangedAttackParameter, false);
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

        animator.SetTrigger(hashHurtParameter);

        if (damageable.CurrentHealth > 0 && damager.forceRespawn)
        {
            animator.SetTrigger(hashForcedRespawnParameter);
        }

        animator.SetBool(hashGroundedParameter, false);

        if (damager.forceRespawn && damageable.CurrentHealth > 0)
        {
            StartCoroutine(DieRespawnCoroutine(false, true));
        }
    }


    // DIE

    public void OnDie()
    {
        animator.SetTrigger(hashDeadParameter);

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

        
        animator.ResetTrigger(hashHurtParameter);
        if (flickerCoroutine != null)
        {
            StopFlickering();
        }
        animator.SetTrigger(hashRespawnParameter);
        

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
