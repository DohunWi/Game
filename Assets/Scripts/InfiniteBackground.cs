using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    [Header("설정")]
    public Transform mainCamera;
    public float parallaxFactor = 0.5f;
    
    [Header("위치 미세 조정")]
    public float heightOffset = 0f; // ★ 새로 추가: 배경 판의 높낮이 조절

    private MeshRenderer meshRenderer;
    private Material mat;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mat = meshRenderer.material;
        if (mainCamera == null) mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 1. 배경 판 위치 이동 (Y축에 heightOffset을 더해줌!)
        //    이제 Inspector에서 Height Offset 값을 조절하면 위아래로 움직입니다.
        transform.position = new Vector3(mainCamera.position.x, mainCamera.position.y + heightOffset, transform.position.z);

        // 2. 그림(텍스처) 무한 스크롤 (X축만)
        //    Y축 오프셋(mat.mainTextureOffset의 y)은 0으로 고정하는 게 깔끔합니다.
        float offsetX = mainCamera.position.x * parallaxFactor;
        mat.mainTextureOffset = new Vector2(offsetX, 0); 
    }
}