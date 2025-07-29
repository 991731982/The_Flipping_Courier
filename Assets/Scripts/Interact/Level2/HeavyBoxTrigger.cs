using UnityEngine;

public class HeavyBoxTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string triggerTag = "Box";                       // Tag to check for collision
    public bool debugMode = true;                           // Enable detailed debug logs
    public bool requireHeavyState = true;                   // Require the box to be in Heavy state

    [Header("Fracture Settings")]
    public float explosionForce = 20f;                      // Explosion force when fracturing
    public bool disableAfterFracture = true;               // Disable this component after fracturing

    private FracturedObject fracturedObject;
    private Collider objectCollider;
    private Rigidbody objectRigidbody;
    private bool hasTriggered = false;

    void Start()
    {
        // Get the FracturedObject component
        fracturedObject = GetComponent<FracturedObject>();
        objectCollider = GetComponent<Collider>();
        objectRigidbody = GetComponent<Rigidbody>();

        if (fracturedObject == null)
        {
            Debug.LogError("SimpleFractureTrigger: No FracturedObject component found on " + gameObject.name);
            return;
        }

        // Ensure the object starts in a stable state
        SetupInitialState();

        if (debugMode)
        {
            Debug.Log($"SimpleFractureTrigger initialized on {gameObject.name}");
        }
    }

    private void SetupInitialState()
    {
        // Make sure the main object is stable initially
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        // Ensure the main collider is enabled
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        // Make sure single mesh visibility is enabled (unfractured appearance)
        fracturedObject.SetSingleMeshVisibility(true);

        // Remove CheckDynamicCollision component if it exists to prevent auto-fracturing
        CheckDynamicCollision dynamicCollision = GetComponent<CheckDynamicCollision>();
        if (dynamicCollision != null)
        {
            DestroyImmediate(dynamicCollision);
        }

        // Disable chunk colliders initially but ensure they have rigidbodies
        SetupInitialChunkPhysics();
        DisableChunkColliders();
    }

    private void SetupInitialChunkPhysics()
    {
        if (fracturedObject?.ListFracturedChunks == null) return;

        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null)
            {
                // Ensure chunk has a rigidbody but keep it kinematic initially
                Rigidbody chunkRb = chunk.GetComponent<Rigidbody>();
                if (chunkRb == null)
                {
                    chunkRb = chunk.gameObject.AddComponent<Rigidbody>();
                }

                // Keep chunks kinematic and without gravity initially
                chunkRb.isKinematic = true;
                chunkRb.useGravity = false;

                if (debugMode) Debug.Log($"Initial setup for chunk: {chunk.name}");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (debugMode)
        {
            Debug.Log($"SimpleFractureTrigger: Collision detected with {collision.gameObject.name}");
        }

        // Prevent multiple triggers
        if (hasTriggered)
        {
            if (debugMode) Debug.Log("SimpleFractureTrigger: Already triggered, ignoring collision");
            return;
        }

        // Check if the collision meets our criteria
        if (!IsValidTriggerCollision(collision)) return;

        Debug.Log($"FRACTURE TRIGGERED! Object: {collision.gameObject.name} collided with {gameObject.name}");

        // Mark as triggered
        hasTriggered = true;

        // Get collision point for explosion position
        Vector3 explosionPosition = collision.contacts.Length > 0 ?
            collision.contacts[0].point :
            transform.position;

        // Trigger the fracture
        TriggerFracture(explosionPosition);
    }

    private bool IsValidTriggerCollision(Collision collision)
    {
        // Check if collision object has the required tag
        if (!collision.gameObject.CompareTag(triggerTag))
        {
            if (debugMode) Debug.Log($"SimpleFractureTrigger: Object {collision.gameObject.name} doesn't have tag '{triggerTag}' (has '{collision.gameObject.tag}')");
            return false;
        }

        // If we require heavy state, check for it
        if (requireHeavyState)
        {
            GravState gravState = collision.gameObject.GetComponent<GravState>();
            if (gravState == null)
            {
                if (debugMode) Debug.Log($"SimpleFractureTrigger: Object {collision.gameObject.name} has no GravState component");
                return false;
            }

            if (gravState.CurrentState != GravState.GravityState.Heavy)
            {
                if (debugMode) Debug.Log($"SimpleFractureTrigger: Object {collision.gameObject.name} is not in Heavy state (current: {gravState.CurrentState})");
                return false;
            }

            if (debugMode) Debug.Log($"SimpleFractureTrigger: Object {collision.gameObject.name} is in Heavy state - criteria met!");
        }

        return true;
    }

    private void TriggerFracture(Vector3 explosionPosition)
    {
        if (fracturedObject == null) return;

        if (debugMode) Debug.Log($"SimpleFractureTrigger: Triggering fracture at position {explosionPosition}");

        // First, disable the main object's collider and rigidbody
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        // CRITICAL: Hide the original mesh BEFORE showing chunks
        HideOriginalMesh();

        // Disable single mesh visibility to show individual chunks
        fracturedObject.SetSingleMeshVisibility(false);

        // Ensure all chunks have proper rigidbodies and colliders
        SetupChunkPhysics();

        // Enable chunk colliders and make them non-kinematic
        EnableChunkColliders();

        // IMPORTANT: Add a small delay before explosion to ensure physics setup is complete
        StartCoroutine(DelayedExplosion(explosionPosition, 0.1f));

        if (debugMode) Debug.Log($"SimpleFractureTrigger: Fracture setup completed for {gameObject.name}");
    }

    private void HideOriginalMesh()
    {
        // Hide the main mesh renderer
        Renderer mainRenderer = GetComponent<Renderer>();
        if (mainRenderer != null)
        {
            mainRenderer.enabled = false;
            if (debugMode) Debug.Log($"Hidden main renderer on {gameObject.name}");
        }

        // Also hide any mesh renderers on child objects that might be part of the original mesh
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            // Only hide renderers that are not part of fractured chunks
            if (!IsFracturedChunkRenderer(renderer))
            {
                renderer.enabled = false;
                if (debugMode) Debug.Log($"Hidden child renderer on {renderer.gameObject.name}");
            }
        }
    }

    private bool IsFracturedChunkRenderer(Renderer renderer)
    {
        // Check if this renderer belongs to a fractured chunk
        FracturedChunk chunk = renderer.GetComponent<FracturedChunk>();
        if (chunk != null) return true;

        // Also check parent objects for FracturedChunk component
        Transform parent = renderer.transform.parent;
        while (parent != null && parent != transform)
        {
            if (parent.GetComponent<FracturedChunk>() != null)
                return true;
            parent = parent.parent;
        }

        return false;
    }

    private System.Collections.IEnumerator DelayedExplosion(Vector3 explosionPosition, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (debugMode) Debug.Log($"SimpleFractureTrigger: Executing delayed explosion at {explosionPosition}");

        // Trigger the explosion using the FracturedObject's built-in method
        fracturedObject.Explode(explosionPosition, explosionForce);

        // Optionally disable this component after fracturing
        if (disableAfterFracture)
        {
            this.enabled = false;
        }

        if (debugMode) Debug.Log($"SimpleFractureTrigger: Explosion completed for {gameObject.name}");
    }

    private void SetupChunkPhysics()
    {
        if (fracturedObject?.ListFracturedChunks == null) return;

        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null)
            {
                // Ensure chunk has a rigidbody
                Rigidbody chunkRb = chunk.GetComponent<Rigidbody>();
                if (chunkRb == null)
                {
                    chunkRb = chunk.gameObject.AddComponent<Rigidbody>();
                }

                // Initially keep chunks kinematic until explosion
                chunkRb.isKinematic = false;
                chunkRb.useGravity = true;

                // Set physics material if main object has one
                if (fracturedObject.ChunkPhysicMaterial != null)
                {
                    Collider chunkCollider = chunk.GetComponent<Collider>();
                    if (chunkCollider != null)
                    {
                        chunkCollider.material = fracturedObject.ChunkPhysicMaterial;
                    }
                }

                if (debugMode) Debug.Log($"Setup physics for chunk: {chunk.name}");
            }
        }

        // Compute masses for all chunks
        fracturedObject.ComputeChunksRelativeVolume();
        fracturedObject.ComputeChunksMass(fracturedObject.TotalMass);
    }

    private void DisableChunkColliders()
    {
        if (fracturedObject?.ListFracturedChunks == null) return;

        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null)
            {
                SetChunkCollidersRecursive(chunk.gameObject, false);
            }
        }
    }

    private void EnableChunkColliders()
    {
        if (fracturedObject?.ListFracturedChunks == null) return;

        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null)
            {
                SetChunkCollidersRecursive(chunk.gameObject, true);
            }
        }
    }

    private void SetChunkCollidersRecursive(GameObject obj, bool enable)
    {
        // Enable/disable collider on current object
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = enable;
            if (enable)
            {
                collider.isTrigger = false; // Ensure it's not a trigger when enabled

                // Make sure the collider is properly configured
                if (fracturedObject.ChunkColliderType == FracturedObject.ColliderType.Collider)
                {
                    collider.isTrigger = false;
                }
                else
                {
                    collider.isTrigger = true;
                }
            }

            if (debugMode && enable) Debug.Log($"Enabled collider on: {obj.name}, IsTrigger: {collider.isTrigger}");
        }

        // Recursively handle children
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetChunkCollidersRecursive(obj.transform.GetChild(i).gameObject, enable);
        }
    }

    /// <summary>
    /// Reset the trigger for testing purposes
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        if (fracturedObject != null && fracturedObject.ResetChunks())
        {
            hasTriggered = false;
            SetupInitialState();
            this.enabled = true;
            Debug.Log($"SimpleFractureTrigger: {gameObject.name} reset successfully");
        }
        else
        {
            Debug.LogWarning($"SimpleFractureTrigger: Failed to reset {gameObject.name}");
        }
    }

    /// <summary>
    /// Manual trigger for testing
    /// </summary>
    [ContextMenu("Manual Trigger")]
    public void ManualTrigger()
    {
        if (!hasTriggered && fracturedObject != null)
        {
            Debug.Log("SimpleFractureTrigger: Manual trigger activated");
            hasTriggered = true;
            TriggerFracture(transform.position);
        }
    }

    void OnValidate()
    {
        // Ensure explosion force is not negative
        if (explosionForce < 0)
        {
            explosionForce = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visual indicator in scene view
        Gizmos.color = hasTriggered ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Show explosion force as a wireframe
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionForce * 0.1f);
    }
}