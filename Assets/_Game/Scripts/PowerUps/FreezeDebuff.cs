using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeDebuff : PowerUpsManager 
{ 
    [SerializeField] ParticleSystem freezeVFX;

    float initialSpeed;

    [SerializeField] float initialTimer = 2f;
    float timer = 0f;

    bool freezeStarted = false;

    float timeUntilDestroy = 0f;
    GridMovement gridMovement;

    void Start()
    {
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
                PowerUpsSpawner.isSpawned = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //bug that if hit while sped up will permenantly be sped up
        //temporary solution for only 2 players - TODO: need to add a feature that automatically detects all players in map and make the player in element 0
        if (other.gameObject == GameManager.players[0].gameObject)
        {
            FreezeController(GameManager.players[1]);
        }
        else
        {
            FreezeController(GameManager.players[0]);
        }
    }
    void FreezeController(Player player)
    {
        GridMovement movement = player.gridMovement;
        gridMovement = movement;

        if (freezeVFX != null)
        {
            freezeVFX.Play();
            freezeVFX.gameObject.transform.parent = player.transform;
            freezeVFX.gameObject.transform.localPosition = Vector3.zero;
        }

        freezeStarted = true;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;

        initialSpeed = gridMovement.timeToMove;
        player.gridMovement.timeToMove = 1f;
        Destroy(gameObject, timeUntilDestroy);
    }
}
