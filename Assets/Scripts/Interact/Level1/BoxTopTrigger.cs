using UnityEngine;

public class BoxTopTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Box boxParent;             // Reference to main Box script
    public int requiredHits = 3;      // Number of hits required to break the box
    public string weightTag = "Weight"; // Tag to check for collision
    public bool debugMode = true;     // Enable debug logs

    [Header("Hit Effects")]
    public bool enableBoxDrop = true; // Enable box dropping on hit
    public float dropAmount = 0.1f;   // How much to drop the box per hit

    private int currentHits = 0;
    private bool canRegisterHit = true;

    private void Start()
    {
        if (boxParent == null)
        {
            boxParent = GetComponentInParent<Box>();
            if (boxParent == null)
            {
                Debug.LogError("BoxTopTrigger: No Box component found in parent!");
            }
        }

        if (debugMode)
        {
            Debug.Log($"BoxTopTrigger initialized. Required hits: {requiredHits}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canRegisterHit || !other.CompareTag(weightTag) || boxParent == null)
        {
            if (debugMode && other.CompareTag(weightTag))
            {
                Debug.Log($"BoxTopTrigger: Hit ignored - canRegister: {canRegisterHit}, hasBoxParent: {boxParent != null}");
            }
            return;
        }

        currentHits++;

        if (debugMode)
        {
            Debug.Log($"BoxTopTrigger hit by {other.name}! Current hits: {currentHits}/{requiredHits}");
        }

        // Apply hit effects
        if (enableBoxDrop && boxParent != null)
        {
            ApplyBoxDrop();
        }

        if (currentHits >= requiredHits)
        {
            if (debugMode)
            {
                Debug.Log($"BoxTopTrigger: Required hits reached! Breaking box...");
            }

            // Break the box (this will trigger the fracture explosion)
            boxParent.BreakBox();

            // Disable this trigger
            gameObject.SetActive(false);
        }
        else
        {
            // Start cooldown for next hit
            StartCoroutine(HitCooldown());
        }
    }

    private void ApplyBoxDrop()
    {
        if (boxParent != null)
        {
            Vector3 currentPos = boxParent.transform.position;
            boxParent.transform.position = new Vector3(currentPos.x, currentPos.y - dropAmount, currentPos.z);

            if (debugMode)
            {
                Debug.Log($"Dropped box by {dropAmount} units");
            }
        }
    }

    private System.Collections.IEnumerator HitCooldown()
    {
        canRegisterHit = false;
        yield return new WaitForSeconds(1.5f);
        canRegisterHit = true;

        if (debugMode)
        {
            Debug.Log("BoxTopTrigger: Hit cooldown ended, ready for next hit");
        }
    }

}
