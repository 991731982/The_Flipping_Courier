using System.Collections.Generic;
using UnityEngine;

public class EyeballPanelTriggerZone : MonoBehaviour
{
    public UIPanelManager panelManager;

    [Tooltip("Panel sequence to play when this trigger is entered")]
    public List<UIPanelManager.PanelData> panelSequence;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Box"))
        {
            if (panelManager != null)
            {
                panelManager.StartStoryboard(panelSequence);
            }

            hasTriggered = true;
            Destroy(gameObject); // only trigger once
        }
    }
}