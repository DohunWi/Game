using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    public GameObject cam;
    
    [Header("Parallax Power")]
    [Tooltip("X: 좌우 움직임, Y: 상하 움직임 (1이면 카메라 고정, 0이면 안 따라옴)")]
    public Vector2 parallaxEffectMultiplier; // (x, y) 값을 따로 설정

    private float length;
    private float startposX;
    private float startposY;

    void Start()
    {
        startposX = transform.position.x;
        startposY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // --- X축 (좌우) : 무한 스크롤 적용 ---
        float tempX = (cam.transform.position.x * (1 - parallaxEffectMultiplier.x));
        float distX = (cam.transform.position.x * parallaxEffectMultiplier.x);

        // --- Y축 (상하) : 무한 스크롤 없음 (한계 존재) ---
        // Y축은 단순히 시작 위치 + (카메라 이동량 * 비율)만큼만 움직임
        float distY = (cam.transform.position.y * parallaxEffectMultiplier.y);

        // 최종 위치 적용
        transform.position = new Vector3(startposX + distX, startposY + distY, transform.position.z);

        // X축 무한 반복 로직 (배경이 끊기지 않게)
        if (tempX > startposX + length) startposX += length;
        else if (tempX < startposX - length) startposX -= length;
    }
}