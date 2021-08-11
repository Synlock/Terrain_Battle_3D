using UnityEngine;

public class AIController : GridMovement
{
    int chosenNumber = 0;
    [SerializeField] float timeUntilChangeDirection = 2f;
    float timer = 0f;

    Vector3 dirToMove = Vector3.back;

    Player player;

    void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();

        timer = timeUntilChangeDirection;
    }

    public override void Update()
    {
        if (GameManager.isGameOver || !GameManager.hasGameStarted)
        {
            isMoving = false;
            return;
        }

        HandleDirectionAndMovement();
    }

    void HandleDirectionAndMovement()
    {
        dir = AIMovementController();
        if (dir.HasValue)
            dirToMove = dir.Value;

        if (timer <= 0)
            timer = timeUntilChangeDirection;

        PreventLeavingGrid();

        if (!isMoving)
        {
            GetBeforeStep()?.Invoke(this, null);
            StartCoroutine(MoveObject(dirToMove));
        }
    }

    void PreventLeavingGrid()
    {
        if (dirToMove == lastMove)
            BlockReverse = true;

        if (GameManager.GetFieldPosition(transform).tilePos.x == RightBound)
        {
            transform.eulerAngles = new Vector3(90f, 270f, 0f);
            animator.SetBool("isMoving", true);
            dirToMove = Vector3.left;
        }
        else if (GameManager.GetFieldPosition(transform).tilePos.x == 0)
        {
            transform.eulerAngles = new Vector3(90f, 90f, 0f);
            animator.SetBool("isMoving", true);
            dirToMove = Vector3.right;
        }
        else if (GameManager.GetFieldPosition(transform).tilePos.z == UpBound)
        {
            transform.eulerAngles = new Vector3(90f, 180f, 0f);
            animator.SetBool("isMoving", true);
            dirToMove = Vector3.back;
        }
        else if (GameManager.GetFieldPosition(transform).tilePos.z == 0)
        {
            transform.eulerAngles = new Vector3(90f, 0f, 0f);
            animator.SetBool("isMoving", true);
            dirToMove = Vector3.forward;
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
                            transform.eulerAngles = new Vector3(90f, 90f, 0f);
                            animator.SetBool("isMoving", true);
                            return Vector3.right;
                        }
                    else animator.SetBool("isMoving", false);
                }
                else if (chosenNumber == 1)
                {
                    if (transform.position.x > LeftBound)
                        if (!(BlockReverse && lastMove == Vector3.right))
                        {
                            animator.SetBool("isMoving", true);
                            transform.eulerAngles = new Vector3(90f, 270f, 0f);
                            return Vector3.left;
                        }
                    else animator.SetBool("isMoving", false);
                }
                else if (chosenNumber == 2)
                {
                    if (transform.position.z < UpBound)
                        if (!(BlockReverse && lastMove == Vector3.back))
                        {
                            animator.SetBool("isMoving", true);
                            transform.eulerAngles = new Vector3(90f, 0f, 0f);
                            return Vector3.forward;
                        }
                    else animator.SetBool("isMoving", false);
                }
                else
                {
                    if (transform.position.z > DownBound)
                        if (!(BlockReverse && lastMove == Vector3.forward))
                        {
                            chosenNumber = chooseRandomDir[Random.Range(0, 2)];
                            animator.SetBool("isMoving", true);
                            transform.eulerAngles = new Vector3(90f, 180f, 0f);
                            return Vector3.back;
                        }
                    else animator.SetBool("isMoving", false);
                }
            }
            else
            {
                if (!GameManager.GetFieldPosition(transform).IsWall)
                {
                    if (transform.position.x < RightBound)
                        if (!(BlockReverse && lastMove == Vector3.left))
                        {
                            animator.SetBool("isMoving", true);
                            return Vector3.right;
                        }
                        else animator.SetBool("isMoving", false);
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