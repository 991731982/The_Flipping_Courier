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

    // Rotation
    private float targetYRotation;

    // Slope handling
    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f; // Maximum walkable slope angle
    private Vector3 surfaceNormal = Vector3.up;

    private float jumpBufferTime = 0.15f;
    private float jumpBufferTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityController = GetComponent<GravityController>();
        groundjump = new Vector3(0.0f, 2.0f, 0.0f);
        roofjump = new Vector3(0.0f, -2.0f, 0.0f);
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
        // Jump input buffering
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }

        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;

            if (isGrounded && Time.time - lastJumpTime > jumpCooldown)
            {
                shouldJump = true;
                jumpBufferTimer = 0f; // Consume buffer
            }
        }

        // Determine facing direction
        if (moveDirection.x != 0)
        {
            float direction = moveDirection.x > 0 ? 1f : -1f;
            targetYRotation = direction > 0 ? 270f : 90f; // Right = 270, Left = 90
        }

        ApplyCombinedRotation();
    }

    void FixedUpdate()
    {
        if (!inputEnabled) return;

        float slopeAngle = Vector3.Angle(surfaceNormal, gravityController.gravityFlipped ? Vector3.down : Vector3.up);
        bool canMoveOnSlope = slopeAngle <= maxSlopeAngle;

        if (canMoveOnSlope && moveDirection != Vector3.zero)
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.right * moveDirection.x, surfaceNormal).normalized;
            Vector3 targetVelocity = new Vector3(slopeDirection.x * movementspeed, rb.linearVelocity.y, rb.linearVelocity.z);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * movementSmoothing);
        }
        else
        {
            // Prevent movement on steep slope
            Vector3 targetVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * movementSmoothing);
        }

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
                surfaceNormal = contact.normal; // Store normal for slope handling
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        surfaceNormal = gravityController.gravityFlipped ? Vector3.down : Vector3.up; // Reset normal
    }

    private void ApplyCombinedRotation()
    {
        float currentY = transform.eulerAngles.y;
        float currentZ = transform.eulerAngles.z;
        float targetZ = gravityController.CurrentZRotation;

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