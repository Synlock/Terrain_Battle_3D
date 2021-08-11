using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TapToUnload : MonoBehaviour
{
    public void UnloadScene()
    {
        GameManager.hasGameStarted = true;
        SceneManager.UnloadSceneAsync("Main Menu");
    }
}
