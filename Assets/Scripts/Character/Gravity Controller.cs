using System.Collections;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    private Rigidbody rb;
    public bool gravityFlipped = false; // True = gravity up, False = gravity down
    public float rotationSpeed = 2.0f; // Rotation smoothing speed

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) && !gravityFlipped && CanFlipGravity())
        {
            FlipGravity(true); // Flip up
            Debug.Log("Gravity flipped up.");
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) && gravityFlipped && CanFlipGravity())
        {
            FlipGravity(false); // Flip down
            Debug.Log("Gravity flipped down.");
        }
    }

    public void FlipGravity(bool flipUp)
    {
        gravityFlipped = flipUp;

        // Set gravity direction
        Physics.gravity = gravityFlipped ? new Vector3(0, 20.0f, 0) : new Vector3(0, -20.0f, 0);

        // Start smooth rotation
        float targetZRotation = gravityFlipped ? 180f : 0f;
        StartCoroutine(SmoothRotateZ(targetZRotation));
    }

    IEnumerator SmoothRotateZ(float targetZRotation)
    {
        float currentZRotation = transform.rotation.eulerAngles.z;

        // Adjust angles to prevent snap between 0 and 180
        if (currentZRotation > 180f && targetZRotation == 0f)
        {
            currentZRotation -= 360f;
        }
        else if (currentZRotation < 0f && targetZRotation == 180f)
        {
            targetZRotation -= 360f;
        }

        // Smoothly interpolate to target Z angle
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

        // Snap exactly to the target at the end
        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y,
            targetZRotation
        );
    }

    bool CanFlipGravity()
    {
        // Only flip if vertical velocity is near zero
        bool canFlip = Mathf.Abs(rb.linearVelocity.y) < 0.1f;
        Debug.Log("CanFlipGravity() = " + canFlip);
        return canFlip;
    }
}