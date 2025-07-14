using UnityEngine;

public class EyeballMovement : MonoBehaviour
{
    [Header("Movement Targets")]
    public Transform startPoint;          // Inside the crate
    public Transform endPoint;            // Eye socket

    [Header("Slither Settings")]
    public float moveSpeed = 0.01f;           // Linear travel speed
    public float slitherAmplitude = 0.01f;    // Bob size (Y axis)
    public float slitherFrequency = 5f;       // Bob speed
    public float stopThreshold = 0.02f;       // How close is "close enough" (in meters)

    private bool isMoving = false;
    private float moveProgress = 0f;          // From 0 to 1
    private Vector3 pathDir;                  // Normalized direction from start to end

    private void Start()
    {
        // Start at the hidden position inside the crate
        if (startPoint != null)
            transform.position = startPoint.position;

        if (startPoint != null && endPoint != null)
            pathDir = (endPoint.position - startPoint.position).normalized;
    }

    private void Update()
    {
        if (!isMoving || startPoint == null || endPoint == null)
            return;

        // Progress along the straight line
        moveProgress += Time.deltaTime * moveSpeed;
        moveProgress = Mathf.Clamp01(moveProgress);

        Vector3 straightPos = Vector3.Lerp(startPoint.position, endPoint.position, moveProgress);

        // Add vertical bobbing motion
        float bobOffset = Mathf.Sin(Time.time * slitherFrequency) * slitherAmplitude;
        Vector3 slitherPos = straightPos + Vector3.up * bobOffset;

        transform.position = slitherPos;

        // Optional: Face travel direction
        // transform.forward = pathDir;

        // Stop if close enough or reached the end
        if (moveProgress >= 1f || Vector3.Distance(transform.position, endPoint.position) <= stopThreshold)
        {
            transform.position = endPoint.position;
            isMoving = false;
        }
    }

    // Call this externally (e.g., from Box script) to start the slither
    public void BeginSlither()
    {
        if (startPoint == null || endPoint == null)
            return;

        isMoving = true;
        moveProgress = 0f;
        pathDir = (endPoint.position - startPoint.position).normalized;
    }
}