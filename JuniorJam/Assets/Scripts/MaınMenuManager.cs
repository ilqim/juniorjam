using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Play'e basınca açılacak sahnenin adı (ilk cutscene). Build Profiles listesindekiyle birebir aynı olmalı")]
    public string nextSceneName = "Cutscene";

    [Tooltip("Butona basıldıktan sonra fade başlamadan önceki bekleme (buton sesi için)")]
    public float loadDelay = 0.25f;

    public void PlayGame()
    {
        Invoke(nameof(LoadNextScene), loadDelay);
    }

    void LoadNextScene()
    {
        SceneFader.Load(nextSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkıldı!");
        Application.Quit();
    }
}