using UnityEngine;

public class PanelTriggerButton : MonoBehaviour
{
    public UIPanelManager panelManager;

    public void Trigger()
    {
        panelManager.StartStoryboard();
    }
}
