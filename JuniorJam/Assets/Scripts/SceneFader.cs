using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Sahneler arası siyah fade geçişi. Sahneler değişse de yok olmaz (DontDestroyOnLoad).
// Sahneye koymana gerek yok: ilk SceneFader.Load çağrısında kendini oluşturur.
// Kullanım: SceneFader.Load("SahneAdi");
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    // Fade sürerken true. Cutscene gibi scriptler bu sırada tıklamayı yok sayabilir.
    public static bool IsFading => Instance != null && Instance.isFading;

    [Tooltip("Kararma süresi (saniye)")]
    public float fadeOutDuration = 0.6f;
    [Tooltip("Açılma süresi (saniye)")]
    public float fadeInDuration = 0.6f;
    [Tooltip("Siyah ekranda, yeni sahne açılmadan önce bekleme (saniye)")]
    public float holdBlack = 0.15f;

    CanvasGroup group;
    bool isFading;

    public static void Load(string sceneName)
    {
        if (Instance == null)
            new GameObject("SceneFader").AddComponent<SceneFader>();

        Instance.FadeToScene(sceneName);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    // Her şeyin üstünde duran tam ekran siyah bir görsel oluşturur.
    void BuildOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        GameObject imageGO = new GameObject("Black");
        imageGO.transform.SetParent(transform, false);
        Image image = imageGO.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void FadeToScene(string sceneName)
    {
        if (isFading) return; // üst üste basılırsa ikinci kez yükleme
        StartCoroutine(FadeRoutine(sceneName));
    }

    IEnumerator FadeRoutine(string sceneName)
    {
        isFading = true;
        group.blocksRaycasts = true; // geçiş sırasında butonlara tıklanmasın

        yield return Fade(0f, 1f, fadeOutDuration);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // Sahneye Time.timeScale = 0 ile girilse bile çalışsın diye gerçek zaman kullanılıyor
        yield return new WaitForSecondsRealtime(holdBlack);

        yield return Fade(1f, 0f, fadeInDuration);

        group.blocksRaycasts = false;
        isFading = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}