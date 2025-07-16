using UnityEngine;

public class CubeCharacterController : MonoBehaviour
{
    public Vector3 groundjump;
    public Vector3 roofjump;
    public float jumpForce = 2.0f;
    public bool isGrounded;
    private Rigidbody rb;
    public int movementspeed = 5;
    private GravityController gravityController;

    // Jump cooldown
    private float lastJumpTime = 0f;
    private float jumpCooldown = 0.25f;

    // Movement smoothing
    public float movementSmoothing = 10f;
    private Vector3 moveDirection = Vector3.zero;
    private bool shouldJump = false;

    // Input toggle
    private bool inputEnabled = true;

    // Target Y rotation for horizontal facing
    private float targetYRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityController = GetComponent<GravityController>();
        groundjump = new Vector3(0.0f, 2.0f, 0.0f);
        roofjump = new Vector3(0.0f, -2.0f, 0.0f);

        // Default facing direction
        targetYRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        if (!inputEnabled) return;

        moveDirection = Vector3.zero;

        // Read horizontal input
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection += Vector3.right;
        }

        // Handle jump request
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && Time.time - lastJumpTime > jumpCooldown)
        {
            shouldJump = true;
        }

        // Set target Y rotation based on direction
        if (moveDirection.x != 0)
        {
            float direction = moveDirection.x > 0 ? 1f : -1f;
            targetYRotation = direction > 0 ? 270f : 90f; // Right = 270, Left = 90
        }

        // Apply smooth combined rotation
        ApplyCombinedRotation();
    }

    void FixedUpdate()
    {
        if (!inputEnabled) return;

        // Apply smoothed movement
        Vector3 targetVelocity = new Vector3(moveDirection.x * movementspeed, rb.linearVelocity.y, rb.linearVelocity.z);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * movementSmoothing);

        // Perform jump
        if (shouldJump)
        {
            Vector3 jumpDirection = gravityController.gravityFlipped ? roofjump : groundjump;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            lastJumpTime = Time.time;
            shouldJump = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if ((!gravityController.gravityFlipped && contact.normal.y > 0.5f) ||
                (gravityController.gravityFlipped && contact.normal.y < -0.5f))
            {
                isGrounded = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void ApplyCombinedRotation()
    {
        float currentY = transform.eulerAngles.y;
        float currentZ = transform.eulerAngles.z;
        float targetZ = gravityController.CurrentZRotation;

        // Smoothly blend both rotations
        float smoothY = Mathf.LerpAngle(currentY, targetYRotation, Time.deltaTime * 10f);
        float smoothZ = Mathf.LerpAngle(currentZ, targetZ, Time.deltaTime * 10f);

        transform.rotation = Quaternion.Euler(0f, smoothY, smoothZ);
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);
    }
}