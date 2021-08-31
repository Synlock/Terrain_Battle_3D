using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingRotator : MonoBehaviour
{
    public float startTime = 1f;
    [SerializeField] float rotationSpeed = 50f;

    float timer = 0f;

    void Start()
    {
        timer = startTime;
    }

    void Update()
    {
        if (!GameManager.isGameOver) return;

        timer -= Time.deltaTime;
        if(timer <= 0)
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}