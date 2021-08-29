using System;
using System.Collections;
using UnityEngine;


public class GridMovement : MonoBehaviour
{
    public enum InputMethod
    {
        Arrows,
        WASD,
        Joystick,
        Custom
    }

    public float timeToMove = 0.2f;
    public float distance = 1f;

    Vector3 originalPos;
    Vector3 targetPos;
    protected Vector3 lastMove = Vector3.zero;

    protected Vector3? dir;

    public int moves;

    public bool BlockReverse = false;

    [SerializeField] InputMethod _inputMethod;
    [SerializeField]
    public InputMethod inputMethod
    {
        get => _inputMethod;
        set
        {
            if (value == _inputMethod) return;
            _inputMethod = value;
            if (_inputMethod == InputMethod.Arrows)
            {
                Up = KeyCode.UpArrow;
                Down = KeyCode.DownArrow;
                Left = KeyCode.LeftArrow;
                Right = KeyCode.RightArrow;
            }
            else if (_inputMethod == InputMethod.WASD)
            {
                Up = KeyCode.W;
                Down = KeyCode.S;
                Left = KeyCode.A;
                Right = KeyCode.D;
            }
        }
    }
    FloatingJoystick joystick;
    [SerializeField] public KeyCode Up;
    [SerializeField] public KeyCode Down;
    [SerializeField] public KeyCode Left;
    [SerializeField] public KeyCode Right;

    [SerializeField] public float UpBound = float.PositiveInfinity;
    [SerializeField] public float DownBound = float.NegativeInfinity;
    [SerializeField] public float LeftBound = float.NegativeInfinity;
    [SerializeField] public float RightBound = float.PositiveInfinity;


    protected bool isMoving = false;

    public event EventHandler BeforeStep;
    public event EventHandler AfterStep;

    [HideInInspector] public Animator animator;

    #region Getters/Setters
    public EventHandler GetBeforeStep()
    {
        return BeforeStep;
    }
    #endregion

    #region Start/Update
    void Start()
    {
        joystick = FindObjectOfType<FloatingJoystick>();
        animator = GetComponent<Animator>();
    }

    public virtual void Update()
    {
        if (GameManager.isGameOver) //|| !GameManager.hasGameStarted)
        {
            joystick.gameObject.SetActive(false);
            return;
        }

        dir = GetDirection();
        if (dir.HasValue)
        {
            BeforeStep?.Invoke(this, null);
            StartCoroutine(MoveObject(dir.Value));
        }
    }
    #endregion

    #region Movement Handlers
    Vector3? GetDirection()
    {
        if (isMoving)
            return null;

        if (inputMethod == InputMethod.Joystick)
        {
            if (transform.position.x < RightBound)
                if (!(BlockReverse && lastMove == Vector3.left))
                    if (joystick.Horizontal > 0.3)
                    {
                        animator.SetBool("isMoving", true);
                        transform.eulerAngles = new Vector3(90f, 90f, 0f);
                        return Vector3.right;
                    }
                    else animator.SetBool("isMoving", false);
            if (transform.position.x > LeftBound)
                if (!(BlockReverse && lastMove == Vector3.right))
                    if (joystick.Horizontal < -0.3)
                    {
                        animator.SetBool("isMoving", true);
                        transform.eulerAngles = new Vector3(90f, 270f, 0f);
                        return Vector3.left;
                    }
                    else animator.SetBool("isMoving", false);
            if (transform.position.z < UpBound)
                if (!(BlockReverse && lastMove == Vector3.back))
                    if (joystick.Vertical > 0.3)
                    {
                        animator.SetBool("isMoving", true);
                        transform.eulerAngles = new Vector3(90f, 0f, 0f);
                        return Vector3.forward;
                    }
                    else animator.SetBool("isMoving", false);
            if (transform.position.z > DownBound)
                if (!(BlockReverse && lastMove == Vector3.forward))
                    if (joystick.Vertical < -0.3)
                    {
                        animator.SetBool("isMoving", true);
                        transform.eulerAngles = new Vector3(90f, 180f, 0f);
                        return Vector3.back;
                    }
                    else animator.SetBool("isMoving", false);
            return null;
        }

        if (transform.position.x < RightBound)
            if (!(BlockReverse && lastMove == Vector3.left))
                if (Input.GetKey(Right))
                {
                    animator.SetBool("isMoving", true);
                    transform.eulerAngles = new Vector3(90f, 90f, 0f);
                    return Vector3.right;
                }
                else animator.SetBool("isMoving", false);
        if (transform.position.x > LeftBound)
            if (!(BlockReverse && lastMove == Vector3.right))
                if (Input.GetKey(Left))
                {
                    animator.SetBool("isMoving", true);
                    transform.eulerAngles = new Vector3(90f, 270f, 0f);
                    return Vector3.left;
                }
                else animator.SetBool("isMoving", false);
        if (transform.position.z < UpBound)
            if (!(BlockReverse && lastMove == Vector3.back))
                if (Input.GetKey(Up))
                {
                    animator.SetBool("isMoving", true);
                    transform.eulerAngles = new Vector3(90f, 0f, 0f);
                    return Vector3.forward;
                }
                else animator.SetBool("isMoving", false);
        if (transform.position.z > DownBound)
            if (!(BlockReverse && lastMove == Vector3.forward))
                if (Input.GetKey(Down))
                {
                    animator.SetBool("isMoving", true);
                    transform.eulerAngles = new Vector3(90f, 180f, 0f);
                    return Vector3.back;

                }
                else animator.SetBool("isMoving", false);
        return null;
    }

    protected IEnumerator MoveObject(Vector3 direction)
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
        lastMove = direction;
        AfterStep?.Invoke(this, null);
    }
    #endregion
}