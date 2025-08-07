using UnityEngine;

public class DragObject : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Drag Type")]
    public DragType dragType = DragType.Free;

    [Header("Object Connector (Optional)")]
    public GameObject connectorObject;

    [Header("Free Drag Settings")]
    public Vector3 localDragOffset = new Vector3(1f, 1.5f, -2f); // Base offset relative to player

    [Header("Constrained Drag Settings")]
    public Transform startPoint;
    public Transform endPoint;
    [Range(0f, 1f)]
    public float currentProgress = 0f; // 0 = start, 1 = end
    public bool showConstraintGizmos = true;

    [Header("Sway Settings")]
    public float swayAmount = 0.1f;
    public float swayFrequency = 4f;

    [Header("Drag Settings")]
    public float dragDistance = 5f;
    public LayerMask collisionMask;
    public Vector3 checkBoxSize = new Vector3(1f, 1f, 1f);

    [Header("UI Prompt")]
    public GameObject promptSprite; // Assign your "E" button sprite here
    public Vector3 promptOffset = new Vector3(-1f, 1f, 0); // Offset from object center (top-left)
    public float jiggleAmount = 10f; // Rotation amount in degrees
    public float jiggleSpeed = 3f; // How fast it jiggles

    private Rigidbody rb;
    private Collider col;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private GravityController gravityController;
    private GravState gravState;

    // Constrained drag variables
    private Vector3 dragStartWorldPos;
    private float initialProgress;
    private Vector3 playerStartPos;

    public enum DragType
    {
        Free,           // Normal dragging anywhere
        Constrained     // Only between start and end points
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        gravityController = player.GetComponent<GravityController>();
        gravState = GetComponent<GravState>();

        if (!rb || !col || !player)
            Debug.LogError("Missing references!");

        // Validate constrained drag setup
        if (dragType == DragType.Constrained && (startPoint == null || endPoint == null))
        {
            Debug.LogError("Constrained drag requires both start and end points!");
        }

        // Make sure prompt starts hidden
        if (promptSprite != null)
        {
            promptSprite.SetActive(false);
        }
    }

    void Update()
    {
        // Handle UI prompt visibility
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= dragDistance;

        if (promptSprite != null)
        {
            // Show prompt when in range and not dragging
            if (inRange && !isDragging)
            {
                if (!promptSprite.activeInHierarchy)
                {
                    promptSprite.SetActive(true);
                }
                // Update prompt position to follow this object
                promptSprite.transform.position = transform.position + promptOffset;
                //promptSprite.transform.position = new Vector3(transform.position.x + promptOffset.x, transform.position.y + promptOffset.y, 0);
                //Vector3 uiPos = new Vector3(transform.position.x + promptOffset.x, transform.position.y + promptOffset.y, 0);
                //promptSprite.transform.position = uiPos;
                //promptSprite.transform.localPosition = new Vector3(promptSprite.transform.localPosition.x, promptSprite.transform.localPosition.y, 0);

                // Add smooth rotation jiggle
                float jiggleRotation = Mathf.Sin(Time.time * jiggleSpeed) * jiggleAmount;
                promptSprite.transform.rotation = Quaternion.Euler(0, 0, jiggleRotation);
            }
            // Hide prompt when out of range or dragging
            else if (promptSprite.activeInHierarchy)
            {
                promptSprite.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
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
            if (dragType == DragType.Free)
            {
                UpdateFreeDrag();
            }
            else if (dragType == DragType.Constrained)
            {
                UpdateConstrainedDrag();
            }
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

        if (connectorObject != null)
        {
            connectorObject.SetActive(true); // 👈 拖拽开始时激活
        }

        if (dragType == DragType.Free)
        {
            StartFreeDrag();
        }
        else if (dragType == DragType.Constrained)
        {
            StartConstrainedDrag();
        }
    }

    void StartFreeDrag()
    {
        // Store world position before parenting
        Vector3 worldPos = transform.position;
        transform.SetParent(player);

        // Calculate local offset based on player's right vector
        Vector3 offsetDirection = transform.position.x < player.position.x ?
            -player.right : player.right;
        Vector3 targetWorldPos = player.position + offsetDirection * 2f + Vector3.up * 1f;
        transform.position = targetWorldPos;

        // Maintain original world rotation
        transform.rotation = originalRotation;
    }

    void StartConstrainedDrag()
    {
        // Store initial states for constrained dragging
        dragStartWorldPos = transform.position;
        playerStartPos = player.position;
        initialProgress = currentProgress;

        // Make sure physics don't interfere with constrained movement
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void UpdateFreeDrag()
    {
        // Apply 2D sway (X and Y)
        float swayX = Mathf.Cos(Time.time * swayFrequency) * swayAmount;
        float swayY = Mathf.Sin(Time.time * swayFrequency) * swayAmount;
        Vector3 swayOffset = new Vector3(swayX, swayY, 0f);
        transform.localPosition = localDragOffset + swayOffset;
    }

    void UpdateConstrainedDrag()
    {
        if (startPoint == null || endPoint == null) return;

        // Calculate player movement from drag start
        Vector3 playerMovement = player.position - playerStartPos;

        // Get the constraint direction (from start to end)
        Vector3 constraintDirection = (endPoint.position - startPoint.position).normalized;
        float constraintLength = Vector3.Distance(startPoint.position, endPoint.position);

        // Project player movement onto the constraint direction
        float projectedMovement = Vector3.Dot(playerMovement, constraintDirection);

        // Convert movement to progress change
        float progressChange = projectedMovement / constraintLength;

        // Update current progress
        currentProgress = Mathf.Clamp01(initialProgress + progressChange);

        // Update object position
        UpdateConstrainedPosition();

        // Apply subtle sway perpendicular to constraint direction
        Vector3 perpendicular = Vector3.Cross(constraintDirection, Vector3.forward);
        if (perpendicular.magnitude < 0.1f) // If constraint is along Z, use different perpendicular
            perpendicular = Vector3.Cross(constraintDirection, Vector3.up);

        float swayOffset = Mathf.Sin(Time.time * swayFrequency) * swayAmount * 0.3f; // Reduced sway for constraints
        Vector3 swayPosition = perpendicular.normalized * swayOffset;

        transform.position += swayPosition;
    }

    void UpdateConstrainedPosition()
    {
        if (startPoint != null && endPoint != null)
        {
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, currentProgress);
        }
    }

    void StopDragging()
    {
        isDragging = false;

        if (dragType == DragType.Free)
        {
            transform.SetParent(originalParent);
        }

        if (connectorObject != null)
        {
            connectorObject.SetActive(false); // 👈 拖拽停止时关闭
        }

        // Re-enable physics for both drag types
        rb.useGravity = true;
        rb.isKinematic = false;
        Physics.IgnoreCollision(col, player.GetComponent<Collider>(), false);

        Debug.Log("Stopped dragging.");
    }

    void OnDrawGizmosSelected()
    {
        // Draw free drag gizmo
        if (dragType == DragType.Free && Application.isPlaying && isDragging)
        {
            Gizmos.color = Color.cyan;
            Vector3 worldTargetPos = player.TransformPoint(localDragOffset);
            Gizmos.DrawWireCube(worldTargetPos, checkBoxSize);
        }

        // Draw constrained drag gizmos
        if (dragType == DragType.Constrained && showConstraintGizmos && startPoint != null && endPoint != null)
        {
            // Draw constraint line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // Draw start and end points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.2f);

            // Draw current position
            Gizmos.color = Color.blue;
            Vector3 currentPos = Vector3.Lerp(startPoint.position, endPoint.position, currentProgress);
            Gizmos.DrawWireSphere(currentPos, 0.15f);

            // Draw progress indicator
            Gizmos.color = Color.white;
            Vector3 labelPos = currentPos + Vector3.up * 0.5f;
        }

        // Draw drag distance
        if (player != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, dragDistance);
        }
    }

    // Inspector helper methods
    [ContextMenu("Set Current Position as Start")]
    void SetCurrentAsStart()
    {
        if (startPoint == null)
        {
            GameObject startGO = new GameObject(gameObject.name + "_StartPoint");
            startPoint = startGO.transform;
        }
        startPoint.position = transform.position;
        currentProgress = 0f;
    }

    [ContextMenu("Set Current Position as End")]
    void SetCurrentAsEnd()
    {
        if (endPoint == null)
        {
            GameObject endGO = new GameObject(gameObject.name + "_EndPoint");
            endPoint = endGO.transform;
        }
        endPoint.position = transform.position;
        currentProgress = 1f;
    }

    [ContextMenu("Go to Start")]
    void GoToStart()
    {
        currentProgress = 0f;
        if (dragType == DragType.Constrained && startPoint != null && endPoint != null)
        {
            transform.position = startPoint.position;
        }
    }

    [ContextMenu("Go to End")]
    void GoToEnd()
    {
        currentProgress = 1f;
        if (dragType == DragType.Constrained && startPoint != null && endPoint != null)
        {
            transform.position = endPoint.position;
        }
    }
}
