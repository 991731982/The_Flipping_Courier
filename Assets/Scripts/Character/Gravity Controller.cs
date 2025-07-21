using System.Collections;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    private Rigidbody rb;
    public bool gravityFlipped = false;           // True = gravity up, False = gravity down
    public float rotationSpeed = 2.0f;            // Rotation smoothing speed

    private bool canFlipAgain = true;             // Prevent multiple flips in mid-air

    [HideInInspector]
    public float CurrentZRotation => transform.eulerAngles.z;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (canFlipAgain)
        {
            // Flip upward (to ceiling)
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) &&
    !gravityFlipped && CanFlipGravity() && HasSpaceToFlip(true))
            {
                FlipGravity(true);
                Debug.Log("Gravity flipped up.");
            }

            // Flip downward (to floor)
            if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && gravityFlipped && CanFlipGravity())
            {
                FlipGravity(false);
                Debug.Log("Gravity flipped down.");
            }
        }
    }
    bool HasSpaceToFlip(bool flipUp)
    {
        float checkDistance = 1.0f; // adjust based on player height
        Vector3 direction = flipUp ? Vector3.up : Vector3.down;
        return !Physics.Raycast(transform.position, direction, checkDistance, LayerMask.GetMask("Ground"));
    }
    public void FlipGravity(bool flipUp)
    {
        gravityFlipped = flipUp;
        canFlipAgain = false;

        // Set new gravity direction
        Physics.gravity = gravityFlipped ? new Vector3(0, 20.0f, 0) : new Vector3(0, -20.0f, 0);

        // Preserve existing velocity exactly
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

        // Start smooth rotation coroutine
        float targetZRotation = gravityFlipped ? 180f : 0f;
        StartCoroutine(SmoothRotateZ(targetZRotation));
    }


    IEnumerator SmoothRotateZ(float targetZRotation)
    {
        float currentZRotation = transform.rotation.eulerAngles.z;

        // Angle correction to avoid snapping
        if (currentZRotation > 180f && targetZRotation == 0f)
            currentZRotation -= 360f;
        else if (currentZRotation < 0f && targetZRotation == 180f)
            targetZRotation -= 360f;

        while (Mathf.Abs(currentZRotation - targetZRotation) > 0.1f)
        {
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                currentZRotation
            );
            yield return null;
        }

        // Snap to final angle
        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y,
            targetZRotation
        );
    }

    bool CanFlipGravity()
    {
        // Only flip if vertical velocity is low
        return Mathf.Abs(rb.linearVelocity.y) < 1.0f;
    }
    public void ForceResetGravityDown()
    {
        gravityFlipped = false;
        Physics.gravity = new Vector3(0, -20f, 0);

        // Cancel any active flip rotation coroutine
        StopAllCoroutines();

        // Force Z rotation to 0 immediately
        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);

        Debug.Log("Forcefully reset gravity and rotation to normal.");
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Confirm the player landed on the floor or ceiling before enabling flip again
        foreach (ContactPoint contact in collision.contacts)
        {
            if ((!gravityFlipped && contact.normal.y > 0.5f) ||
                (gravityFlipped && contact.normal.y < -0.5f))
            {
                canFlipAgain = true;
                Debug.Log("Player landed — flip re-enabled.");
                break;
            }
        }
    }
}