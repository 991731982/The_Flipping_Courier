using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;
    public float rotationSpeed = 10f; // Increased for more responsiveness
    public float maxZRotation = 45f; // Degrees (left and right)

    [Header("Angle Configuration")]
    public float centerAngle = 180f; // Angle when player is directly in front
    public float leftAngle = 225f;   // Angle when player is to the left
    public float rightAngle = 135f;  // Angle when player is to the right

    private float baseRotationY = 0f;
    private float baseRotationX = -90f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player not found! Tag your player as 'Player'");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;

        // Convert to local space to work with the object's orientation
        Vector3 localDirection = transform.InverseTransformDirection(directionToPlayer);

        // For Z-rotation, we want to look at the X-Z plane (horizontal plane)
        // This will give us the left-right angle
        float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;

        // Clamp the angle to our desired range
        float clampedAngle = Mathf.Clamp(horizontalAngle, -maxZRotation, maxZRotation);

        // Map the clamped angle to our desired Z rotation
        // When angle is -maxZRotation (left), use leftAngle
        // When angle is 0 (center), use centerAngle  
        // When angle is +maxZRotation (right), use rightAngle
        float normalizedAngle = clampedAngle / maxZRotation; // -1 to 1
        float targetZRotation;

        if (normalizedAngle <= 0) // Left side
        {
            targetZRotation = Mathf.Lerp(centerAngle, leftAngle, -normalizedAngle);
        }
        else // Right side
        {
            targetZRotation = Mathf.Lerp(centerAngle, rightAngle, normalizedAngle);
        }

        // Apply the rotation
        Quaternion targetLocalRotation = Quaternion.Euler(baseRotationX, baseRotationY, targetZRotation);

        // Smoothly rotate with increased speed
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRotation, Time.deltaTime * rotationSpeed);
    }

    // Debug helper - call this to see current angles
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Draw line to player
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);

            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}