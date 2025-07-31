using UnityEngine;

public class SnapColliderDetector : MonoBehaviour
{
    private BoxSnappingSystem snapSystem;
    private SnapPair snapPair;
    private Collider thisCollider;

    public void Initialize(BoxSnappingSystem system, SnapPair pair)
    {
        snapSystem = system;
        snapPair = pair;
        thisCollider = GetComponent<Collider>();

        // Make sure this collider is a trigger for detection
        thisCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the other collider is in our target list
        if (snapPair.targetColliders.Contains(other))
        {
            snapSystem.OnSnapColliderEnter(thisCollider, other, snapPair);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (snapPair.targetColliders.Contains(other))
        {
            snapSystem.OnSnapColliderExit(thisCollider, other);
        }
    }
}