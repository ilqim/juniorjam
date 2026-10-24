using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Fail sahnesi: ekran siyahtan açılınca önce animasyonu oynatır,
// animasyon bitince fail panelini (skorlar + butonlar) açar.
public class FailSceneManager : MonoBehaviour
{
    [Header("Animasyon")]
    [Tooltip("Ayakla ezilme vb. animasyonu oynatan Animator")]
    public Animator animator;
    [Tooltip("Oynatılacak state'in adı. Boşsa Animator'un varsayılan state'i oynar")]
    public string animationState = "";
    [Tooltip("Animator yoksa ya da ek bekleme istersen: animasyondan sonra panel açılmadan önce bekleme (saniye)")]
    public float delayAfterAnimation = 0.3f;

    [Header("Sahne müziği")]
    [Tooltip("Sahne açılır açılmaz çalan müziğin AudioSource'u")]
    public AudioSource musicSource;
    public AudioClip sceneMusic;
    public bool loopSceneMusic = true;

    [Header("Animasyon Event sesi")]
    [Tooltip("Animasyondaki event'in çalacağı ses için AudioSource")]
    public AudioSource eventSource;
    [Tooltip("Animasyon event'i tetiklenince çalacak ses/müzik")]
    public AudioClip eventClip;
    [Tooltip("Event sesi çalarken sahne müziği dursun mu")]
    public bool stopSceneMusicOnEvent = false;

    [Header("Panel")]
    [Tooltip("Canvas içindeki fail ekranı kapsayıcısı. Başta kapalı olmalı")]
    public GameObject failPanel;
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreText;
    public Button playAgainButton;
    public Button mainMenuButton;
    public int scoreDigits = 6;

    [Header("Sahneler")]
    public string gameScene = "Game";
    public string mainMenuScene = "MainMenu";

    void Awake()
    {
        if (failPanel != null) failPanel.SetActive(false);

        // Fade bitene kadar animasyon başlamasın
        if (animator != null) animator.enabled = false;

        // Sahne açıldığı anda müzik başlasın (fade sürerken de)
        if (musicSource != null && sceneMusic != null)
        {
            musicSource.clip = sceneMusic;
            musicSource.loop = loopSceneMusic;
            musicSource.Play();
        }

        if (playAgainButton != null) playAgainButton.onClick.AddListener(() => SceneFader.Load(gameScene));
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => SceneFader.Load(mainMenuScene));
    }

    IEnumerator Start()
    {
        // Ekran siyahtan tamamen açılana kadar bekle
        while (SceneFader.IsFading) yield return null;

        if (animator != null)
        {
            animator.enabled = true;
            if (!string.IsNullOrEmpty(animationState))
                animator.Play(animationState, 0, 0f);

            yield return null; // Animator'un state'e geçmesi için bir kare bekle

            // Animasyon bitene kadar bekle
            while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        yield return new WaitForSeconds(delayAfterAnimation);

        ShowPanel();
    }

    // Animation Event'ten çağrılır. Animasyonda istediğin karede event ekleyip bu fonksiyonu seç.
    public void PlayEventSound()
    {
        if (stopSceneMusicOnEvent && musicSource != null)
            musicSource.Stop();

        if (eventSource != null && eventClip != null)
            eventSource.PlayOneShot(eventClip);
    }

    void ShowPanel()
    {
        string format = new string('0', Mathf.Max(1, scoreDigits));
        int score = GameManager.LastScore;
        int best = PlayerPrefs.GetInt("BestScore", 0);

        if (finalScoreText != null) finalScoreText.text = $"Score: {score.ToString(format)}";
        if (bestScoreText != null)
            bestScoreText.text = GameManager.LastWasNewBest
                ? $"New Best: {best.ToString(format)}"
                : $"Best: {best.ToString(format)}";

        if (failPanel != null) failPanel.SetActive(true);
    }
}