using UnityEngine;

public class DragObject : MonoBehaviour
{
    public Transform player;
    public float dragDistance = 5f;
    public float followSpeed = 5f;
    public Vector3 dragOffset = new Vector3(1f, 0, 1f); // X:左右偏移, Z:前后偏移
    public float yOffset = 1.5f;
    public float gravityOffset = 1.5f;

    [Header("碰撞检测")]
    public LayerMask collisionMask;
    public Vector3 checkBoxSize = new Vector3(1f, 1f, 1f); // 拖动过程中检测碰撞的盒子大小

    private Rigidbody rb;
    private bool isDragging = false;
    private bool isOnRightSide = true;
    private GravityController gravityController;
    private GravState gravState;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
          rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        gravityController = player.GetComponent<GravityController>();
        if (gravityController == null)
        {
            Debug.LogWarning("GravityController not found on player. Gravity flip may not work.");
        }

        gravState = GetComponent<GravState>();
        if (gravState == null)
        {
            Debug.LogWarning("GravState not found on object!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (gravState != null && gravState.CurrentState == Bullet.BulletType.Heavy)
            {
                Debug.Log("Cannot drag object: it's in HEAVY state.");
                return;
            }

            float dist = Vector3.Distance(transform.position, player.position);

            if (isDragging)
            {
                StopDragging();
            }
            else if (dist <= dragDistance)
            {
                StartDragging();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            if (Vector3.Distance(transform.position, player.position) > dragDistance)
            {
                StopDragging();
                return;
            }

            Vector3 sideOffset = dragOffset;
            sideOffset.x = isOnRightSide ? Mathf.Abs(dragOffset.x) : -Mathf.Abs(dragOffset.x);
            Vector3 targetPos = player.position + sideOffset;
            targetPos.y += gravityController != null && gravityController.gravityFlipped ? -gravityOffset : yOffset;

            // ✅ 检测障碍，如果碰到墙，就停下来
            if (Physics.CheckBox(targetPos, checkBoxSize * 0.5f, Quaternion.identity, collisionMask))
            {
                Debug.Log("Dragging blocked by collision. Releasing.");
                StopDragging();
                return;
            }

            // ✅ 保留物理移动方式（不会穿墙）
            Vector3 smoothedPos = Vector3.Lerp(rb.position, targetPos, followSpeed * Time.fixedDeltaTime);
            rb.MovePosition(smoothedPos);
        }
    }

    void StartDragging()
    {
        isDragging = true;
        isOnRightSide = transform.position.x >= player.position.x;

        // ✅ 保持碰撞参与，去掉 isKinematic
        rb.useGravity = false;
        rb.isKinematic = false; // ✅ 让它保持受物理控制，这样才有碰撞！

        Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        Debug.Log("Started dragging.");
    }

    void StopDragging()
    {
        isDragging = false;
        rb.useGravity = true;
        rb.isKinematic = false;

        Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        Debug.Log("Stopped dragging.");
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !isDragging) return;

        Vector3 sideOffset = dragOffset;
        sideOffset.x = isOnRightSide ? Mathf.Abs(dragOffset.x) : -Mathf.Abs(dragOffset.x);
        Vector3 targetPos = player.position + sideOffset;
        targetPos.y += gravityController != null && gravityController.gravityFlipped ? -gravityOffset : yOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPos, checkBoxSize);
    }
}