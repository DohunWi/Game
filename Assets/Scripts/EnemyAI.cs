using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    #region Variables
    // --- FSM 관련 ---
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyHitState HitState { get; private set; }

    // --- 컴포넌트 ---
    public Animator Anim { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    private SpriteRenderer sr; // 깜빡임 효과용

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float detectRange = 5f;
    
    [Header("Health Settings")] // [추가] 체력 설정
    public float maxHealth = 3f;
    private float currentHealth;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;   
    public float attackCooldown = 2f;  
    public float damage = 1f;          
    [HideInInspector] public float lastAttackTime; 

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.2f; 
    public Vector2 knockbackSpeed = new Vector2(5, 3); // x, y 힘

    [Header("Audio Clips")] // [추가] 소리 파일 넣을 곳
    [SerializeField] private AudioClip attackSound; // 공격 기합/휘두르기
    [SerializeField] private AudioClip hitSound;    // 맞았을 때 비명
    [SerializeField] private AudioClip dieSound;    // 사망 소리
    [SerializeField] private AudioClip idleSound; // [추가] 평소 울음소리

    [Header("References")]
    public Transform playerCheck;  
    public LayerMask playerLayer;  
    
    // [추가] 이펙트 프리팹을 넣을 변수
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private GameObject hitPos; 
    public Transform Target { get; private set; } 
    private int facingDirection = 1; 

    [HideInInspector] public float startTime; 
    #endregion

    private void Awake()
    {
        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine, "Idle");
        ChaseState = new EnemyChaseState(this, StateMachine, "Chase"); 
        AttackState = new EnemyAttackState(this, StateMachine, "Attack");
        HitState = new EnemyHitState(this, StateMachine, "Hit");

        Anim = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth; // [추가] 체력 초기화

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Target = player.transform;
        }

        StateMachine.Initialize(IdleState);
        StartCoroutine(IdleSoundRoutine());
    }

    private void Update()
    {
        // 죽었으면 업데이트 중단 (선택 사항)
        if (currentHealth <= 0) return;

        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    // --- 외부 호출 함수 (피격) ---
    public void TakeDamage(float damage, Transform damageSource)
    {
        if (currentHealth <= 0) return; // 이미 죽었으면 무시

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(hitSound);
        // 1. 체력 감소
        currentHealth -= damage;
        Debug.Log($"몬스터 체력: {currentHealth}/{maxHealth}");

        // 2. 깜빡임 효과 실행
        StartCoroutine(FlashRoutine());

        if (hitVFXPrefab != null)
        {
            // Instantiate(프리팹, 생성 위치, 회전값);
            // 위치는 현재 몬스터 위치(transform.position), 회전은 기본값(Quaternion.identity)
            Instantiate(hitVFXPrefab, hitPos.transform.position, Quaternion.identity);
        }

        // 3. 사망 체크
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 4. 살아있다면 넉백 상태로 전환
        int dir = (transform.position.x < damageSource.position.x) ? -1 : 1;
        HitState.SetKnockbackDirection(dir);
        StateMachine.ChangeState(HitState);
    }

    // [추가] 사망 처리 함수
    private void Die()
    {
        Debug.Log("몬스터 사망!");
        
        // 1. 더 이상 움직이지 못하게 고정
        SetVelocity(0);
        Rb.linearVelocity = Vector2.zero;
        Rb.bodyType = RigidbodyType2D.Kinematic; // 중력 영향 끄기 (선택사항)

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(dieSound);

        // 2. 더 이상 맞거나 길을 막지 않게 콜라이더 끄기
        GetComponent<Collider2D>().enabled = false;
        
        // 3. AI 로직 정지 (더 이상 쫓아오거나 공격 안 함)
        this.enabled = false; 

        // 4. 사망 애니메이션 재생
        Anim.Play("Die");

        // 5. 오브젝트 삭제 (애니메이션이 끝날 때쯤 삭제)
        // 팁: Death 애니메이션 길이를 확인하고 그 시간만큼 적어주세요 (예: 1초)
        Destroy(gameObject, 1.2f);
    }

    // [추가] 피격 시 하얗게 깜빡이는 연출
    private IEnumerator FlashRoutine()
    {
        if (sr != null)
        {
            // 원래 색 저장
            Color originalColor = sr.color;

            // 반투명한 빨간색으로 변경 (알파값을 0.5로)
            sr.color = new Color(1f, 0.5f, 0.5f, 0.7f); 
            
            yield return new WaitForSeconds(0.1f);
            
            // 원래 색 복구
            sr.color = originalColor;
        }
    }
    private IEnumerator IdleSoundRoutine()
    {
        while (currentHealth > 0)
        {
            // 3초에서 7초 사이 랜덤 대기
            float waitTime = Random.Range(3f, 7f);
            yield return new WaitForSeconds(waitTime);

            // 소리 파일이 있고, 너무 멀리 있지 않을 때만 재생 (선택사항)
            // (화면 밖 몬스터 소리가 들리면 시끄러우니까요)
            if (idleSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(idleSound);
            }
        }
    }

    // --- 헬퍼 함수들 ---
    public void SetVelocity(float xVelocity)
    {
        if(Rb != null)
            Rb.linearVelocity = new Vector2(xVelocity, Rb.linearVelocity.y);
    }

    public bool CheckPlayerInSight()
    {
        if (playerCheck == null) return false;
        return Physics2D.OverlapCircle(playerCheck.position, detectRange, playerLayer);
    }

    public bool CheckPlayerInAttackRange()
    {
        if (playerCheck == null) return false;
        return Physics2D.OverlapCircle(playerCheck.position, attackRange, playerLayer);
    }

    public void Flip(float xDirection)
    {
        if ((xDirection > 0 && facingDirection == -1) || (xDirection < 0 && facingDirection == 1))
        {
            facingDirection *= -1;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCheck.position, detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerCheck.position, attackRange);
        }
    }
    public void PlayAnim(string stateName)
    {
        // 현재 재생 중인 애니메이션의 상태 정보를 가져옴
        AnimatorStateInfo stateInfo = Anim.GetCurrentAnimatorStateInfo(0);

        // 만약 요청한 애니메이션이 이미 재생 중이라면? -> 아무것도 안 하고 리턴!
        if (stateInfo.IsName(stateName)) return;

        // 아니라면 -> 재생!
        Anim.Play(stateName);
    }
    public void AnimationAttackTrigger()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(attackSound);
        // 공격 범위 안에 있는 플레이어 찾기
        Collider2D hit = Physics2D.OverlapCircle(playerCheck.position, attackRange, playerLayer);
        
        if (hit != null)
        {
            // 플레이어 체력 스크립트 가져오기
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            
            if (health != null)
            {
                // 데미지 주기
                health.TakeDamage((int)damage, transform);
                Debug.Log("타격 성공!");
            }
        }
    }
}