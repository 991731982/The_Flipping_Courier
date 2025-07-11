/*using UnityEngine;

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
        lockedPosition = transform.position;

        // 停止刚体运动
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 关闭物理影响
        rb.useGravity = false;
        rb.isKinematic = true;


    }

    void Update()
    {
        if (isLocked)
        {
            // 强制每帧锁住位置，防止任何代码篡改
            transform.position = lockedPosition;
        }
    }
}
*/