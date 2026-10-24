using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ImageCutscene : MonoBehaviour
{
    public Image displayImage;
    public Sprite[] frames;
    public UnityEvent onCutsceneEnd;

    private int currentIndex = 0;

    void OnEnable()
    {
        currentIndex = 0;
        if (frames != null && frames.Length > 0) 
        {
            displayImage.sprite = frames[0];
        }
    }

    public void NextFrame()
    {
        currentIndex++;
        if (currentIndex < frames.Length)
        {
            displayImage.sprite = frames[currentIndex];
        }
        else
        {
            gameObject.SetActive(false); // Hides the cutscene when finished
            onCutsceneEnd.Invoke();      // Triggers your next game event
        }
    }
}