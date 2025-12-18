using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("1. Movement & Jump")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 16f;
    [Range(0, 1)] [SerializeField] private float jumpCutMultiplier = 0.5f; // 점프 키 뗐을 때 감속 (낮은 점프)
    [SerializeField] private float gravityScale = 4.5f; // 기본 중력 (떨어질 때 빠르 게)
    [SerializeField] private float fallGravityMult = 1.5f; // 낙하 시 추가 가속 (묵직한 느낌)

    [Header("2. Dash")]
    [SerializeField] private float dashPower = 24f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("3. Advanced Physics (Feel)")]
    [SerializeField] private float coyoteTime = 0.1f; // 땅에서 떨어져도 점프 가능한 시간
    [SerializeField] private float jumpBufferTime = 0.1f; // 땅에 닿기 전 점프 미리 입력 허용 시간
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform footPos; // 발바닥 위치 (빈 오브젝트)
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.2f); // 접지 판정 박스 크기

    // 내부 변수
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    // 상태 플래그
    private bool isDashing;
    private bool canDash = true;
    private bool isJumping;

    // 고급 점프용 타이머
    private float lastGroundedTime;
    private float lastJumpPressedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 중력 스케일 초기 설정
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        // 타이머 업데이트
        lastGroundedTime -= Time.deltaTime;
        lastJumpPressedTime -= Time.deltaTime;

        if (isDashing) return;

        // 1. 접지 판정 (BoxCast 이용 - Raycast보다 안정적)
        // 발바닥 위치에서 아래로 쏘는 사각형 레이캐스트
        bool isGrounded = Physics2D.OverlapBox(footPos.position, groundCheckSize, 0f, groundLayer);
        
        // 땅에 닿아있으면 코요테 타임 갱신 (항상 점프 가능 상태로 유지)
        if (isGrounded && rb.linearVelocity.y <= 0) // 올라가는 중이 아닐 때만
        {
            lastGroundedTime = coyoteTime;
            isJumping = false;
        }

        // 2. 점프 실행 로직 (코요테 타임 > 0  AND  점프 버퍼 > 0)
        if (lastJumpPressedTime > 0 && lastGroundedTime > 0 && !isJumping)
        {
            PerformJump();
        }

        // 3. 방향 전환
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);
        }

        // 4. 낙하 시 중력 조절 (더 묵직한 조작감)
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMult;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        // 이동 적용
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void PerformJump()
    {
        isJumping = true;
        lastGroundedTime = 0; // 점프 했으니 코요테 타임 즉시 소멸
        lastJumpPressedTime = 0; // 점프 했으니 버퍼 즉시 소멸

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // 기존 Y 속도 초기화
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // --- Input System Events ---

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        // 키를 누른 순간: 버퍼 타임 설정
        if (value.isPressed)
        {
            lastJumpPressedTime = jumpBufferTime;
        }
        // 키를 뗐을 때 (가변 점프): 점프 중이라면 속도를 깎아서 낮게 점프
        else if (rb.linearVelocity.y > 0 && isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("공격! (애니메이션 연결 필요)");
            // 여기에 공격 로직 추가
        }
    }

    // --- Coroutines ---
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // --- Editor Debugging ---
    // 씬 뷰에서 접지 판정 박스를 눈으로 확인하기 위함
    private void OnDrawGizmos()
    {
        if (footPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(footPos.position, groundCheckSize);
        }
    }
}