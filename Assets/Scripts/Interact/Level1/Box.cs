using UnityEngine;
using System.Collections;
public class Box : MonoBehaviour
{
    [Header("Box Settings")]
    public int hitsToDestroy = 3;             // Number of hits needed to destroy the box
    public float fallAmount = 0.1f;           // How much the box drops per hit

    // [Header("Spawn Settings")]
    // public GameObject smallCubePrefab;      // Reward prefab
    // public Vector3 spawnOffset = new Vector3(0, 1, 0); // Offset for reward spawn

    [Header("Eyeball Settings")]
    public GameObject eyeball;                // Eyeball to activate when box is destroyed

    public int currentHits = 0;
    private bool canRegisterHit = true;

    private void Start()
    {
        // Ensure Rigidbody exists and is kinematic
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
    }

    // This will now be called by the BoxTopTrigger script
    public void BreakBox()
    {
        TriggerEyeballSlither();
        DisableColliders();
        Destroy(gameObject);
    }

    private void TriggerEyeballSlither()
    {
        if (eyeball == null) return;

        EyeballMovement mover = eyeball.GetComponent<EyeballMovement>();
        if (mover != null)
        {
            mover.BeginSlither();
            Debug.Log("Eyeball slither started!");
        }
    }

    private void DisableColliders()
    {
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
    }

    private IEnumerator HitCooldown()
    {
        canRegisterHit = false;
        yield return new WaitForSeconds(2f);
        canRegisterHit = true;
    }
}