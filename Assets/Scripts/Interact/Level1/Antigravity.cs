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
    // Start is called before the first frame update
    void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();
        // If no gravity controller is assigned in the inspector, try to find one in the scene
        if (gravityController == null)
        {
            gravityController = GravityController.FindFirstObjectByType<GravityController>();
        }
    }
    // Update is called once per frame
    void Update()
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
        // Reset any existing velocity to prevent unwanted movement
        rb.linearVelocity = Vector3.zero;
        // If gravity is flipped, apply a downward force; otherwise, apply an upward force
        if (gravityController.gravityFlipped)
        {
            rb.AddForce(new Vector3(0, -90.0f, 0), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(new Vector3(0, 90.0f, 0), ForceMode.Acceleration);
        }
    }
}