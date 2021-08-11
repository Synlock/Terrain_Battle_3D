using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenesManager : MonoBehaviour
{
    public static int currentLevel = 1;

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && !SceneManager.GetSceneByBuildIndex(currentLevel).isLoaded)
        {
            if(PlayerPrefs.GetInt("Current Level") == 0)
            {
                currentLevel = 1;
            }
            else currentLevel = PlayerPrefs.GetInt("Current Level");

            StartCoroutine(StartGame());
        }
    }
    IEnumerator StartGame()
    {
        SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
        yield return new WaitForSeconds(0.01f);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentLevel));
    }
    public void LoadNextScene()
    {
        GameManager.hasGameStarted = false;
        Time.timeScale = 1f;

        currentLevel++;
        if (currentLevel < 2)
            SceneManager.LoadScene(currentLevel + 1);
        else SceneManager.LoadScene(currentLevel);

        PlayerPrefs.SetInt("Current Level", currentLevel);

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