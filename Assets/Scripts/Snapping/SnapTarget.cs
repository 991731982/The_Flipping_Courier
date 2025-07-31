using UnityEngine;

public class SnapTarget : MonoBehaviour
{
    [Header("Snap Target Settings")]
    public string snapID = "";           // Optional ID for matching specific box colliders
    public bool showGizmo = true;        // Show visual indicator in scene
    public Color gizmoColor = Color.cyan;

    private Collider targetCollider;

    void Start()
    {
        targetCollider = GetComponent<Collider>();

        // Snap targets should be triggers
        if (targetCollider != null)
        {
            targetCollider.isTrigger = true;
        }
    }

    void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(transform.position, transform.localScale);

            // Draw a small indicator
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, 0.1f);
        }
    }
}