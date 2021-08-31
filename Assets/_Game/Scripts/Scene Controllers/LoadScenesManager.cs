using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenesManager : MonoBehaviour
{
    public static int currentLevel = 1;
    int maxLevel;

    void Start()
    {
        maxLevel = SceneManager.sceneCountInBuildSettings - 1;
        int checkIndex;
        if (currentLevel <= maxLevel)
            checkIndex = currentLevel;
        else checkIndex = maxLevel;

        if (SceneManager.GetActiveScene().buildIndex == 0 && !SceneManager.GetSceneByBuildIndex(checkIndex).isLoaded)
        {
            if (PlayerPrefs.GetInt("Current Level") == 0)
            {
                currentLevel = 1;
            }
            else currentLevel = PlayerPrefs.GetInt("Current Level");
            
            StartGame();
        }
    }
    private void StartGame()
    {
        if (currentLevel <= maxLevel)
        {
            SceneManager.LoadSceneAsync(currentLevel);
            SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
        }
        else
        {
            int randomLevel = Random.Range(1, maxLevel + 1);
            SceneManager.LoadSceneAsync(randomLevel);
            SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
        }
    }
    public void LoadNextScene()
    {
        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        currentLevel++;
        if (currentLevel < maxLevel)
        {
            SceneManager.LoadScene(currentLevel);
        }
        else SceneManager.LoadScene(Random.Range(1, maxLevel + 1));

        PlayerPrefs.SetInt("Current Level", currentLevel);

        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
    public void ReloadScene()
    {
        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
    public void UnloadScene()
    {
        GameManager.hasGameStarted = true;
        SceneManager.UnloadSceneAsync("Main Menu");
    }
}