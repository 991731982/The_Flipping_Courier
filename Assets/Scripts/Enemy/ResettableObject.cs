using UnityEngine;

public class ResettableObject : MonoBehaviour
{
    [Header("Reset Settings")]
    [SerializeField] private bool resetOnStart = true;
    [SerializeField] private bool excludeEnemies = true;
    [SerializeField] private string[] excludedTags = { "Enemy" };

    [Header("What to Reset")]
    [SerializeField] private bool resetPosition = true;
    [SerializeField] private bool resetRotation = true;
    [SerializeField] private bool resetScale = false;
    [SerializeField] private bool resetRigidbody = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Rigidbody rb;
    private bool isInitialized = false;

    void Start()
    {
        if (resetOnStart)
        {
            InitializeOriginalValues();
        }
    }

    public void InitializeOriginalValues()
    {
        // Store original transform values
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // Get rigidbody component if it exists
        rb = GetComponent<Rigidbody>();

        isInitialized = true;
    }

    public void ResetPosition()
    {
        ResetToOriginal();
    }

    public void ResetToOriginal()
    {
        // Check if we should skip this object
        if (ShouldSkipReset())
        {
            return;
        }

        // Initialize if not already done
        if (!isInitialized)
        {
            InitializeOriginalValues();
        }

        // Reset transform properties
        if (resetPosition)
            transform.position = originalPosition;

        if (resetRotation)
            transform.rotation = originalRotation;

        if (resetScale)
            transform.localScale = originalScale;

        // Reset rigidbody properties
        if (resetRigidbody && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Optional: wake up the rigidbody if it was sleeping
            if (rb.IsSleeping())
                rb.WakeUp();
        }

        // Trigger reset event for other components that might need it
        OnObjectReset();
    }

    private bool ShouldSkipReset()
    {
        if (!excludeEnemies) return false;

        // Check against all excluded tags
        foreach (string tag in excludedTags)
        {
            if (CompareTag(tag))
                return true;
        }

        return false;
    }

    // Virtual method that can be overridden by derived classes
    protected virtual void OnObjectReset()
    {
        // Override this in derived classes for custom reset behavior
        // Example: reset health, ammunition, special effects, etc.
    }

    // Public method to manually set new "original" values
    public void SetNewOriginalPosition()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    // Utility methods for external scripts
    public Vector3 GetOriginalPosition() => originalPosition;
    public Quaternion GetOriginalRotation() => originalRotation;
    public Vector3 GetOriginalScale() => originalScale;

    // Editor helper - allows testing reset in play mode
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void EditorResetToOriginal()
    {
        if (Application.isPlaying)
        {
            ResetToOriginal();
        }
    }
}