using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpsPooling : MonoBehaviour
{
    public static PowerUpsPooling Instance;

    [SerializeField] List<GameObject> powerUps = new List<GameObject>();

    [SerializeField] float initialTimer = 10f;
    float timer = 0f;

    void Start()
    {
        Instance = this;

        timer = initialTimer;

        for (int i = 0; i < powerUps.Count; i++)
        {
            GameObject ups = Instantiate(powerUps[0]);
            ups.SetActive(true);
        }
    }
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            powerUps[0].SetActive(true);
            ObjectPooling();
            timer = initialTimer;
        }
    }
    void ObjectPooling()
    {
        GameObject powerUp = powerUps[0];
        
        powerUp.transform.position = new Vector3(Random.Range(1f, GameManager.WIDTH), 2f, Random.Range(1f, GameManager.HEIGHT));
    }
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < powerUps.Count; i++)
        {
            if(!powerUps[i].activeInHierarchy)
            {
                return powerUps[i];
            }
        }
        return null;
    }
}
