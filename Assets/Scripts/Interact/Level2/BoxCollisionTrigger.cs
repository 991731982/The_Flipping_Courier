using UnityEngine;

public class BoxCollisionTrigger : MonoBehaviour
{
    public GameObject cubeToMove; // 要移动的 Cube
    public float moveDistance = 2f; // 向上移动距离
    public float moveSpeed = 2f;    // 移动速度

    [Header("Eyeball Settings")]
    public GameObject eyeball;  // 👈 要触发移动的 Eyeball 对象

    private bool shouldMove = false;
    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        if (cubeToMove != null)
        {
            startPos = cubeToMove.transform.position;
            targetPos = startPos + Vector3.up * moveDistance;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger activated by: " + other.name);

        if (other.CompareTag("Box") && cubeToMove != null)
        {
            shouldMove = true;
            Debug.Log("Box trigger confirmed, moving cube");

            // ✅ 只增加：触发 eyeball 的移动
            if (eyeball != null)
            {
                EyeballMovement mover = eyeball.GetComponent<EyeballMovement>();
                if (mover != null)
                {
                    mover.BeginSlither();
                    Debug.Log("Eyeball slither started from trigger!");
                }
                else
                {
                    Debug.LogWarning("EyeballMovement component not found on eyeball GameObject.");
                }
            }
            else
            {
                Debug.LogWarning("Eyeball GameObject not assigned in inspector.");
            }

            // 可选：禁用拖拽
            DragObject drag = cubeToMove.GetComponent<DragObject>();
            if (drag != null)
            {
                drag.enabled = false;
            }
        }
    }

    void Update()
    {
        if (shouldMove && cubeToMove != null)
        {
            cubeToMove.transform.position = Vector3.MoveTowards(
                cubeToMove.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(cubeToMove.transform.position, targetPos) < 0.01f)
            {
                shouldMove = false;
            }
        }
    }
}
