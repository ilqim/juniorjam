using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Yelpaze: tuşa her basışta bir kez sallanır ve yaprağa kısa bir rüzgâr üfler.
// Ne kadar hızlı basılırsa üfleme o kadar güçlü olur.
// Soldaki yelpaze sağa, sağdaki yelpaze sola iter. Güç yaprağa olan uzaklığa da bağlıdır.
public class Fan : MonoBehaviour
{
    public enum PushDirection { Right = 1, Left = -1 }

    [Header("Kontrol")]
    [Tooltip("Sol yelpaze için A, sağ yelpaze için D")]
    public KeyCode key = KeyCode.A;

    [Header("Yön")]
    [Tooltip("Soldaki yelpaze: Right, sağdaki yelpaze: Left")]
    public PushDirection direction = PushDirection.Right;

    [Header("Üfleme")]
    [Tooltip("Yavaş basışta tek bir sallamanın gücü")]
    public float baseGust = 20f;

    [Tooltip("En hızlı basışta güç kaç katına çıkar")]
    public float maxRateMultiplier = 2.5f;

    [Tooltip("Saniyede bu kadar basış = en yüksek güç")]
    public float maxTapsPerSecond = 8f;

    [Tooltip("Basış hızını ölçmek için geriye bakılan süre (saniye)")]
    public float rateWindow = 1f;

    [Tooltip("Bir sallamanın rüzgârının sürdüğü süre (saniye)")]
    public float gustDuration = 0.3f;

    [Header("Uzaklık")]
    [Tooltip("Güç bu mesafeye kadar giderek azalır, daha uzakta en düşük seviyede sabit kalır")]
    public float falloffDistance = 15f;

    [Tooltip("En uzakta gücün yüzde kaçı kalır (0-1). 0'dan büyük olduğu sürece yaprak her yerde hareket eder")]
    [Range(0.05f, 1f)] public float minDistanceMultiplier = 0.3f;

    [Header("Efektler (opsiyonel)")]
    [Tooltip("Animator'da bu isimde bir Trigger varsa her basışta tetiklenir")]
    public Animator animator;
    public string waveTrigger = "Wave";
    public ParticleSystem windParticles;
    [Tooltip("Her basışta çıkacak parçacık sayısı")]
    public int particlesPerTap = 10;
    public AudioSource audioSource;
    public AudioClip waveSound;

    // Son basışların zamanları (basış hızını hesaplamak için)
    readonly System.Collections.Generic.Queue<float> tapTimes = new System.Collections.Generic.Queue<float>();

    float currentGust;   // şu anki rüzgâr gücü, zamanla sönümlenir
    float gustStart;     // son sallamanın başlangıç gücü

    // Arayüzde göstermek istersen: 0 = yavaş, 1 = en hızlı
    public float RateNormalized { get; private set; }

    void Update()
    {
        if (WasKeyPressed())
            Wave();

        // Eski basışları listeden at
        while (tapTimes.Count > 0 && Time.time - tapTimes.Peek() > rateWindow)
            tapTimes.Dequeue();

        // Rüzgârı zamanla söndür
        if (currentGust > 0f)
            currentGust = Mathf.Max(0f, currentGust - gustStart / gustDuration * Time.deltaTime);
    }

    void Wave()
    {
        tapTimes.Enqueue(Time.time);

        float tapsPerSecond = tapTimes.Count / rateWindow;
        RateNormalized = Mathf.Clamp01(tapsPerSecond / maxTapsPerSecond);

        float multiplier = Mathf.Lerp(1f, maxRateMultiplier, RateNormalized);
        gustStart = baseGust * multiplier;

        // Hızlı basışta rüzgâr kesilmesin, güçlü olan kalsın
        currentGust = Mathf.Max(currentGust, gustStart);

        PlayEffects();
    }

    // Yaprak her fizik adımında bunu çağırır ve yelpazenin ona uyguladığı yatay gücü alır.
    public float GetPush(Vector2 leafPosition)
    {
        if (currentGust <= 0f) return 0f;

        // Menzil sınırı yok: yakınken tam güç, uzaklaştıkça azalır ama hiç sıfırlanmaz
        float distance = Vector2.Distance(transform.position, leafPosition);
        float t = Mathf.Clamp01(distance / falloffDistance);
        float distanceFactor = Mathf.Lerp(1f, minDistanceMultiplier, t);

        return currentGust * distanceFactor * (int)direction;
    }

    bool WasKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            Key k = ToInputSystemKey(key);
            if (k != Key.None) return Keyboard.current[k].wasPressedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        return false;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    static Key ToInputSystemKey(KeyCode code)
    {
        if (code >= KeyCode.A && code <= KeyCode.Z)
            return Key.A + (code - KeyCode.A);

        switch (code)
        {
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            case KeyCode.Space: return Key.Space;
            default: return Key.None;
        }
    }
#endif

    void PlayEffects()
    {
        if (animator != null && !string.IsNullOrEmpty(waveTrigger))
            animator.SetTrigger(waveTrigger);

        if (windParticles != null)
            windParticles.Emit(Mathf.RoundToInt(particlesPerTap * Mathf.Lerp(1f, maxRateMultiplier, RateNormalized)));

        if (audioSource != null && waveSound != null)
        {
            audioSource.pitch = Mathf.Lerp(1f, 1.3f, RateNormalized); // hızlandıkça ses biraz incelir
            audioSource.PlayOneShot(waveSound);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, falloffDistance);
    }
}