using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Skoru tutar (süreyle artar), yaprak çarpınca fail ekranını açar.
// Play Again ve Main Menu butonları için fonksiyonlar da burada.
public class GameManager : MonoBehaviour
{
    [Header("Referanslar")]
    public Leaf leaf;
    [Tooltip("Fail olunca dalları durdurmak için (opsiyonel)")]
    public BranchSpawner branchSpawner;
    [Tooltip("Fail olunca durdurulacak arka plan Scroller'ları (opsiyonel)")]
    public Scroller[] backgroundScrollers;

    [Header("UI")]
    [Tooltip("Oyun sırasında görünen skor")]
    public TMP_Text scoreText;
    [Tooltip("Canvas içindeki fail ekranı kapsayıcısı (başta kapalı olmalı)")]
    public GameObject failScreen;
    [Tooltip("Fail ekranında son skor (opsiyonel)")]
    public TMP_Text finalScoreText;
    [Tooltip("Fail ekranında en iyi skor (opsiyonel)")]
    public TMP_Text bestScoreText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Skor")]
    [Tooltip("Başlangıçta saniyede kazanılan puan")]
    public float pointsPerSecond = 10f;
    [Tooltip("Her saniye puan kazanma hızına eklenen miktar (0 = sabit hız)")]
    public float pointsAcceleration = 0.5f;

    [Header("Sahneler")]
    public string mainMenuScene = "MainMenu";
    [Tooltip("Play Again'de açılacak sahne (oyun sahnesi)")]
    public string gameScene = "Game";
    [Tooltip("Fail olunca açılacak sahne. Boş bırakılırsa bu sahnedeki Fail Screen açılır")]
    public string failScene = "Fail";
    [Tooltip("Çarptıktan sonra kararmaya başlamadan önce bekleme (saniye)")]
    public float failDelay = 0.5f;

    // Fail sahnesinde göstermek için son oyunun skoru
    public static int LastScore { get; private set; }
    public static bool LastWasNewBest { get; private set; }

    const string BestScoreKey = "BestScore";

    [Tooltip("Skorun kaç basamaklı gösterileceği (önü sıfırla doldurulur)")]
    public int scoreDigits = 6;
    string ScoreFormat => new string('0', Mathf.Max(1, scoreDigits));

    float score;
    float currentRate;
    bool isGameOver;

    public int Score => Mathf.FloorToInt(score);
    public int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    void Awake()
    {
        if (leaf == null) leaf = FindFirstObjectByType<Leaf>();
        if (branchSpawner == null) branchSpawner = FindFirstObjectByType<BranchSpawner>();

        // Yaprak çarpınca otomatik GameOver çağrılsın, Inspector'dan bağlamaya gerek yok
        if (leaf != null) leaf.onFail.AddListener(GameOver);

        if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgain);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (failScreen != null) failScreen.SetActive(false);

        currentRate = pointsPerSecond;
        UpdateScoreText();
    }

    void Update()
    {
        if (isGameOver) return;

        currentRate += pointsAcceleration * Time.deltaTime;
        score += currentRate * Time.deltaTime;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = Score.ToString(ScoreFormat);
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // En iyi skoru kaydet
        bool newBest = Score > BestScore;
        if (newBest)
        {
            PlayerPrefs.SetInt(BestScoreKey, Score);
            PlayerPrefs.Save();
        }

        // Her şeyi durdur
        if (branchSpawner != null) branchSpawner.StopAll();
        if (backgroundScrollers != null)
            foreach (Scroller s in backgroundScrollers)
                if (s != null) s.speed = 0f;

        LastScore = Score;
        LastWasNewBest = newBest;

        // Fail sahnesi varsa siyah geçişle oraya git
        if (!string.IsNullOrEmpty(failScene))
        {
            Invoke(nameof(LoadFailScene), failDelay);
            return;
        }

        // Yoksa bu sahnedeki fail ekranı
        if (finalScoreText != null) finalScoreText.text = $"Score: {Score.ToString(ScoreFormat)}";
        if (bestScoreText != null) bestScoreText.text = newBest
            ? $"New Best: {Score.ToString(ScoreFormat)}"
            : $"Best: {BestScore.ToString(ScoreFormat)}";
        if (failScreen != null) failScreen.SetActive(true);
    }

    void LoadFailScene()
    {
        SceneFader.Load(failScene);
    }

    // Play Again butonuna bağla
    public void PlayAgain()
    {
        SceneFader.Load(string.IsNullOrEmpty(gameScene) ? SceneManager.GetActiveScene().name : gameScene);
    }

    // Main Menu butonuna bağla
    public void GoToMainMenu()
    {
        SceneFader.Load(mainMenuScene);
    }
}