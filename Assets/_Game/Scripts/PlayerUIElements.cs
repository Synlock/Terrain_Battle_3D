﻿using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class PlayerUIElements : MonoBehaviour
{
    [SerializeField] Slider percentageSlider;
    [SerializeField] Image fill;

    [SerializeField] GameObject winPanel;

    Player player;
    void Start()
    {
        player = GetComponent<Player>();
        percentageSlider.maxValue = 100;

        fill.color = player.myColor;

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
        if (player.percent >= GameManager.percentToWin)
        {
            GameManager.isGameOver = true;

            if (winPanel != null)
                winPanel.SetActive(true);
        }
    }
    void LeaderImageOnTop()
    {
        if (GameManager.players[0].percent > GameManager.players[1].percent)
        {
            GameManager.players[0].slider.transform.SetAsLastSibling();
        }
        else GameManager.players[1].slider.transform.SetAsLastSibling();
    }
}