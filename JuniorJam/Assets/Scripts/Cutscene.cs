using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Cutscene : MonoBehaviour
{
    // Tüm cutscene'lerin yazı hızı. Değiştirmek için sadece bu sayıyı düzenle.
    private const float SecondsPerLetter = 0.05f;

    [SerializeField] private Image display;
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private Sprite[] slides;
    [SerializeField, TextArea] private string[] sentences;
    [SerializeField] private string nextSceneName;

    private int index;
    private bool isTyping;
    private bool isLeaving;
    private Coroutine typingRoutine;

    private void Start()
    {
        ShowSlide(0);
    }

    private void Update()
    {
        // Fade sırasında ya da sahneden çıkarken tıklamaları yok say
        if (isLeaving || SceneFader.IsFading) return;
        if (!AdvancePressed()) return;

        if (isTyping)
            FinishTyping();   // yazılırken tık: cümleyi tamamla
        else
            Next();           // cümle bittiyse tık: sonraki slide
    }

    private void ShowSlide(int i)
    {
        index = i;
        display.sprite = slides[i];
        string line = i < sentences.Length ? sentences[i] : "";
        typingRoutine = StartCoroutine(TypeText(line));
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

    private void Next()
    {
        if (index + 1 < slides.Length)
        {
            ShowSlide(index + 1);
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            isLeaving = true;
            SceneFader.Load(nextSceneName);
        }
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