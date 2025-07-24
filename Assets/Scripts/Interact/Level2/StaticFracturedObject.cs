
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UltimateFracturing;
public class StaticFracturedObject : MonoBehaviour
{
    [Header("Fracture Settings")]
    public bool isTriggered = false;                    // Whether the object has been triggered to fracture
    public float explosionForce = 20f;                  // Force applied when exploding
    public bool disableAfterFracture = true;           // Disable this component after fracturing

    private FracturedObject fracturedObject;
    private Collider objectCollider;
    private Rigidbody objectRigidbody;

    void Start()
    {
        fracturedObject = GetComponent<FracturedObject>();
        objectCollider = GetComponent<Collider>();
        objectRigidbody = GetComponent<Rigidbody>();

        if (fracturedObject == null)
        {
            Debug.LogError("StaticFracturedObject: No FracturedObject component found!");
            return;
        }

        // Ensure the object starts static and unfractured
        SetupStaticState();
    }

    private void SetupStaticState()
    {
        // Make sure the main object is static
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        // Enable the main collider
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        // Disable all chunk colliders to prevent individual chunk interactions
        DisableChunkColliders();

        // Ensure single mesh visibility is enabled (unfractured appearance)
        fracturedObject.SetSingleMeshVisibility(true);

        // Remove CheckDynamicCollision component if it exists to prevent auto-fracturing
        CheckDynamicCollision dynamicCollision = GetComponent<CheckDynamicCollision>();
        if (dynamicCollision != null)
        {
            DestroyImmediate(dynamicCollision);
        }
    }

    /// <summary>
    /// Call this method to trigger the fracturing/explosion
    /// </summary>
    public void TriggerFracture(Vector3 explosionPosition)
    {
        if (isTriggered || fracturedObject == null)
        {
            return;
        }

        Debug.Log($"Triggering fracture for {gameObject.name}");

        isTriggered = true;

        // Disable the main object's collider
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        // Make rigidbody kinematic to prevent it from interfering
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        // Enable all chunk colliders
        EnableChunkColliders();

        // Trigger the explosion
        fracturedObject.Explode(explosionPosition, explosionForce);

        // Optionally disable this component after fracturing
        if (disableAfterFracture)
        {
            this.enabled = false;
        }
    }

    /// <summary>
    /// Overloaded method that uses the object's center as explosion position
    /// </summary>
    public void TriggerFracture()
    {
        TriggerFracture(transform.position);
    }

    private void DisableChunkColliders()
    {
        if (fracturedObject.ListFracturedChunks == null) return;

        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null)
            {
                EnableChunkCollidersRecursive(chunk.gameObject, false);
            }
        }
    }

    private void EnableChunkColliders()
    {
        if (fracturedObject.ListFracturedChunks == null) return;

        foreach (FracturedChunk chunk in fracturedObject.ListFracturedChunks)
        {
            if (chunk != null)
            {
                EnableChunkCollidersRecursive(chunk.gameObject, true);
            }
        }
    }

    private void EnableChunkCollidersRecursive(GameObject obj, bool enable)
    {
        // Enable/disable collider on current object
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = enable;
            if (enable)
            {
                collider.isTrigger = false; // Ensure it's not a trigger when enabled
            }
        }

        // Recursively enable/disable colliders on children
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            EnableChunkCollidersRecursive(obj.transform.GetChild(i).gameObject, enable);
        }
    }

    /// <summary>
    /// Reset the object to its unfractured state (if possible)
    /// </summary>
    public void ResetToStatic()
    {
        if (fracturedObject != null && fracturedObject.ResetChunks())
        {
            isTriggered = false;
            SetupStaticState();
            this.enabled = true;
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
}