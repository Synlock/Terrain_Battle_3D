using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class UIController : MonoBehaviour
{
    [SerializeField] Slider percentageSlider;
    [SerializeField] Image fill;

    [SerializeField] GameObject winPanel;

    Player player;
    void Start()
    {
        player = GetComponent<Player>();
        percentageSlider.maxValue = 100;

        fill.color = GetComponent<MeshRenderer>().material.color;

        percentageSlider.value = player.percent;
    }

    void Update()
    {
        percentageSlider.value = player.percent;

        WinHandler();
    }

    void WinHandler()
    {
        if(player.percent >= 80f)
        {
            if(winPanel != null)
                winPanel.SetActive(true);
        }
    }
}
