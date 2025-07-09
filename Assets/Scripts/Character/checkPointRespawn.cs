using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPointRespawn : MonoBehaviour
{
    private Vector3 checkpointPosition;  // Variable to store the checkpoint position
    public Vector3 respawnOffset = new Vector3(0, 2, 0);  // The offset for respawn position
    private GravityController gravityController;
    private Rigidbody rb;
    private PlayerHealthDisplay playerHealth;  // Reference to the player's health management

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityController = GetComponent<GravityController>();
        playerHealth = GetComponent<PlayerHealthDisplay>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthDisplay component not found on player!");
        }

        // Set the initial checkpoint to the player's starting position
        checkpointPosition = transform.position;
        Debug.Log("Starting position set as checkpoint: " + checkpointPosition);
    }

    void Update()
    {
        // Restart or respawn at checkpoint when "R" is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnAtCheckpoint();
        }
    }

    // Set the checkpoint position when the player reaches it
    public void SetCheckpoint(Vector3 newCheckpointPosition)
    {
        checkpointPosition = newCheckpointPosition;
        Debug.Log("Checkpoint position set to: " + checkpointPosition);
    }

    // Respawn the player at the last checkpoint with an offset
    public void RespawnAtCheckpoint()
    {
        Debug.Log("Respawning at checkpoint...");
        // Add the respawnOffset to the checkpointPosition
        Vector3 respawnPosition = checkpointPosition + respawnOffset;

        // Set the player's position to the new respawn position with the offset
        transform.position = respawnPosition;
        rb.linearVelocity = Vector3.zero; // Reset the player's velocity
        gravityController.gravityFlipped = false;  // Set gravity state back to normal
        Physics.gravity = new Vector3(0, -20f, 0);  // Set gravity to default direction (downward)

        // Restore player's health to maximum
        if (playerHealth != null)
        {
            playerHealth.RestoreFullHealth();
        }

        Debug.Log("Gravity has been reset to normal, and health restored to max.");
    }
}
