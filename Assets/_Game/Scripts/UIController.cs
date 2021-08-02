using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class UIController : MonoBehaviour
{
    Player[] players;

    [SerializeField] Slider percentageSlider;
    [SerializeField] Image fill;

    [SerializeField] GameObject winPanel;

    Player player;
    void Start()
    {
        players = new Player[] { GameObject.Find("Player").GetComponent<Player>(), GameObject.Find("Enemy").GetComponent<Player>() };

        player = GetComponent<Player>();
        percentageSlider.maxValue = 100;

        fill.color = GetComponent<MeshRenderer>().material.color;

        percentageSlider.value = player.percent;
    }

    void Update()
    {
        percentageSlider.value = player.percent;

        WinHandler();
        LeaderImageOnTop();
    }

    void WinHandler()
    {
        if(player.percent >= 80f)
        {
            GameManager.isGameOver = true;

            if(winPanel != null)
                winPanel.SetActive(true);
        }
    }
    void LeaderImageOnTop()
    {
        if(players[0].percent > players[1].percent)
            players[0].handle.transform.SetAsFirstSibling();
        else players[1].handle.transform.SetAsFirstSibling();
    }
}
