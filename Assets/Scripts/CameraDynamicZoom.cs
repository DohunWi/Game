using UnityEngine;
using Unity.Cinemachine;

public class CameraDynamicZoom : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private CinemachineCamera targetCamera; 
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Size Settings")]
    [SerializeField] private float defaultSize = 6f; // 평소 (줌인 상태)
    [SerializeField] private float runSize = 8f;     // 달릴 때 (줌아웃 상태)
    [SerializeField] private float speedThreshold = 0.1f; 

    [Header("Speed Settings")]
    [Tooltip("달리기 시작할 때 멀어지는 속도 (빠르게)")]
    [SerializeField] private float zoomOutSpeed = 3f; 
    
    [Tooltip("멈췄을 때 원래대로 돌아오는 속도 (느리게)")]
    [SerializeField] private float zoomInSpeed = 0.5f; 

    private void Update()
    {
        if (targetCamera == null || playerRb == null) return;

        // 1. 현재 렌즈 설정 가져오기
        var lensSettings = targetCamera.Lens;
        float currentSize = lensSettings.OrthographicSize;

        // 2. 목표 사이즈 결정
        float currentSpeed = Mathf.Abs(playerRb.linearVelocity.x);
        float targetSize = (currentSpeed > speedThreshold) ? runSize : defaultSize;

        // 3. 상황에 맞는 속도 선택 (핵심 로직 변경)
        // 목표가 현재보다 크면(=줌아웃 중이면) OutSpeed, 작으면(=줌인 중이면) InSpeed 사용
        float selectSpeed = (targetSize > currentSize) ? zoomOutSpeed : zoomInSpeed;

        // 4. 부드럽게 값 변경
        lensSettings.OrthographicSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * selectSpeed);

        // 5. 적용
        targetCamera.Lens = lensSettings;
    }
}