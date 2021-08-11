using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenesManager : MonoBehaviour
{
    public static int currentLevel = 1;

    void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
            StartGame();
    }
    public void StartGame()
    {
        SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
    }
    public void LoadNextScene()
    {
        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        currentLevel++;
        if (currentLevel < 2)
            SceneManager.LoadScene(currentLevel + 1);
        else SceneManager.LoadScene(currentLevel);

        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
    public void ReloadScene()
    {
        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(currentLevel);
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
}
