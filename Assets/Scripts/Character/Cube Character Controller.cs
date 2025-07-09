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
            // Detect ground or roof based on gravity and ignore side contacts
            if ((!gravityController.gravityFlipped && contact.normal.y > 0.5f) ||
                (gravityController.gravityFlipped && contact.normal.y < -0.5f))
            {
                isGrounded = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // When exiting collision, set grounded to false
        isGrounded = false;
    }

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        // Movement input
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left * movementspeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right * movementspeed;
        }

        // Cap horizontal movement speed
        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, rb.linearVelocity.z);

        // 如果有移动方向，则更新角色朝向
        if (moveDirection.x != 0)
        {
            Vector3 targetForward = gravityController.gravityFlipped
                ? new Vector3(-moveDirection.x, 0, 0)
                : new Vector3(-moveDirection.x, 0, 0);
            Quaternion targetRotation = Quaternion.LookRotation(targetForward, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }


        // 修正角色的上下方向
        FixCharacterUpDirection();

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 jumpDirection = gravityController.gravityFlipped ? roofjump : groundjump;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            isGrounded = false; // Prevent further jumps until grounded again
        }
    }


    private void FixCharacterUpDirection()
    {
        // 根据重力状态修正角色的上下方向，但保持左右方向
        Vector3 targetUp = gravityController.gravityFlipped ? Vector3.down : Vector3.up;

        // 计算修正后的方向，同时保留当前左右方向（通过forward方向）
        Vector3 targetForward = transform.forward; // 保留当前朝向
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);

        // 立即或平滑设置角色的旋转
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}