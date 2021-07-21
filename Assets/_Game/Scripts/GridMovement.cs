using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    [Tooltip("MUST CHECK TRUE ON PLAYER OBJECT")] [SerializeField] bool isPlayer = false;
    [SerializeField] float timeToMove = 0.2f;
    [SerializeField] float distance = 0.5f;

    public int moves = 0;
    public int verticalCounter = 0;
    public int horizontalCounter = 0;

    Vector3 originalPos;
    Vector3 targetPos;

    public bool goingForward, goingBack, goingLeft, goingRight = false;
    bool isMoving = false;

    FloatingJoystick joystick;

    public event EventHandler OnStep;

    [Header("AI Params")]
    [SerializeField] float initialTimer = 2f;
    float timer = 0f;
    int chosenNumber = 0;
    #region Getter/Setter
    public bool GetIsMoving()
    {
        return isMoving;
    }
    public bool GetIsPlayer()
    {
        return isPlayer;
    }
    #endregion

    #region Start/Update
    void Start()
    {
        joystick = FindObjectOfType<FloatingJoystick>();
    }
    void Update()
    {
        if (isPlayer)
        {
            PlayerMovement();
        }
        else
        {
            Player2Movement();
            //AIMovementController();
        }
    }
    #endregion

    #region OnTriggerEvents
    void OnTriggerStay(Collider other)
    {
        if (isPlayer)
        {
            if (other.gameObject.CompareTag("PlayerLand") || other.gameObject.CompareTag("Wall"))
            {
                goingRight = false;
                goingForward = false;
                goingBack = false;
                goingLeft = false;
            }
        }
        else
        {
            if(other.gameObject.CompareTag("EnemyLand") || other.gameObject.CompareTag("Wall"))
            {
                goingRight = false;
                goingForward = false;
                goingBack = false;
                goingLeft = false;
            }
        } 
            
    }
    #endregion

    #region Movement Handlers
    IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;
        moves++;

        float elapsedTime = 0f;

        originalPos = transform.position;
        targetPos = originalPos + direction * distance;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(originalPos, targetPos, elapsedTime / timeToMove);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }

    void PlayerMovement()
    {
        Vector3? dir = null;

        if (joystick.Horizontal > 0.3 && !isMoving && !goingLeft)
        {
            goingRight = true;

            goingForward = false;
            goingBack = false;
            goingLeft = false;

            dir = Vector3.right;
        }

        if (joystick.Horizontal < -0.3 && !isMoving && !goingRight)
        {
            goingLeft = true;

            goingForward = false;
            goingBack = false;
            goingRight = false;

            dir = Vector3.left;
        }

        if (joystick.Vertical > 0.3 && !isMoving && !goingBack)
        {
            goingForward = true;

            goingBack = false;
            goingLeft = false;
            goingRight = false;

            dir = Vector3.forward;
        }

        if (joystick.Vertical < -0.3 && !isMoving && !goingForward)
        {
            goingBack = true;

            goingForward = false;
            goingLeft = false;
            goingRight = false;

            dir = Vector3.back;
        }
        if(dir.HasValue)
        {
            OnStep?.Invoke(this, new StepEventArgs() { Direction = dir.Value });
            StartCoroutine(MovePlayer(dir.Value));
        }
    }
    public class StepEventArgs : EventArgs
    {
        public Vector3 Direction;
    }
    void Player2Movement()
    {
        if (Input.GetKey(KeyCode.D) && !isMoving && !goingLeft)
        {
            goingRight = true;

            goingForward = false;
            goingBack = false;
            goingLeft = false;

            StartCoroutine(MovePlayer(Vector3.right));
        }

        if (Input.GetKey(KeyCode.A) && !isMoving && !goingRight)
        {
            goingLeft = true;

            goingForward = false;
            goingBack = false;
            goingRight = false;

            StartCoroutine(MovePlayer(Vector3.left));
        }

        if (Input.GetKey(KeyCode.W) && !isMoving && !goingBack)
        {
            goingForward = true;

            goingBack = false;
            goingLeft = false;
            goingRight = false;

            StartCoroutine(MovePlayer(Vector3.forward));
        }

        if (Input.GetKey(KeyCode.S) && !isMoving && !goingForward)
        {
            goingBack = true;

            goingForward = false;
            goingLeft = false;
            goingRight = false;

            StartCoroutine(MovePlayer(Vector3.back));
        }
    }

    void AIMovementController()
    {
        int[] chooseRandomDir = { 0, 1, 2, 3 };
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            chosenNumber = chooseRandomDir[UnityEngine.Random.Range(0, chooseRandomDir.Length)];
            if (!goingRight && chosenNumber == 0)
            {
                chosenNumber = chooseRandomDir[UnityEngine.Random.Range(1, chooseRandomDir.Length)];
            }
            else if (!goingLeft && chosenNumber == 1)
            {
                chosenNumber++;
            }
            else if (!goingForward && chosenNumber == 2)
            {
                chosenNumber++;
            }
            else if (!goingBack && chosenNumber == 3)
            {
                chosenNumber = chooseRandomDir[UnityEngine.Random.Range(0, 2)];
            }
            timer = initialTimer;
        }

        if (!isMoving && !goingLeft && chosenNumber == 0)
        {
            goingRight = true;

            goingForward = false;
            goingBack = false;
            goingLeft = false;

            StartCoroutine(MovePlayer(Vector3.right));
        }

        else if (!isMoving && !goingRight && chosenNumber == 1)
        {
            goingLeft = true;

            goingForward = false;
            goingBack = false;
            goingRight = false;

            StartCoroutine(MovePlayer(Vector3.left));
        }

        else if (!isMoving && !goingBack && chosenNumber == 2)
        {
            goingForward = true;

            goingBack = false;
            goingLeft = false;
            goingRight = false;

            StartCoroutine(MovePlayer(Vector3.forward));
        }

        else if (!isMoving && !goingForward && chosenNumber == 3)
        {
            goingBack = true;

            goingForward = false;
            goingLeft = false;
            goingRight = false;

            StartCoroutine(MovePlayer(Vector3.back));
        }
    }
    #endregion
}