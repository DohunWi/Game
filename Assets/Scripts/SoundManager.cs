using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // 배경음악용
    [SerializeField] private AudioSource sfxSource; // 효과음용 (공격, 점프, 피격 등)

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 배경음악 재생
    public void PlayBGM(AudioClip clip)
    {
        // 만약 지금 틀어져 있는 노래가 요청한 노래랑 똑같다면?
        // 아무것도 하지 말고 리턴 (음악이 끊기지 않고 계속 이어짐)
        if (bgmSource.clip == clip) return; 

        bgmSource.clip = clip;
        bgmSource.loop = true; // 무한 반복
        bgmSource.volume = 0.5f;
        bgmSource.Play();
    }

    // 효과음 재생 (겹쳐도 소리 나게 PlayOneShot 사용)
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, masterVolume);
    }
}