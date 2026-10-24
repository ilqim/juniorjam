using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Booster barı: zamanla dolar, doluyken tuşa basınca aktif olur.
// Aktifken bar boşalır ve yaprağın hızı artar. Bar bitince normale döner ve tekrar dolmaya başlar.
public class Booster : MonoBehaviour
{
    [Header("Referanslar")]
    public Leaf leaf;
    [Tooltip("Oyun müziğini çalan AudioSource")]
    public AudioSource musicSource;

    [Tooltip("Booster aktifken müzik hız çarpanı")]
    public float musicSpeedMultiplier = 2f;

    float normalPitch = 1f;

    [Header("Ayarlar")]
    [Tooltip("Barın boştan tama dolma süresi (saniye)")]
    public float fillTime = 15f;

    [Tooltip("Booster'ın aktif kalma süresi, yani dolu barın boşalma süresi (saniye)")]
    public float activeDuration = 4f;

    [Tooltip("Aktifken yaprağın hız çarpanı")]
    public float speedMultiplier = 2f;

    [Tooltip("Booster'ı aktif eden tuş")]
    public KeyCode key = KeyCode.Space;

    [Header("UI (birini kullanman yeterli)")]
    [Tooltip("Image Type: Filled olan bir bar görseli")]
    public Image barFill;
    [Tooltip("Ya da Slider (Min 0, Max 1)")]
    public Slider barSlider;
    [Tooltip("Bar dolunca gösterilecek obje, örneğin 'SPACE' yazısı (opsiyonel)")]
    public GameObject readyIndicator;
    [Tooltip("Indicator'ün yanıp sönme hızı (saniyede kaç kez)")]
    public float blinkSpeed = 4f;
    [Range(0f, 1f)] public float blinkMinAlpha = 0.2f;
    [Range(0f, 1f)] public float blinkMaxAlpha = 1f;

    CanvasGroup indicatorGroup;

    [Header("Olaylar (ses, efekt vb. için)")]
    public UnityEvent onReady;
    public UnityEvent onActivated;
    public UnityEvent onEnded;

    // 0 = boş, 1 = dolu
    public float Charge { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsReady => !IsActive && Charge >= 1f;

    bool stopped;

    void Awake()
    {
        if (leaf == null) leaf = FindFirstObjectByType<Leaf>();
        if (leaf != null) leaf.onFail.AddListener(Stop);

        // Indicator'ün içindeki her şeyin (yazı, görsel) opaklığını birlikte değiştirmek için
        if (readyIndicator != null)
        {
            indicatorGroup = readyIndicator.GetComponent<CanvasGroup>();
            if (indicatorGroup == null) indicatorGroup = readyIndicator.AddComponent<CanvasGroup>();
        }

        Charge = 0f;
        UpdateUI();
    }

    void Update()
    {
        if (stopped) return;

        if (IsActive)
        {
            // Aktifken boşal
            Charge -= Time.deltaTime / activeDuration;
            if (Charge <= 0f)
            {
                Charge = 0f;
                Deactivate();
            }
        }
        else if (Charge < 1f)
        {
            // Pasifken dol
            Charge += Time.deltaTime / fillTime;
            if (Charge >= 1f)
            {
                Charge = 1f;
                onReady?.Invoke();
            }
        }
        else if (WasKeyPressed())
        {
            Activate();
        }

        UpdateUI();
    }

    void Activate()
    {
        IsActive = true;
        if (leaf != null) leaf.speedMultiplier = speedMultiplier;
        if (musicSource != null)
        {
            normalPitch = musicSource.pitch;
            musicSource.pitch = normalPitch * musicSpeedMultiplier;
        }
        onActivated?.Invoke();
    }

    void Deactivate()
    {
        IsActive = false;
        if (leaf != null) leaf.speedMultiplier = 1f;
        if (musicSource != null) musicSource.pitch = normalPitch;
        onEnded?.Invoke();
    }

    // Fail olunca bar donar
    public void Stop()
    {
        stopped = true;
        if (IsActive) Deactivate();
    }

    void UpdateUI()
    {
        if (barFill != null) barFill.fillAmount = Charge;
        if (barSlider != null) barSlider.value = Charge;
        if (readyIndicator != null)
        {
            readyIndicator.SetActive(IsReady);

            if (IsReady && indicatorGroup != null)
            {
                // 0-1 arası gidip gelen değer, yanıp sönme
                float t = Mathf.PingPong(Time.time * blinkSpeed * 2f, 1f);
                indicatorGroup.alpha = Mathf.Lerp(blinkMinAlpha, blinkMaxAlpha, t);
            }
        }
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
            case KeyCode.Space: return Key.Space;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            default: return Key.None;
        }
    }
#endif
}