﻿using UnityEngine;

public class AIController : GridMovement
{
    int chosenNumber = 0;
    [SerializeField] float timeUntilChangeDirection = 2f;
    float timer = 0f;

    Vector3 dirToMove;

    Player player;

    void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();

        timer = timeUntilChangeDirection;
    }

    public override void Update()
    {
        if (GameManager.isGameOver)
        {
            isMoving = false;
            return;
        }

        dir = AIMovementController();
        if (dir.HasValue)
            dirToMove = dir.Value;

        if (timer <= 0)
            timer = timeUntilChangeDirection;

        if (GameManager.GetFieldPosition(transform).IsWall)
            timer = 0f;

        if (!isMoving)
        {
            GetBeforeStep()?.Invoke(this, null);
            StartCoroutine(MoveObject(dirToMove));
        }
    }

    Vector3? AIMovementController()
    {
        int[] chooseRandomDir = { 0, 1, 2, 3 };
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (player.canFill)
            {
                chosenNumber = chooseRandomDir[Random.Range(0, chooseRandomDir.Length)];
                if (chosenNumber == 0)
                {
                    if (transform.position.x < RightBound)
                        if (!(BlockReverse && lastMove == Vector3.left))
                        {
                            chosenNumber = chooseRandomDir[Random.Range(1, chooseRandomDir.Length)];
                            return Vector3.right;
                        }
                }
                else if (chosenNumber == 1)
                {
                    if (transform.position.x > LeftBound)
                        if (!(BlockReverse && lastMove == Vector3.right))
                            return Vector3.left;
                }
                else if (chosenNumber == 2)
                {
                    if (transform.position.z < UpBound)
                        if (!(BlockReverse && lastMove == Vector3.back))
                            return Vector3.forward;
                }
                else
                {
                    if (transform.position.z > DownBound)
                        if (!(BlockReverse && lastMove == Vector3.forward))
                        {
                            chosenNumber = chooseRandomDir[Random.Range(0, 2)];
                            return Vector3.back;
                        }
                }
            }
            else
            {
                if (!GameManager.GetFieldPosition(transform).IsWall)
                {
                    if (transform.position.x < RightBound)
                        if (!(BlockReverse && lastMove == Vector3.left))
                            return Vector3.right;
                }
                else
                {
                    timer = timeUntilChangeDirection;
                    player.canFill = true;
                }
            }
        }
        return null;
    }
}