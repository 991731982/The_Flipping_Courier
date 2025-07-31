using UnityEngine;

public class HeavyBoxTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string triggerTag = "Box";                       // Tag to check for collision
    public bool debugMode = true;                           // Enable detailed debug logs
    public bool requireHeavyState = true;                   // Require the box to be in Heavy state

    [Header("Fracture Settings")]
    public float explosionForce = 100f;                      // Base explosion force when fracturing
    public float explosionForceMultiplier = 5.0f;          // Additional multiplier for more power
    public float explosionRadius = 20f;                     // Explosion radius
    public float upwardsModifier = 5.0f;                    // Upwards force modifier
    public bool disableAfterFracture = true;               // Disable this component after fracturing

    [Header("Chunk Physics Settings")]
    public bool disableChunkCollisions = true;             // Disable collision between chunks
    public float chunkLifetime = 2.0f;                     // Time before chunks disappear (seconds)
    public bool useRandomLifetime = false;                  // Use random lifetime for variety
    public float minLifetime = 2.0f;                       // Minimum lifetime if using random
    public float maxLifetime = 4.0f;                       // Maximum lifetime if using random
    public bool fadeOutChunks = true;                     // Fade chunks before destroying them
    public float fadeOutDuration = 1.0f;                   // Duration of fade out effect

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
            Debug.LogError("HeavyBoxTrigger: No FracturedObject component found on " + gameObject.name);
            return;
        }

        // Ensure the object starts in a stable state
        SetupInitialState();

        if (debugMode)
        {
            Debug.Log($"HeavyBoxTrigger initialized on {gameObject.name}");
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

        // Setup initial chunk physics
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

                // Set DontDeleteAfterBroken to false so we can control lifetime manually
                chunk.DontDeleteAfterBroken = true;

                if (debugMode) Debug.Log($"Initial setup for chunk: {chunk.name}");
            }
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

        Debug.Log($"FRACTURE TRIGGERED! Object: {collision.gameObject.name} collided with {gameObject.name}");

        // Mark as triggered
        hasTriggered = true;

        // Get collision point for explosion position
        Vector3 explosionPosition = collision.contacts.Length > 0 ?
            collision.contacts[0].point :
            transform.position;

        // Trigger the fracture with enhanced explosion
        TriggerEnhancedFracture(explosionPosition);
    }

    private bool IsValidTriggerCollision(Collision collision)
    {
        // Check if collision object has the required tag
        if (!collision.gameObject.CompareTag(triggerTag))
        {
            if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} doesn't have tag '{triggerTag}' (has '{collision.gameObject.tag}')");
            return false;
        }

        // If we require heavy state, check for it
        if (requireHeavyState)
        {
            GravState gravState = collision.gameObject.GetComponent<GravState>();
            if (gravState == null)
            {
                if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} has no GravState component");
                return false;
            }

            if (gravState.CurrentState != GravState.GravityState.Heavy)
            {
                if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} is not in Heavy state (current: {gravState.CurrentState})");
                return false;
            }

            if (debugMode) Debug.Log($"HeavyBoxTrigger: Object {collision.gameObject.name} is in Heavy state - criteria met!");
        }

        return true;
    }

    private void TriggerEnhancedFracture(Vector3 explosionPosition)
    {
        if (fracturedObject == null) return;

        if (debugMode) Debug.Log($"HeavyBoxTrigger: Triggering enhanced fracture at position {explosionPosition}");

        // First, disable the main object's collider and rigidbody
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        // Hide the original mesh BEFORE showing chunks
        HideOriginalMesh();

        // Disable single mesh visibility to show individual chunks
        fracturedObject.SetSingleMeshVisibility(false);

        // Setup chunk physics with enhanced settings
        SetupEnhancedChunkPhysics();

        // Enable chunk colliders
        EnableChunkColliders();

        // Apply enhanced explosion with delay
        StartCoroutine(DelayedEnhancedExplosion(explosionPosition, 0.1f));

        if (debugMode) Debug.Log($"HeavyBoxTrigger: Enhanced fracture setup completed for {gameObject.name}");
    }

    private void SetupEnhancedChunkPhysics()
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

                // Make chunks non-kinematic and enable gravity
                chunkRb.isKinematic = false;
                chunkRb.useGravity = true;

                // Increase drag slightly for more realistic movement
                chunkRb.linearDamping = 0.3f;
                chunkRb.angularDamping = 0.5f;

                // Set physics material if available
                if (fracturedObject.ChunkPhysicMaterial != null)
                {
                    Collider chunkCollider = chunk.GetComponent<Collider>();
                    if (chunkCollider != null)
                    {
                        chunkCollider.material = fracturedObject.ChunkPhysicMaterial;

                        // Disable collision between chunks if requested
                        if (disableChunkCollisions)
                        {
                            // Create a unique layer for chunk collisions or use existing physics settings
                            // Alternatively, we can disable chunk-to-chunk collisions in the explosion
                        }
                    }
                }

                // Set chunk to be destroyed after lifetime
                chunk.DontDeleteAfterBroken = true; // We'll handle destruction manually

                if (debugMode) Debug.Log($"Enhanced physics setup for chunk: {chunk.name}");
            }
        }

        // Compute masses for all chunks
        fracturedObject.ComputeChunksRelativeVolume();
        fracturedObject.ComputeChunksMass(fracturedObject.TotalMass);
    }

    private System.Collections.IEnumerator DelayedEnhancedExplosion(Vector3 explosionPosition, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (debugMode) Debug.Log($"HeavyBoxTrigger: Executing enhanced explosion at {explosionPosition}");

        // Calculate enhanced explosion force
        float finalExplosionForce = explosionForce * explosionForceMultiplier;

        // Apply explosion to each chunk individually for more control
        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null && chunk.GetComponent<Rigidbody>() != null)
            {
                Rigidbody chunkRb = chunk.GetComponent<Rigidbody>();

                // Detach chunk from the fractured object
                chunk.DetachFromObject(false);

                // Apply enhanced explosion force
                chunkRb.AddExplosionForce(finalExplosionForce, explosionPosition, explosionRadius, upwardsModifier);

                // Add some random angular velocity for more dynamic movement
                Vector3 randomTorque = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f)
                );
                chunkRb.AddTorque(randomTorque, ForceMode.Impulse);

                // Handle chunk collision settings
                if (disableChunkCollisions)
                {
                    StartCoroutine(DisableChunkCollisionsAfterDelay(chunk, 0.5f));
                }

                // Set up chunk lifetime
                StartCoroutine(HandleChunkLifetime(chunk));

                if (debugMode) Debug.Log($"Applied enhanced explosion to chunk: {chunk.name} with force: {finalExplosionForce}");
            }
        }

        // Optionally disable this component after fracturing
        if (disableAfterFracture)
        {
            this.enabled = false;
        }

        if (debugMode) Debug.Log($"HeavyBoxTrigger: Enhanced explosion completed for {gameObject.name}");
    }

    private System.Collections.IEnumerator DisableChunkCollisionsAfterDelay(FracturedChunk chunk, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (chunk != null && chunk.GetComponent<Collider>() != null)
        {
            Collider chunkCollider = chunk.GetComponent<Collider>();
            chunkCollider.isTrigger = true; // Make it a trigger to disable solid collisions

            if (debugMode) Debug.Log($"Disabled collisions for chunk: {chunk.name}");
        }
    }

    private System.Collections.IEnumerator HandleChunkLifetime(FracturedChunk chunk)
    {
        if (chunk == null) yield break;

        // Calculate lifetime
        float lifetime = chunkLifetime;
        if (useRandomLifetime)
        {
            lifetime = Random.Range(minLifetime, maxLifetime);
        }

        // Wait for the lifetime duration
        yield return new WaitForSeconds(lifetime);

        if (chunk != null && chunk.gameObject != null)
        {
            if (fadeOutChunks)
            {
                // Fade out the chunk
                yield return StartCoroutine(FadeOutChunk(chunk));
            }

            // Destroy the chunk
            if (debugMode) Debug.Log($"Destroying chunk: {chunk.name} after {lifetime} seconds");
            Destroy(chunk.gameObject);
        }
    }

    private System.Collections.IEnumerator FadeOutChunk(FracturedChunk chunk)
    {
        if (chunk == null || chunk.gameObject == null) yield break;

        Renderer chunkRenderer = chunk.GetComponent<Renderer>();
        if (chunkRenderer == null) yield break;

        Material[] materials = chunkRenderer.materials;
        Color[] originalColors = new Color[materials.Length];

        // Store original colors
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].HasProperty("_Color"))
            {
                originalColors[i] = materials[i].color;
            }
        }

        float fadeTimer = 0f;
        while (fadeTimer < fadeOutDuration)
        {
            if (chunk == null || chunk.gameObject == null) yield break;

            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);

            // Apply fade to all materials
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_Color"))
                {
                    Color newColor = originalColors[i];
                    newColor.a = alpha;
                    materials[i].color = newColor;
                }
            }

            yield return null;
        }
    }

    // ... (keep existing methods: HideOriginalMesh, IsFracturedChunkRenderer, DisableChunkColliders, EnableChunkColliders, SetChunkCollidersRecursive)

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
                collider.isTrigger = false; // Ensure it's not a trigger when enabled initially
            }

            if (debugMode && enable) Debug.Log($"Enabled collider on: {obj.name}");
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
            Debug.Log($"HeavyBoxTrigger: {gameObject.name} reset successfully");
        }
        else
        {
            Debug.LogWarning($"HeavyBoxTrigger: Failed to reset {gameObject.name}");
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
            Debug.Log("HeavyBoxTrigger: Manual trigger activated");
            hasTriggered = true;
            TriggerEnhancedFracture(transform.position);
        }
    }

    void OnValidate()
    {
        // Ensure explosion force is not negative
        if (explosionForce < 0) explosionForce = 0;
        if (explosionForceMultiplier < 0) explosionForceMultiplier = 0;
        if (explosionRadius < 0) explosionRadius = 0;
        if (chunkLifetime < 0) chunkLifetime = 0;
        if (minLifetime < 0) minLifetime = 0;
        if (maxLifetime < minLifetime) maxLifetime = minLifetime;
        if (fadeOutDuration < 0) fadeOutDuration = 0;
    }
}