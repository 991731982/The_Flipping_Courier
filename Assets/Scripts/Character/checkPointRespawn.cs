using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPointRespawn : MonoBehaviour
{
    private Vector3 checkpointPosition;
    private float storedZRotation = 0f;
    private bool wasGroundedAtCheckpoint = true;
    private bool wasGroundedOnDeath = true; // NEW: Track grounded state at time of death

    public Vector3 respawnOffset = new Vector3(0, 2, 0);
    private GravityController gravityController;
    private Rigidbody rb;
    private PlayerHealthDisplay playerHealth;
    private CubeCharacterController playerController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityController = GetComponent<GravityController>();
        playerHealth = GetComponent<PlayerHealthDisplay>();
        playerController = GetComponent<CubeCharacterController>();

        if (playerHealth == null)
            Debug.LogError("PlayerHealthDisplay component not found!");

        checkpointPosition = transform.position;
        storedZRotation = transform.eulerAngles.z;
        wasGroundedAtCheckpoint = true;
    }

    void Update()
    {
        // For testing: press R to respawn
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Record grounded state at time of death
            wasGroundedOnDeath = playerController != null && playerController.isGrounded;
            RespawnAtCheckpoint();
        }
    }

    public void SetCheckpoint(Vector3 newCheckpointPosition)
    {
        checkpointPosition = newCheckpointPosition;

        // If grounded, record Z rotation
        if (playerController != null && playerController.isGrounded)
        {
            storedZRotation = transform.eulerAngles.z;
            wasGroundedAtCheckpoint = true;
            Debug.Log($"Checkpoint set: Grounded, Z = {storedZRotation}");
        }
        else
        {
            storedZRotation = 0f;
            wasGroundedAtCheckpoint = false;
            Debug.Log($"Checkpoint set: In air, Z reset to 0");
        }
    }

    public void RespawnAtCheckpoint()
    {
        Vector3 respawnPosition = checkpointPosition + respawnOffset;
        transform.position = respawnPosition;
        rb.linearVelocity = Vector3.zero;

        float currentY = transform.eulerAngles.y;

        if (!wasGroundedOnDeath)
        {
            // Fully cancel flip and rotation
            gravityController.ForceResetGravityDown();
        }
        else
        {
            float targetZ = wasGroundedAtCheckpoint ? storedZRotation : 0f;
            transform.rotation = Quaternion.Euler(0f, currentY, targetZ);
        }

        if (playerHealth != null)
        {
            playerHealth.RestoreFullHealth();
        }

        Debug.Log($"Respawned at {respawnPosition}, GroundedOnDeath: {wasGroundedOnDeath}");
    }
}