using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("설정")]
    public float bounceForce = 15f; // 튀어 오르는 힘 (15~20 추천)

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. 위에서 밟았는지 확인 (발판보다 플레이어 발이 위에 있어야 함)
            // (간단하게 충돌 지점의 법선 벡터로 확인)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // 플레이어가 위에서 내려찍음
                {
                    Bounce(collision.gameObject);
                    break;
                }
            }
        }
    }

    void Bounce(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // ★ 핵심: 기존 낙하 속도를 0으로 만들어야 점프 높이가 일정함
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // 위로 힘을 가함 (Impulse는 순간적인 힘)
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }

        // 애니메이션 재생 (스프링 팅기는 동작)
        if (anim != null)
        {
            anim.Play("Jump_Bounce");
        }
    }
}