 using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("이펙트가 유지될 시간(초). 애니메이션 길이보다 약간 길게 설정하세요.")]
    public float destroyDelay = 0.3f;

    void Start()
    {
        // 태어나자마자 "destroyDelay 초 후에 나를 파괴해라"라고 예약 걸기
        Destroy(gameObject, destroyDelay);
    }
}