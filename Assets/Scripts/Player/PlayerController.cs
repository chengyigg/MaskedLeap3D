using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("配置")]
    public PlayerConfig config;

    [Header("视觉模型")]
    public Transform modelTransform;
    public float squashAmount = 0.15f;
    public float squashDuration = 0.15f;

    [Header("重生")]
    public Transform spawnPoint;

    private CharacterController controller;
    public Vector3 velocity;
    public int jumpCount = 0;

    private float bounceLockTimer = 0f;
    private Vector3 initialSpawnPos;
    private MovingPlatform currentPlatform;

    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool wasGrounded = false;
    private float jumpOffPlatformTimer = 0f;

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private Vector3 modelOriginalScale;
    private float squashTimer = 0f;

    public bool IsGrounded => controller.isGrounded || currentPlatform != null;
    public bool IsDashing => dashTimer > 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        initialSpawnPos = transform.position;
        modelOriginalScale = modelTransform != null ? modelTransform.localScale : Vector3.one;
    }

    void Update()
    {
        if (config == null) { Debug.LogError("config 未赋值"); return; }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Camera.main.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = Camera.main.transform.right; camRight.y = 0f; camRight.Normalize();
        if (camForward != Vector3.zero) transform.rotation = Quaternion.LookRotation(camForward);

        DetectPlatform();
        bool grounded = controller.isGrounded || currentPlatform != null;

        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (dashTimer > 0f) dashTimer -= Time.deltaTime;

        // 【冲刺触发 → 音效】
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && dashTimer <= 0f)
        {
            dashTimer = config.dashDuration;
            dashCooldownTimer = config.dashCooldown;
            dashDirection = camForward;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDash();
        }

        if (dashTimer > 0f)
            controller.Move(dashDirection * config.dashSpeed * Time.deltaTime);
        else
        {
            Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;
            float speed = grounded ? config.moveSpeed : config.moveSpeed * config.airControlMultiplier;
            Vector3 moveVel = moveDir * speed;
            if (currentPlatform != null) moveVel += currentPlatform.platformVelocity;
            controller.Move(moveVel * Time.deltaTime);
        }

        if (grounded) coyoteTimer = config.coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferTimer = config.jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        if (bounceLockTimer > 0f) bounceLockTimer -= Time.deltaTime;
        else
        {
            if (grounded) { jumpCount = 0; if (velocity.y < 0) velocity.y = -2f; }
            else velocity.y += config.gravity * Time.deltaTime;
        }

        // 【跳跃触发 → 音效（普通跳跃 vs 二段跳）】
        bool canFirst = coyoteTimer > 0f && jumpCount == 0;
        bool canSecond = jumpCount > 0 && jumpCount < config.maxJumpCount;
        if (jumpBufferTimer > 0f && (canFirst || canSecond))
        {
            bool isDoubleJump = jumpCount > 0;

            // 【修改】根据是几段跳，用不同的高度
            float jumpH = isDoubleJump ? config.doubleJumpHeight : config.jumpHeight;
            velocity.y = Mathf.Sqrt(jumpH * -2f * config.gravity);

            jumpCount++;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            currentPlatform = null;
            jumpOffPlatformTimer = 0.2f;

            ParticleBurst.Play(transform.position + Vector3.up * 0.1f, new Color(0.7f, 0.9f, 1f), 12, 3f);

            if (AudioManager.Instance != null)
            {
                if (isDoubleJump) AudioManager.Instance.PlayDoubleJump();
                else AudioManager.Instance.PlayJump();
            }
        }

        if (Input.GetButtonUp("Jump") && velocity.y > 0)
            velocity.y *= config.variableJumpMultiplier;

        controller.Move(velocity * Time.deltaTime);

        // 【落地 → 音效】
        if (!wasGrounded && grounded)
        {
            squashTimer = squashDuration;
            ParticleBurst.Play(transform.position + Vector3.up * 0.1f, new Color(0.8f, 0.8f, 0.8f), 20, 2.5f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLand();
        }
        wasGrounded = grounded;

        UpdateSquash();
    }

    void DetectPlatform()
    {
        if (jumpOffPlatformTimer > 0f)
        {
            jumpOffPlatformTimer -= Time.deltaTime;
            currentPlatform = null;
            return;
        }
        currentPlatform = null;
        Vector3 feetPos = transform.position + controller.center - Vector3.up * (controller.height / 2f);
        RaycastHit hit;
        if (Physics.Raycast(feetPos + Vector3.up * 0.1f, Vector3.down, out hit, 0.3f))
        {
            MovingPlatform mp = hit.collider.GetComponent<MovingPlatform>();
            if (mp != null) currentPlatform = mp;
        }
    }

    void UpdateSquash()
    {
        if (modelTransform == null) return;
        if (squashTimer > 0f)
        {
            squashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(squashTimer / squashDuration);
            float sy = 1f - squashAmount * t;
            float sxz = 1f + squashAmount * 0.5f * t;
            modelTransform.localScale = new Vector3(
                modelOriginalScale.x * sxz, modelOriginalScale.y * sy, modelOriginalScale.z * sxz);
        }
        else modelTransform.localScale = modelOriginalScale;
    }

    public void ApplyBounce(float force)
    {
        velocity.y = force;
        jumpCount = 0;
        bounceLockTimer = 0.2f;
        coyoteTimer = 0f;
        GameEvents.PlayerBounced();
    }

    public void Respawn()
    {
        // 【死亡 → 音效】
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHurt();
        GameEvents.PlayerDied();

        if (DeathFade.Instance != null)
            StartCoroutine(DeathFade.Instance.DeathSequence(DoRespawn));
        else DoRespawn();
    }

    void DoRespawn()
    {
        currentPlatform = null;
        controller.enabled = false;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else transform.position = initialSpawnPos;

        velocity = Vector3.zero;
        jumpCount = 0;
        bounceLockTimer = 0f;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        dashTimer = 0f;
        dashCooldownTimer = 0f;

        controller.enabled = true;

        // 【重生 → 音效】
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRespawn();
    }
}