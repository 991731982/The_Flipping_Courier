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
    private float jumpCooldown = 0.25f; // seconds

    // Movement smoothing
    public float movementSmoothing = 10f;

    // Input toggle
    private bool inputEnabled = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityController = GetComponent<GravityController>();
        groundjump = new Vector3(0.0f, 2.0f, 0.0f);
        roofjump = new Vector3(0.0f, -2.0f, 0.0f);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Grounded detection based on contact normal and gravity direction
            if ((!gravityController.gravityFlipped && contact.normal.y > 0.5f) ||
                (gravityController.gravityFlipped && contact.normal.y < -0.5f))
            {
                isGrounded = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Reset grounded flag on collision exit
        isGrounded = false;
    }

    void Update()
    {
        if (!inputEnabled) return; // Skip all movement input if disabled

        Vector3 moveDirection = Vector3.zero;

        // Read horizontal input
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection += Vector3.right;
        }

        // Target horizontal velocity
        Vector3 targetVelocity = new Vector3(moveDirection.x * movementspeed, rb.linearVelocity.y, rb.linearVelocity.z);

        // Smoothly interpolate to target velocity
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * movementSmoothing);

        // Only rotate character while grounded and moving
        if (isGrounded && moveDirection.x != 0)
        {
            float direction = moveDirection.x > 0 ? 1f : -1f;

            // Rotate to face direction
            Quaternion targetRotation = Quaternion.LookRotation(
                new Vector3(-direction, 0f, 0f),  // Forward (face left or right)
                gravityController.gravityFlipped ? Vector3.down : Vector3.up // Up direction based on gravity
            );

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Adjust up direction for gravity flip
        FixCharacterUpDirection();

        // Jump with cooldown and grounded check
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && Time.time - lastJumpTime > jumpCooldown)
        {
            Vector3 jumpDirection = gravityController.gravityFlipped ? roofjump : groundjump;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            lastJumpTime = Time.time;
        }
    }

    private void FixCharacterUpDirection()
    {
        // Set the "up" direction depending on gravity flipped state
        Vector3 targetUp = gravityController.gravityFlipped ? Vector3.down : Vector3.up;
        Vector3 targetForward = transform.forward;

        // Apply smooth orientation fix
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    // Public input toggle methods
    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z); // Stop movement when frozen
    }
}