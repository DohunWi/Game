using UnityEngine;

public class PoisonBall : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    public float lifeTime = 1.5f; 

    void Start()
    {
        // 아무것도 안 맞춰도 3초 뒤에는 사라짐 (메모리 절약)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 위쪽 방향(초록색 화살표)으로 계속 날아감
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 맞춤
        if (collision.CompareTag("Player"))
        {
            // 플레이어 스크립트에서 TakeDamage 함수 호출 (이전 함정 코드 참고)
            var player = collision.GetComponent<PlayerHealth>(); // 본인 스크립트 이름으로 변경!
            if (player != null) player.TakeDamage(damage, transform);
            
            Destroy(gameObject); // 총알 삭제
        }
        // 땅이나 벽에 닿음
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject); // 총알 삭제
        }
    }
}