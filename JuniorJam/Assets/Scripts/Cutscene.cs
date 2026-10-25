using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Çizgi roman sayfası gibi cutscene: her karenin önünde siyah bir kapak var.
// Sahne açılınca 1. kapak yavaşça kaybolur (1. kare görünür) ve 1. cümle yazılır.
// Basış: yazı yazılıyorsa önce yazıyı tamamlar.
// Yazı bittiyse sıradaki kapak kaybolur, sıradaki cümle yazılır.
// Son kareden sonra basınca sonraki sahneye geçer.
public class Cutscene : MonoBehaviour
{
    // Tüm cutscene'lerin yazı hızı. Değiştirmek için sadece bu sayıyı düzenle.
    private const float SecondsPerLetter = 0.05f;

    [Tooltip("Karelerin önündeki siyah kapaklar, açılma sırasına göre")]
    [SerializeField] private Image[] covers;
    [SerializeField] private TMP_Text textBox;
    [Tooltip("Her kare için bir cümle, aynı sırayla")]
    [SerializeField, TextArea] private string[] sentences;
    [SerializeField] private string nextSceneName;

    [Tooltip("Bir kapağın kaybolma süresi (saniye)")]
    [SerializeField] private float revealDuration = 1f;

    private int index = -1;
    private bool isTyping;
    private bool isRevealing;
    private bool isLeaving;
    private bool started;
    private Coroutine typingRoutine;

    private IEnumerator Start()
    {
        // Bütün kareler kapalı başlasın
        foreach (Image cover in covers)
            SetAlpha(cover, 1f);

        textBox.text = "";

        // Sahne geçişindeki siyah ekran tamamen açılana kadar bekle
        while (SceneFader.IsFading) yield return null;

        started = true;
        RevealNext();
    }

    private void Update()
    {
        if (!started || isLeaving || SceneFader.IsFading) return;
        if (!AdvancePressed()) return;

        if (isTyping)
        {
            FinishTyping();          // yazılırken tık: cümleyi tamamla
        }
        else if (!isRevealing)
        {
            if (index + 1 < covers.Length)
            {
                RevealNext();        // sıradaki kare
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                isLeaving = true;
                SceneFader.Load(nextSceneName);
            }
        }
    }

    private void RevealNext()
    {
        index++;
        StartCoroutine(Reveal(covers[index]));

        string line = index < sentences.Length ? sentences[index] : "";
        typingRoutine = StartCoroutine(TypeText(line));
    }

    private IEnumerator Reveal(Image cover)
    {
        isRevealing = true;

        float t = 0f;
        while (t < revealDuration)
        {
            t += Time.deltaTime;
            SetAlpha(cover, Mathf.Lerp(1f, 0f, t / revealDuration));
            yield return null;
        }
        SetAlpha(cover, 0f);

        isRevealing = false;
    }

    private static void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private IEnumerator TypeText(string sentence)
    {
        isTyping = true;
        textBox.text = sentence;
        textBox.maxVisibleCharacters = 0;
        textBox.ForceMeshUpdate();
        int total = textBox.textInfo.characterCount;

        for (int i = 1; i <= total; i++)
        {
            textBox.maxVisibleCharacters = i;
            yield return new WaitForSeconds(SecondsPerLetter);
        }
        isTyping = false;
    }

    private void FinishTyping()
    {
        StopCoroutine(typingRoutine);
        textBox.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    private bool AdvancePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame ||
                 Keyboard.current.enterKey.wasPressedThisFrame))
            || (Mouse.current != null &&
                (Mouse.current.leftButton.wasPressedThisFrame ||
                 Mouse.current.rightButton.wasPressedThisFrame));
#else
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetMouseButtonDown(0)
            || Input.GetMouseButtonDown(1);
#endif
    }
}