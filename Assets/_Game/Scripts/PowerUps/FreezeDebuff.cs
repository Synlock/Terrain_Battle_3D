using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeDebuff : MonoBehaviour
{
    Player[] players;

    [SerializeField] ParticleSystem speedVFX;

    float initialSpeed;

    [SerializeField] float initialTimer = 2f;
    float timer = 0f;

    bool freezeStarted = false;

    float timeUntilDestroy = 0f;
    GridMovement gridMovement;

    void Start()
    {
        players =  new Player[]{ GameObject.Find("Player").GetComponent<Player>(), GameObject.Find("Enemy").GetComponent<Player>() };
        
        timer = initialTimer;
        timeUntilDestroy = initialTimer + 2f;
    }
    void Update()
    {
        if (freezeStarted)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                gridMovement.timeToMove = initialSpeed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //bug that if hit while sped up will permenantly be sped up
        //temporary solution for only 2 players - TODO: need to add a feature that automatically detects all players in map and make the player in element 0
        if (players[0])
        {
            FreezeController(players[1]);
        }
        else
        {
            FreezeController(players[0]);
        }
    }
    void FreezeController(Player player)
    {
        GridMovement movement = player.gridMovement;
        gridMovement = movement;

        if (speedVFX != null)
            speedVFX.Play();

        freezeStarted = true;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;

        initialSpeed = gridMovement.timeToMove;
        player.gridMovement.timeToMove = 1f;
        Destroy(gameObject, timeUntilDestroy);
    }
}
