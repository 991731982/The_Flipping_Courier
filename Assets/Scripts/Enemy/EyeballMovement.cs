using UnityEngine;

public class EyeballMovement : MonoBehaviour
{
    [Header("Movement Targets")]
    public Transform startPoint;      // Inside the crate
    public Transform endPoint;        // Eye socket

    [Header("Slither Settings")]
    public float moveSpeed = 2f;
    public float slitherAmplitude = 0.2f;   // How far it bobs up & down
    public float slitherFrequency = 5f;     // How fast it bobs

    private bool isMoving = false;
    private float moveProgress = 0f;       
    private Vector3 pathDir;                // Normalized direction crate ? socket

    void Start()
    {
        // Start inside the crate
        if (startPoint != null) transform.position = startPoint.position;

        if (startPoint != null && endPoint != null)
            pathDir = (endPoint.position - startPoint.position).normalized;
    }

    void Update()
    {
        // press L to test the slither
        if (Input.GetKeyDown(KeyCode.L)) 
            BeginSlither();

        if (!isMoving || startPoint == null || endPoint == null) 
            return;

        // Advance along the straight path
        moveProgress += Time.deltaTime * moveSpeed;
        moveProgress = Mathf.Clamp01(moveProgress);

        Vector3 straightPos = Vector3.Lerp(startPoint.position, endPoint.position, moveProgress);

        // Add vertical bob (world?space Y) on top of forward motion
        float bobOffset = Mathf.Sin(Time.time * slitherFrequency) * slitherAmplitude;
        Vector3 slitherPos = straightPos + Vector3.up * bobOffset;

        transform.position = slitherPos;

        // Optional: face the direction of travel (purely cosmetic)
        //transform.forward = pathDir;

        // Stop when we’ve reached (or exceeded)
        if (moveProgress >= 1f)
        {
            transform.position = endPoint.position; // snap exactly
            isMoving = false;
        }
    }

    //External trigger – call when crate is destroyed.
    public void BeginSlither()
    {
        if (startPoint == null || endPoint == null) return;

        isMoving = true;
        moveProgress = 0f;
        pathDir = (endPoint.position - startPoint.position).normalized;
    }
}