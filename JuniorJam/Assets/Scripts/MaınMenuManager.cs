using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Invoke("LoadGameScene", 0.25f);
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }
    

    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkıldı!");
        Application.Quit();
    }
}