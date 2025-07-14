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

    private void OnCollisionEnter(Collision collision)
    {
        if (!canRegisterHit || !collision.gameObject.CompareTag("Weight"))
            return;

        // TEMP: Allow hits from any direction for testing
        currentHits++;
        Debug.Log("Box was hit by Weight. Current hits: " + currentHits);

        // Visually sink the box slightly to indicate impact
        transform.position -= new Vector3(0, fallAmount, 0);

        if (currentHits >= hitsToDestroy)
        {
            TriggerEyeballSlither();
            // SpawnSmallCube(); // Disabled for now
            DisableColliders();
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(HitCooldown());
        }
    }
    /*private void OnCollisionEnter(Collision collision)
{
    if (!canRegisterHit || !collision.gameObject.CompareTag("Weight"))
        return;

    bool validTopHit = false;

    foreach (ContactPoint contact in collision.contacts)
    {
        float dot = Vector3.Dot(contact.normal, transform.up);
        Debug.Log($"Contact normal: {contact.normal}, Dot: {dot}, Relative velocity Y: {collision.relativeVelocity.y}");

        // Confirm it's a top-down hit
        if (collision.relativeVelocity.y < 0 && dot > 0.5f)
        {
            validTopHit = true;
            break;
        }
    }

    if (validTopHit)
    {
        currentHits++;
        Debug.Log("Top hit registered! Current hits: " + currentHits);

        // Visually sink the box slightly to indicate damage
        transform.position -= new Vector3(0, fallAmount, 0);

        if (currentHits >= hitsToDestroy)
        {
            TriggerEyeballSlither();
            // SpawnSmallCube(); // Disabled for now
            DisableColliders();
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(HitCooldown());
        }
    }
    else
    {
        Debug.Log("Hit ignored — not from the top.");
    }
}*/
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

    /*
    private void SpawnSmallCube()
    {
        if (smallCubePrefab == null) return;

        Vector3 spawnPosition = transform.position + spawnOffset;
        Instantiate(smallCubePrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Small cube spawned!");
    }
    */

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