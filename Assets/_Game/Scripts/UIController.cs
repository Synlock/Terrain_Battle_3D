using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;

    void Start()
    {
        levelText.text = "Level " + LoadScenesManager.currentLevel.ToString();
    }
}
