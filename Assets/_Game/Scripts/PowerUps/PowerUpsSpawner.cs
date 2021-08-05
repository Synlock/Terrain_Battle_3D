using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpsSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] powerUpsToSpawn;

    [SerializeField] float initialTimer = 10f;
    float timer = 0f;

    public static bool isSpawned = false;

    void Start()
    {
        timer = initialTimer;
    }
    void Update()
    {
        if (isSpawned) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnPowerUp();
            timer = initialTimer;
        }
    }
    void SpawnPowerUp()
    {
        if (powerUpsToSpawn.Length > 1)
        {
            GameObject newPowerUp = Instantiate(powerUpsToSpawn[Random.Range(0, powerUpsToSpawn.Length)]);
            newPowerUp.transform.position = new Vector3(Random.Range(5f, GameManager.WIDTH), 2f, Random.Range(5f, GameManager.HEIGHT));
        }
        else
        {
            GameObject newPowerUp = Instantiate(powerUpsToSpawn[0]);
            newPowerUp.transform.position = new Vector3(Random.Range(5f, GameManager.WIDTH), 2f, Random.Range(5f, GameManager.HEIGHT));
        }
        isSpawned = true;
    }
}
