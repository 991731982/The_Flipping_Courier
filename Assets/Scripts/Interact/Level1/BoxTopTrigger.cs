using UnityEngine;

public class BoxTopTrigger : MonoBehaviour
{
    public Box boxParent;             // Reference to main Box script
    public int requiredHits = 3;
    private int currentHits = 0;
    private bool canRegisterHit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canRegisterHit || other.tag != "Weight" || boxParent == null)
            return;

        currentHits++;
        Debug.Log("Top Trigger hit! Current: " + currentHits);

        if (currentHits >= requiredHits)
        {
            boxParent.BreakBox();
            gameObject.SetActive(false); // Disable the top trigger
        }
        else
        {
            StartCoroutine(HitCooldown());
        }
    }

    private System.Collections.IEnumerator HitCooldown()
    {
        canRegisterHit = false;
        yield return new WaitForSeconds(1.5f);
        canRegisterHit = true;
    }
}
