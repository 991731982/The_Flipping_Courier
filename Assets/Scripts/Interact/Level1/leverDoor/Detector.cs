using UnityEngine;

public class Detector : MonoBehaviour
{
    public DoorLever doorLever;
    public string boxTag = "Box";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(boxTag))
        {
            doorLever.OpenDoor();
            Debug.Log("Box entered, door is opening!");

           
            EyeballMovement eyeball = other.GetComponent<EyeballMovement>();
            if (eyeball != null)
            {
                eyeball.BeginSlither();
            }

           
            DragObject drag = other.GetComponent<DragObject>();
            if (drag != null)
            {
                if (drag.connectorObject != null)
                {
                    drag.connectorObject.SetActive(false); 
                    Debug.Log("Connector object deactivated.");
                }

                drag.enabled = false; 
                Debug.Log("DragObject disabled.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(boxTag))
        {
            doorLever.CloseDoor();
            Debug.Log("Box exited, door is closing!");
        }
    }
}
