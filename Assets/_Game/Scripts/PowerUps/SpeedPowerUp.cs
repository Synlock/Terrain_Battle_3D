using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    ParticleSystem speedVFX;
    
    float initialSpeed; 
    [SerializeField] float speedMultiplier = 2f;

    [SerializeField] float initialTimer = 2f;
    float timer = 0f;

    bool speedStarted = false;

    float timeUntilDestroy = 0f;
    GridMovement gridMovement;

    void Start()
    {
        speedVFX = GetComponentInChildren<ParticleSystem>();

        timer = initialTimer;
        timeUntilDestroy = initialTimer + 2f;
    }
    void Update()
    {
        if(speedStarted)
        {
            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                gridMovement.timeToMove = initialSpeed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GridMovement movement = other.GetComponent<GridMovement>();
        gridMovement = movement;

        if (movement)
        {
            if (speedVFX != null)
                speedVFX.transform.parent = other.transform;

            speedStarted = true;

            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;

            initialSpeed = movement.timeToMove;
            movement.timeToMove /= speedMultiplier;
            Destroy(gameObject, timeUntilDestroy);
        }
    }
}
