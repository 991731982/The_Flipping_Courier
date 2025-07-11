using UnityEngine;

public class LockOnCollision : MonoBehaviour
{
    private Rigidbody rb;
    private bool isLocked = false;
    private Vector3 lockedPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isLocked && collision.collider.CompareTag("StopZone"))
        {
            LockObject();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLocked && other.CompareTag("StopZone"))
        {
            LockObject();
        }
    }

    void LockObject()
    {
        isLocked = true;

        // 记住当前锁定位置
        lockedPosition = transform.position;

        // 停止物理行为
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        // 可选：禁用其他控制脚本，比如拖拽等
        // GetComponent<YourDragScript>()?.enabled = false;
    }

    void LateUpdate()
    {
        if (isLocked && transform.position != lockedPosition)
        {
            transform.position = lockedPosition;
        }
    }
}
