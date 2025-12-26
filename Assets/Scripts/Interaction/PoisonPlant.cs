using UnityEngine;

public class PoisonPlant : MonoBehaviour
{
    [Header("설정")]
    public GameObject ballPrefab; // 독침 프리팹 연결
    public Transform firePoint;     // 독침이 생성될 위치 (입)
    public float fireRate = 2f;     // 발사 간격 (초)

    private float timer = 0f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            anim.Play("Fire");
            timer = 0f;
        }
    }

    public void Shoot()
    {
        // 발사 위치(firePoint)에서 총알 생성
        // 식물이 회전해 있으면 총알도 그 방향으로 나감 (rotation)
        Instantiate(ballPrefab, firePoint.position, firePoint.rotation);

    }
}