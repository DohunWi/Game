using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("1. Movement & Jump")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 16f;
    [Range(0, 1)] [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float gravityScale = 4.5f;
    [SerializeField] private float fallGravityMult = 1.5f;

    [Header("2. Dash")]
    [SerializeField] private float dashPower = 24f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("3. Advanced Physics")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform footPos;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.2f);

    [Header("Combat")]
    [SerializeField] private GameObject[] attackVFXs; // 1타, 2타, 3타 이펙트
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 1.0f; // 이 시간 안에 다음 공격해야 콤보 유지
    private int comboStep = 0; // 현재 콤보 단계 (0, 1, 2...)
    private float lastAttackTime; // 마지막 공격이 끝난 시간

    [Header("Audio")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip[] footstepSounds; // 배열로 만들어서 랜덤 재생
    [SerializeField] private float footstepRate = 0.3f; // 발소리 간격 (달리기 속도에 맞추세요)
    private float footstepTimer;

    [Header("State Checks")]
    public bool isGrounded;
    public bool isAttacking;
    public bool isDashing;
    [HideInInspector] public bool isHit; // PlayerHealth에서 제어

    // 내부 변수
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private string currentAnimName; // 중복 재생 방지용

    // 타이머 및 상태
    private bool canDash = true;
    private bool isJumping; // 점프 중인지 체크 (코요테 타임 로직용)
    private float lastGroundedTime;
    private float lastJumpPressedTime;

    // --- 애니메이션 이름 상수 (대소문자 정확해야 함) ---
    const string ANIM_IDLE = "Player_idle";
    const string ANIM_RUN = "Player_run";
    const string ANIM_JUMP = "Player_jump";
    const string ANIM_DASH = "Player_dash"; 
    const string ANIM_HIT = "Player_Hit";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        if (isHit) return; // 피격 중엔 조작 불가

        // 타이머 업데이트
        lastGroundedTime -= Time.deltaTime;
        lastJumpPressedTime -= Time.deltaTime;

        if (isDashing) return; // 대시 중엔 아래 로직 무시

        // 1. 접지 판정
        if (Physics2D.OverlapBox(footPos.position, groundCheckSize, 0f, groundLayer))
        {
            isGrounded = true;
            // 낙하 중이거나 바닥에 있을 때 코요테 타임 갱신
            if (rb.linearVelocity.y <= 0)
            {
                lastGroundedTime = coyoteTime;
                isJumping = false;
            }
        }
        else
        {
            isGrounded = false;
        }

        // 2. 점프 실행 (버퍼 && 코요테)
        if (lastJumpPressedTime > 0 && lastGroundedTime > 0 && !isJumping)
        {
            PerformJump();
        }

        // 3. 방향 전환
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);
        }

        // 4. 낙하 시 중력 조절
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMult;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        HandleFootsteps();
        // 5. 애니메이션 갱신
        HandleAnimations();
    }

    private void FixedUpdate()
    {
        if (isDashing || isHit) return;

        // 이동 적용 (Unity 6가 아니면 velocity 사용 권장)
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    private void HandleFootsteps()
    {
        // 조건: 땅에 닿아있음 && 움직이는 중 (속도가 있음) && 대시 중이 아님
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f && !isDashing)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                // 소리 재생
                if (footstepSounds.Length > 0 && SoundManager.Instance != null)
                {
                    // 랜덤한 발소리 골라서 재생 (훨씬 자연스러움)
                    int randIndex = Random.Range(0, footstepSounds.Length);
                    SoundManager.Instance.PlaySFX(footstepSounds[randIndex]);
                }

                // 타이머 리셋
                footstepTimer = footstepRate;
            }
        }
        else
        {
            // 멈추거나 공중에 뜨면 타이머를 0으로 만들어서, 착지하자마자 소리가 나게 함 (선택)
            footstepTimer = 0; 
        }
    }
    // 애니메이션 우선순위 관리
    private void HandleAnimations()
    {
        // 1순위: 대시
        if (isDashing)
        {
            PlayAnim(ANIM_DASH);
            return;
        }

        // 2순위: 공중 (점프/낙하)
        if (!isGrounded)
        {
            PlayAnim(ANIM_JUMP);
            return;
        }

        // 3순위: 달리기 (이동 입력이 있고 && 공격 중이 아닐 때)
        if (Mathf.Abs(moveInput.x) > 0.01f && !isAttacking)
        {
            PlayAnim(ANIM_RUN);
        }
        else
        {
            // 그 외: 대기
            PlayAnim(ANIM_IDLE);
        }
    }

    // [누락되었던 함수 추가]
    public void PlayAnim(string animName)
    {
        if (currentAnimName == animName) return;
        anim.Play(animName);
        currentAnimName = animName;
    }

    private void PerformJump()
    {
        isJumping = true;
        lastGroundedTime = 0;
        lastJumpPressedTime = 0;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Y속도 초기화 후 점프
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        // 점프 소리는 여기서 한 번만 (OnJump에서 중복 재생 방지)
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(jumpSound);
    }

    // --- Input Events ---

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            lastJumpPressedTime = jumpBufferTime;
        }
        // 가변 점프 (키 뗐을 때)
        else if (rb.linearVelocity.y > 0 && isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDashing && !isHit)
        {
            StartCoroutine(DashRoutine());
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttacking && !isDashing && !isHit)
        {
            // 1. 콤보 유지 시간 체크
            // (마지막 공격 끝난 후 1초가 지났으면 콤보 초기화)
            if (Time.time - lastAttackTime > comboResetTime)
            {
                comboStep = 0;
            }

            StartCoroutine(AttackRoutine());
        }
    }

    // --- Coroutines ---

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        
        // --- 1. 콤보 단계에 맞는 리소스 선택 ---
        // 배열 크기를 벗어나지 않게 안전장치 (나머지 연산 %)
        // 예: 이펙트가 3개인데 4타째가 되면 다시 0번으로
        int stepIndex = comboStep % attackVFXs.Length;

        // --- 2. 소리 재생 ---
        if (SoundManager.Instance != null && attackSounds.Length > 0) 
        {
            // 배열 인덱스 보호 (소리가 이펙트보다 적을 수도 있으니)
            int soundIndex = comboStep % attackSounds.Length;
            SoundManager.Instance.PlaySFX(attackSounds[soundIndex]);
        }

        // --- 3. VFX 켜기 ---
        GameObject currentVFX = attackVFXs[stepIndex];
        if (currentVFX != null) currentVFX.SetActive(true);

        // --- 4. 데미지 판정 (콤보마다 데미지 증가 가능!) ---
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy"))
            {
                EnemyAI enemyAI = enemyCollider.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // [팁] 3타(마지막) 공격은 데미지를 2배로 줄까요?
                    int damage = 1;
                    if (stepIndex == 2) damage = 2; // 3번째 공격은 2데미지!

                    enemyAI.TakeDamage(damage, transform); 
                }
            }
        }

        // --- 5. 대기 ---
        yield return new WaitForSeconds(attackDuration);

        // --- 6. 정리 ---
        if (currentVFX != null) currentVFX.SetActive(false);
        
        isAttacking = false;
        
        // [중요] 콤보 다음 단계로 증가
        comboStep++;
        // 마지막 공격 끝난 시간 기록 (이 시간부터 1초 카운트 시작)
        lastAttackTime = Time.time; 
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;

        PlayAnim(ANIM_DASH);
        
        if(SoundManager.Instance != null) 
            SoundManager.Instance.PlaySFX(dashSound);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        // 보고 있는 방향으로 대시
        rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    // 피격 시 호출할 함수
    public void OnHit()
    {
        isHit = true;
    }

    private void OnDrawGizmos()
    {
        if (footPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(footPos.position, groundCheckSize);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}