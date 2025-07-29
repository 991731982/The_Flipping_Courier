using UnityEngine;

public class HeavyBoxTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public float massThreshold = 25f;                       // Lowered threshold to work with your setup
    public string triggerTag = "Box";                       // Tag to check for collision
    public bool debugMode = true;                           // Enable detailed debug logs

    [Header("Drop Animation")]
    public float dropDistance = 10f;                        // Distance to move down
    public float dropSpeed = 5f;                           // Speed of downward movement

    [Header("Fracture Target")]
    public StaticFracturedObject staticFracturedObject;     // The static fractured object to trigger

    // Alternative: if you want to keep using FracturedObject directly
    [Header("Alternative: Direct FracturedObject (Legacy)")]
    public FracturedObject fracturedObject;                 // Direct reference (not recommended)
    public float explosionForce = 20f;                      // Explosion force for direct method

    private bool shouldDrop = false;
    private bool hasTriggered = false;
    private Vector3 targetPosition;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
        targetPosition = transform.position + Vector3.down * dropDistance;

        // Validate setup
        if (staticFracturedObject == null && fracturedObject == null)
        {
            Debug.LogWarning("HeavyBoxTrigger: No target fractured object assigned!");
        }

        if (debugMode)
        {
            Debug.Log($"HeavyBoxTrigger initialized - Threshold: {massThreshold}, Tag: {triggerTag}");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (debugMode)
        {
            Debug.Log($"HeavyBoxTrigger: Collision detected with {collision.gameObject.name}");
        }

        // Prevent multiple triggers
        if (hasTriggered)
        {
            if (debugMode) Debug.Log("HeavyBoxTrigger: Already triggered, ignoring collision");
            return;
        }

        // Check if the collision meets our criteria
        if (!IsValidTriggerCollision(collision)) return;

        Debug.Log($"Heavy Box collision TRIGGERED! Object: {collision.gameObject.name}, Mass: {collision.rigidbody.mass}");

        // Mark as triggered
        hasTriggered = true;
        shouldDrop = true;

        // Trigger the fracture
        TriggerFracture(collision);
    }

    private bool IsValidTriggerCollision(Collision collision)
    {
        // Check if collision object has the required tag
        if (!collision.gameObject.CompareTag(triggerTag))
        {
            if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} doesn't have tag '{triggerTag}' (has '{collision.gameObject.tag}')");
            return false;
        }

        // Check if it has a rigidbody
        Rigidbody rb = collision.rigidbody;
        if (rb == null)
        {
            if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} has no rigidbody");
            return false;
        }

        // Get the actual mass (could be modified by GravState)
        float actualMass = rb.mass;

        // Also check for GravState component to get the effective mass
        GravState gravState = collision.gameObject.GetComponent<GravState>();
        if (gravState != null && gravState.CurrentState == GravState.GravityState.Heavy)
        {
            if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} is in Heavy state");
        }

        // Check if mass meets threshold
        if (actualMass < massThreshold)
        {
            if (debugMode) Debug.Log($"HeavyBoxTrigger: Box mass ({actualMass}) below threshold ({massThreshold})");
            return false;
        }

        if (debugMode) Debug.Log($"HeavyBoxTrigger: Valid collision - Mass: {actualMass}, Threshold: {massThreshold}");
        return true;
    }

    private void TriggerFracture(Collision collision)
    {
        Vector3 explosionPosition = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;

        if (debugMode) Debug.Log($"HeavyBoxTrigger: Triggering fracture at position {explosionPosition}");

        // Use StaticFracturedObject if available (recommended)
        if (staticFracturedObject != null)
        {
            staticFracturedObject.TriggerFracture(explosionPosition);
        }
        // Fallback to direct FracturedObject method (legacy)
        else if (fracturedObject != null)
        {
            // Disable the main collider and enable chunk colliders manually
            Collider mainCollider = fracturedObject.GetComponent<Collider>();
            if (mainCollider != null)
            {
                mainCollider.enabled = false;
            }

            Rigidbody mainRigidbody = fracturedObject.GetComponent<Rigidbody>();
            if (mainRigidbody != null)
            {
                mainRigidbody.isKinematic = true;
            }

            // Enable chunk colliders
            EnableFracturedObjectChunkColliders(fracturedObject, true);

            // Trigger explosion
            fracturedObject.Explode(explosionPosition, explosionForce);
        }
    }

    private void EnableFracturedObjectChunkColliders(FracturedObject fracObj, bool enable)
    {
        foreach (FracturedChunk chunk in fracObj.ListFracturedChunks)
        {
            if (chunk != null)
            {
                EnableChunkCollidersRecursive(chunk.gameObject, enable);
            }
        }
    }

    private void EnableChunkCollidersRecursive(GameObject obj, bool enable)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = enable;
            if (enable)
            {
                collider.isTrigger = false;
            }
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            EnableChunkCollidersRecursive(obj.transform.GetChild(i).gameObject, enable);
        }
    }

    void Update()
    {
        // Handle drop animation
        if (shouldDrop)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, dropSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                shouldDrop = false;
                if (debugMode) Debug.Log("HeavyBoxTrigger: Drop animation completed");
            }
        }
    }

    /// <summary>
    /// Reset the trigger for testing purposes
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        shouldDrop = false;
        transform.position = originalPosition;

        if (staticFracturedObject != null)
        {
            staticFracturedObject.ResetToStatic();
        }

        Debug.Log("HeavyBoxTrigger: Trigger reset");
    }

    /// <summary>
    /// Manual trigger for testing
    /// </summary>
    [ContextMenu("Manual Trigger")]
    public void ManualTrigger()
    {
        if (!hasTriggered)
        {
            Debug.Log("HeavyBoxTrigger: Manual trigger activated");
            hasTriggered = true;
            shouldDrop = true;

            if (staticFracturedObject != null)
            {
                staticFracturedObject.TriggerFracture(transform.position);
            }
            else if (fracturedObject != null)
            {
                fracturedObject.Explode(transform.position, explosionForce);
            }
        }
    }

    void OnValidate()
    {
        // Ensure mass threshold is not negative
        if (massThreshold < 0)
        {
            massThreshold = 0;
        }

        // Ensure drop distance is not negative
        if (dropDistance < 0)
        {
            dropDistance = 0;
        }

        // Ensure drop speed is positive
        if (dropSpeed <= 0)
        {
            dropSpeed = 1f;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the drop path in the scene view
        Gizmos.color = Color.yellow;
        Vector3 start = Application.isPlaying ? originalPosition : transform.position;
        Vector3 end = start + Vector3.down * dropDistance;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.5f);

        // Draw mass threshold info
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}