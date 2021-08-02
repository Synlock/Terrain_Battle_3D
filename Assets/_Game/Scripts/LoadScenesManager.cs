using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenesManager : MonoBehaviour
{
    public static int currentLevel = 1;

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        Time.timeScale = 1f;

        currentLevel++;
        SceneManager.LoadScene(currentSceneIndex);
    }
    public void ReloadScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        Time.timeScale = 1f;

        SceneManager.LoadScene(currentSceneIndex);
    }
}
