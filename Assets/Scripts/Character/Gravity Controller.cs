using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

public class GravityController : MonoBehaviour
{
    private Rigidbody rb;

    public bool gravityFlipped = false;           // True = gravity up, False = gravity down
    public float rotationSpeed = 2.0f;            // Rotation smoothing speed

    private bool canFlipAgain = true;             // Prevent multiple flips in mid-air
    private bool justFlipped = false;
    private Vector3 expectedLandingNormal;

    [Header("Gravity Flip Settings")]
    public bool useSingleKeyToggle = false;       // Toggle this in the Inspector

    public MMF_Player PlayerFlipFeedback;

    [HideInInspector]
    public float CurrentZRotation => transform.eulerAngles.z;

    private GravityFlipFeedback feedbackSystem;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        feedbackSystem = GetComponent<GravityFlipFeedback>();

        // Add GravityFlipFeedback component if it doesn't exist
        if (feedbackSystem == null)
        {
            feedbackSystem = gameObject.AddComponent<GravityFlipFeedback>();
        }
    }

    private void Update()
    {
        if (!canFlipAgain || !CanFlipGravity())
            return;

        if (useSingleKeyToggle)
        {
            // Toggle gravity flip with only S/Down
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                bool flipUp = !gravityFlipped;

                if (flipUp && HasSpaceToFlip(true))
                {
                    FlipGravity(true);
                    //Debug.Log("Gravity flipped up (toggle mode).");
                }
                else if (!flipUp && HasSpaceToFlip(false))
                {
                    FlipGravity(false);
                    //Debug.Log("Gravity flipped down (toggle mode).");
                }
            }
        }
        else
        {
            // Traditional W/S controls
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) &&
                !gravityFlipped && HasSpaceToFlip(true))
            {
                FlipGravity(true);
                //Debug.Log("Gravity flipped up.");
            }

            if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) &&
                gravityFlipped && HasSpaceToFlip(false))
            {
                FlipGravity(false);
                //Debug.Log("Gravity flipped down.");
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
        justFlipped = true;
        expectedLandingNormal = flipUp ? Vector3.down : Vector3.up;

        Physics.gravity = gravityFlipped ? new Vector3(0, 20.0f, 0) : new Vector3(0, -20.0f, 0);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

        // Trigger code-based feedback
        if (feedbackSystem != null)
        {
            feedbackSystem.TriggerGravityFlipFeedback(flipUp);
        }

        float targetZRotation = gravityFlipped ? 180f : 0f;
        StartCoroutine(SmoothRotateZ(targetZRotation));
    }

    IEnumerator SmoothRotateZ(float targetZRotation)
    {
        float currentZRotation = transform.rotation.eulerAngles.z;

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

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y,
            targetZRotation
        );
    }

    bool CanFlipGravity()
    {
        return Mathf.Abs(rb.linearVelocity.y) < 1.0f;
    }

    public void ForceResetGravityDown()
    {
        gravityFlipped = false;
        Physics.gravity = new Vector3(0, -20f, 0);

        StopAllCoroutines();

        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);

        // Reset flip control variables to allow flipping again
        ResetFlipState();

        //Debug.Log("Forcefully reset gravity and rotation to normal.");
    }

    // NEW METHOD: Reset flip control state
    public void ResetFlipState()
    {
        canFlipAgain = true;
        justFlipped = false;
        expectedLandingNormal = Vector3.up;
        //Debug.Log("Flip state reset - player can flip again.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Must match the expected surface from the last flip
            if (justFlipped && Vector3.Dot(contact.normal, expectedLandingNormal) > 0.5f)
            {
                canFlipAgain = true;

                if (PlayerFlipFeedback != null)
                {
                    PlayerFlipFeedback.PlayFeedbacks();
                }

                justFlipped = false; // Reset
                break;
            }

            // Even if we didn't flip, still allow regular grounded movement
            if (!justFlipped &&
                ((!gravityFlipped && contact.normal.y > 0.5f) ||
                 (gravityFlipped && contact.normal.y < -0.5f)))
            {
                canFlipAgain = true;
            }
        }
    }
}