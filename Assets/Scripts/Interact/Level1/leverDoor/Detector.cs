using UnityEngine;

public class Detector : MonoBehaviour
{
    public DoorLever doorLever; // DoorLever ½Å±¾µÄÒýÓÃ
    public string boxTag = "Box"; // ÓÃÓÚ±êÊ¶Ïä×ÓµÄ±êÇ©

    // µ±Ïä×Ó½øÈë´¥·¢ÇøÓòÊ±
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(boxTag))
        {
            doorLever.OpenDoor(); // 打开门
            Debug.Log("Box entered, door is opening!");

            // 触发 EyeballMovement
            EyeballMovement eyeball = other.GetComponent<EyeballMovement>();
            if (eyeball != null)
            {
                eyeball.BeginSlither();
            }

            // 禁用玩家拖动控制
            DragObject drag = other.GetComponent<DragObject>();
            if (drag != null)
            {
                drag.enabled = false;
            }
        }
    }


    // µ±Ïä×ÓÀë¿ª´¥·¢ÇøÓòÊ±
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(boxTag))
        {
            doorLever.CloseDoor(); // ¹Ø±ÕÃÅ
            Debug.Log("Box exited, door is closing!");
        }
    }
}
