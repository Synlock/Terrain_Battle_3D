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
    Vector3 lastMove = Vector3.zero;

    public int moves;

    [SerializeField] public bool BlockReverse = false;

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


    bool isMoving = false;
  
    public event EventHandler BeforeStep;
    public event EventHandler AfterStep;

    #region Start/Update
    void Start()
    {
        joystick = FindObjectOfType<FloatingJoystick>();
    }

    void Update()
    {
        if (GameManager.isGameOver) 
        {
            joystick.gameObject.SetActive(false);
            return; 
        }

        Vector3? dir = GetDirection();
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
                        return Vector3.right;
            if (transform.position.x > LeftBound)
                if (!(BlockReverse && lastMove == Vector3.right))
                    if (joystick.Horizontal < -0.3)
                        return Vector3.left;
            if (transform.position.z < UpBound)
                if (!(BlockReverse && lastMove == Vector3.back))
                    if (joystick.Vertical > 0.3)
                        return Vector3.forward;
            if (transform.position.z > DownBound)
                if (!(BlockReverse && lastMove == Vector3.forward))
                    if (joystick.Vertical < -0.3)
                        return Vector3.back;
            return null;
        }

        if (transform.position.x < RightBound)
            if (!(BlockReverse && lastMove == Vector3.left))
                if (Input.GetKey(Right))
                    return Vector3.right;
        if (transform.position.x > LeftBound)
            if (!(BlockReverse && lastMove == Vector3.right))
                if (Input.GetKey(Left))
                    return Vector3.left;
        if (transform.position.z < UpBound)
            if (!(BlockReverse && lastMove == Vector3.back))
                if (Input.GetKey(Up))
                    return Vector3.forward;
        if (transform.position.z > DownBound)
            if (!(BlockReverse && lastMove == Vector3.forward))
                if (Input.GetKey(Down))
                    return Vector3.back;

        return null;
    }

    IEnumerator MoveObject(Vector3 direction)
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