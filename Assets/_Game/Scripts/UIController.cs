using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class UIController : MonoBehaviour
{
    [SerializeField] Slider percentageSlider;

    Player player;
    void Start()
    {
        player = GetComponent<Player>();
        percentageSlider.maxValue = 100;

        percentageSlider.value = player.percent;
    }

    void Update()
    {
        percentageSlider.value = player.percent;
    }
}
