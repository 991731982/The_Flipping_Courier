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

            // ✅ 触发 eyeball 动画
            EyeballMovement eyeball = other.GetComponent<EyeballMovement>();
            if (eyeball != null)
            {
                eyeball.BeginSlither();
            }

            // ✅ 禁用玩家拖拽 + 关闭连接线
            DragObject drag = other.GetComponent<DragObject>();
            if (drag != null)
            {
                if (drag.connectorObject != null)
                {
                    drag.connectorObject.SetActive(false); // 关闭连线物体
                    Debug.Log("Connector object deactivated.");
                }

                drag.enabled = false; // 禁用拖拽功能
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
