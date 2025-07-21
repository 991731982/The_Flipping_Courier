using UnityEngine;

public class DragObject : MonoBehaviour
{
    public Transform player;
    public Vector3 localDragOffset = new Vector3(1f, 1f, 1f); // Base offset relative to player

    [Header("Sway Settings")]
    public float swayAmount = 0.1f;
    public float swayFrequency = 4f;

    [Header("Drag Settings")]
    public float dragDistance = 5f;
    public LayerMask collisionMask;
    public Vector3 checkBoxSize = new Vector3(1f, 1f, 1f);

    private Rigidbody rb;
    private Collider col;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    private GravityController gravityController;
    private GravState gravState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        gravityController = player.GetComponent<GravityController>();
        gravState = GetComponent<GravState>();

        if (!rb || !col || !player)
            Debug.LogError("Missing references!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (gravState != null && gravState.CurrentState == Bullet.BulletType.Heavy)
            {
                Debug.Log("Can't drag — object is heavy.");
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            if (isDragging)
            {
                StopDragging();
            }
            else if (distance <= dragDistance)
            {
                StartDragging();
            }
        }

        if (isDragging)
        {
            // Apply 2D sway (X and Y)
            float swayX = Mathf.Cos(Time.time * swayFrequency) * swayAmount;
            float swayY = Mathf.Sin(Time.time * swayFrequency) * swayAmount;

            Vector3 swayOffset = new Vector3(swayX, swayY, 0f);
            transform.localPosition = localDragOffset + swayOffset;
        }
    }

    void StartDragging()
    {
        isDragging = true;
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        rb.useGravity = false;
        rb.isKinematic = true;
        Physics.IgnoreCollision(col, player.GetComponent<Collider>(), true);

        // Store world position before parenting
        Vector3 worldPos = transform.position;
        transform.SetParent(player);

        // Calculate local offset based on player's right vector (not world space)
        Vector3 offsetDirection = transform.position.x < player.position.x ?
            -player.right : player.right;

        Vector3 targetWorldPos = player.position + offsetDirection * 2f + Vector3.up * 1f;
        transform.position = targetWorldPos;

        // Maintain original world rotation
        transform.rotation = originalRotation;
    }

    void StopDragging()
    {
        isDragging = false;

        transform.SetParent(originalParent);
        rb.useGravity = true;
        rb.isKinematic = false;

        Physics.IgnoreCollision(col, player.GetComponent<Collider>(), false);

        Debug.Log("Stopped dragging.");
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !isDragging) return;

        Gizmos.color = Color.cyan;
        Vector3 worldTargetPos = player.TransformPoint(localDragOffset);
        Gizmos.DrawWireCube(worldTargetPos, checkBoxSize);
    }
}