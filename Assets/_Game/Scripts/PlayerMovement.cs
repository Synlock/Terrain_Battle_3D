using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float zSpeed = 10f;
    [SerializeField] float xSpeed = 10f;
    [SerializeField] float lerpSpeed = 1f;
    [SerializeField] float xRange = 10f;

    Rigidbody rb;
    Transform myTransform;

    bool notPressing = true;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myTransform = transform;
    }
    void Update()
    {
        MoveHorizontally();

        if (Input.GetMouseButtonUp(0))
        {
            notPressing = true;
        }
    }
    void FixedUpdate()
    {
        MoveForward();

        if (notPressing)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, rb.velocity.z);
        }
    }
    void MoveForward()
    {
        rb.AddForce(Vector3.forward * zSpeed * Time.deltaTime, ForceMode.Impulse);
    }
    void MoveHorizontally()
    {
        if (Input.GetMouseButton(0))
        {
            float xMovement = 0f;
            xMovement += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;

            Vector3 targetPos = new Vector3(xMovement, 0f, 0f);
            Vector3 rawXPos = targetPos + myTransform.position;
            Vector3 clampedXPos = new Vector3(Mathf.Clamp(rawXPos.x, -xRange, xRange), transform.position.y, transform.position.z);

            myTransform.position = Vector3.Lerp(myTransform.position, clampedXPos, lerpSpeed * Time.deltaTime);

            notPressing = false;
        }
    }
}