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

            StartCoroutine(StartGame());
        }
    }
    IEnumerator StartGame()
    {
        if (currentLevel <= maxLevel)
        {
            SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
            yield return new WaitForSeconds(0.01f);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentLevel));
        }
        else
        {
            SceneManager.LoadScene(maxLevel, LoadSceneMode.Additive);
            yield return new WaitForSeconds(0.01f);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(maxLevel));
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
        else SceneManager.LoadScene(maxLevel);

        PlayerPrefs.SetInt("Current Level", currentLevel);

        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
    public void ReloadScene()
    {
        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        if (currentLevel < maxLevel)
        {
            SceneManager.LoadScene(currentLevel);
        }
        else SceneManager.LoadScene(maxLevel);

        SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
    }
}