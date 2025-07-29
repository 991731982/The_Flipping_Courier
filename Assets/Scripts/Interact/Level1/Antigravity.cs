using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the anti-gravity behavior of a GameObject based on a gravity controller's state.
public class Antigravity : MonoBehaviour
{
    // Rigidbody component of the GameObject
    private Rigidbody rb;
    // Reference to the GravityController script, which manages the gravity state
    public GravityController gravityController;

    [Header("Antigravity Settings")]
    [SerializeField] private float antigravityForce = 20.0f; // Reduced from 90.0f
    [SerializeField] private float maxSpeed = 10.0f; // Maximum antigravity speed
    [SerializeField] private bool preserveHorizontalMovement = true; // Keep horizontal velocity

    // Track previous gravity state to detect changes
    private bool previousGravityState;
    private Vector3 storedHorizontalVelocity;

    void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();

        // If no gravity controller is assigned in the inspector, try to find one in the scene
        if (gravityController == null)
        {
            gravityController = GravityController.FindFirstObjectByType<GravityController>();
        }

        // Initialize previous state
        if (gravityController != null)
        {
            previousGravityState = gravityController.gravityFlipped;
        }
    }

    void FixedUpdate() // Use FixedUpdate for physics
    {
        // Check if the gravity controller exists, then apply the opposite gravity effect
        if (gravityController != null)
        {
            ApplyOppositeGravity();
        }
    }

    // Applies an anti-gravity force based on the gravity controller's state
    private void ApplyOppositeGravity()
    {
        // Disable Unity's built-in gravity
        rb.useGravity = false;

        // Store horizontal velocity if we want to preserve it
        if (preserveHorizontalMovement)
        {
            storedHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }

        // Detect gravity state change
        if (previousGravityState != gravityController.gravityFlipped)
        {
            // Gravity just changed, reset vertical velocity for smoother transition
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            previousGravityState = gravityController.gravityFlipped;
        }

        // Calculate desired force direction
        Vector3 forceDirection;
        float currentVerticalSpeed = rb.linearVelocity.y;

        if (gravityController.gravityFlipped)
        {
            // Gravity is up, so apply downward antigravity
            forceDirection = Vector3.down;

            // Only apply force if we haven't reached max speed downward
            if (currentVerticalSpeed > -maxSpeed)
            {
                rb.AddForce(forceDirection * antigravityForce, ForceMode.Acceleration);
            }
        }
        else
        {
            // Gravity is down, so apply upward antigravity
            forceDirection = Vector3.up;

            // Only apply force if we haven't reached max speed upward
            if (currentVerticalSpeed < maxSpeed)
            {
                rb.AddForce(forceDirection * antigravityForce, ForceMode.Acceleration);
            }
        }

        // Restore horizontal movement if enabled
        if (preserveHorizontalMovement && storedHorizontalVelocity != Vector3.zero)
        {
            Vector3 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(storedHorizontalVelocity.x, currentVelocity.y, storedHorizontalVelocity.z);
        }
    }

    // Optional: Method to reset the platform's state
    public void ResetAntigravity()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
        }
    }

    // Optional: Method to temporarily disable antigravity
    public void SetAntigravityEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled && rb != null)
        {
            rb.useGravity = true;
        }
    }
}