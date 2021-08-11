using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenesManager : MonoBehaviour
{
    public static int currentLevel = 1;

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        currentLevel++;
        if (currentSceneIndex < 2)
            SceneManager.LoadScene(currentSceneIndex + 1);
        else SceneManager.LoadScene(currentSceneIndex);

        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
    public void ReloadScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(currentSceneIndex);
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
}
