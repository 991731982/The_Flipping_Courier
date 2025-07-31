
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SnapPair
{
    [Header("Snap Configuration")]
    public Collider boxCollider;          // The collider on this box
    public List<Collider> targetColliders = new List<Collider>(); // Which target colliders it can snap to
    public string snapID = "";            // Optional ID for matching (e.g. "top", "bottom", "side1")
}

public class BoxSnappingSystem : MonoBehaviour
{
    [Header("Snap Pairs Configuration")]
    public List<SnapPair> snapPairs = new List<SnapPair>();

    [Header("Snap Settings")]
    public float snapDistance = 2f;       // Distance to start snapping
    public float snapSpeed = 5f;          // How fast to snap
    public bool smoothSnapping = true;    // Smooth vs instant snap

    private Rigidbody rb;
    private bool isSnapping = false;
    private bool isSnapped = false;
    private Vector3 targetSnapPosition;
    private Quaternion targetSnapRotation;

    // Track which colliders are in range
    private Dictionary<Collider, Collider> collidersInRange = new Dictionary<Collider, Collider>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetupSnapColliders();
    }

    void SetupSnapColliders()
    {
        // Make sure all box colliders are set as triggers for detection
        foreach (var snapPair in snapPairs)
        {
            if (snapPair.boxCollider != null)
            {
                // Add SnapColliderDetector component to each box collider
                var detector = snapPair.boxCollider.gameObject.GetComponent<SnapColliderDetector>();
                if (detector == null)
                {
                    detector = snapPair.boxCollider.gameObject.AddComponent<SnapColliderDetector>();
                }
                detector.Initialize(this, snapPair);
            }
        }
    }

    public void OnSnapColliderEnter(Collider boxCollider, Collider targetCollider, SnapPair snapPair)
    {
        // Check if this target collider is valid for this box collider
        if (snapPair.targetColliders.Contains(targetCollider))
        {
            float distance = Vector3.Distance(boxCollider.transform.position, targetCollider.transform.position);

            if (distance <= snapDistance && !isSnapped)
            {
                collidersInRange[boxCollider] = targetCollider;
                TrySnap(boxCollider, targetCollider);
            }
        }
    }

    public void OnSnapColliderExit(Collider boxCollider, Collider targetCollider)
    {
        if (collidersInRange.ContainsKey(boxCollider))
        {
            collidersInRange.Remove(boxCollider);
        }
    }

    void TrySnap(Collider boxCollider, Collider targetCollider)
    {
        if (isSnapping || isSnapped) return;

        // Calculate snap position (align the colliders)
        Vector3 offset = boxCollider.transform.position - transform.position;
        targetSnapPosition = targetCollider.transform.position - offset;
        targetSnapRotation = targetCollider.transform.rotation;

        if (smoothSnapping)
        {
            isSnapping = true;
            // Disable physics during smooth snap
            rb.isKinematic = true;
        }
        else
        {
            // Instant snap
            transform.position = targetSnapPosition;
            transform.rotation = targetSnapRotation;
            SnapComplete();
        }
    }

    void Update()
    {
        if (isSnapping && smoothSnapping)
        {
            // Smooth interpolation to snap position
            transform.position = Vector3.Lerp(transform.position, targetSnapPosition, snapSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetSnapRotation, snapSpeed * Time.deltaTime);

            // Check if close enough to complete snap
            if (Vector3.Distance(transform.position, targetSnapPosition) < 0.01f)
            {
                transform.position = targetSnapPosition;
                transform.rotation = targetSnapRotation;
                SnapComplete();
            }
        }
    }

    void SnapComplete()
    {
        isSnapping = false;
        isSnapped = true;

        // Lock the object in place
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("Box snapped successfully!");
    }

    public void ReleaseSnap()
    {
        isSnapped = false;
        isSnapping = false;
        rb.isKinematic = false;
        collidersInRange.Clear();
    }

    void OnDrawGizmosSelected()
    {
        // Visualize snap pairs
        foreach (var snapPair in snapPairs)
        {
            if (snapPair.boxCollider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(snapPair.boxCollider.transform.position, snapDistance);

                foreach (var targetCollider in snapPair.targetColliders)
                {
                    if (targetCollider != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(snapPair.boxCollider.transform.position, targetCollider.transform.position);
                        Gizmos.DrawWireCube(targetCollider.transform.position, targetCollider.bounds.size);
                    }
                }
            }
        }
    }
}