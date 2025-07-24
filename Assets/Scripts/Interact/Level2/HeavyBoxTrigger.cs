using UnityEngine;

public class HeavyBoxTrigger : MonoBehaviour
{
    public float massThreshold = 400f;            // 触发爆炸的质量阈值
    public float dropDistance = 10f;              // 向下移动的距离
    public float dropSpeed = 5f;                  // 向下移动速度
    public FracturedObject fracturedObject;       // 要爆炸的FracturedObject对象

    private bool shouldDrop = false;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position + Vector3.down * dropDistance;
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;

        if (collision.gameObject.CompareTag("Box") && rb != null && rb.mass >= massThreshold)
        {
            Debug.Log("重物 Box 碰撞触发！");
            shouldDrop = true;

            if (fracturedObject != null)
            {
                Vector3 explosionPosition = transform.position;
                fracturedObject.Explode(explosionPosition, 20f); // 触发爆炸，20为爆炸力大小
            }
        }
    }

    void Update()
    {
        if (shouldDrop)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, dropSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                shouldDrop = false;
            }
        }
    }
}
