using System.Collections;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    private Rigidbody rb;
    public bool gravityFlipped = false;
    public float rotationSpeed = 2.0f; // 旋转过渡速度

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (CanFlipGravity())
            {
                FlipGravity();
                Debug.Log("Gravity flipped successfully.");
            }
        }
    }

    public void FlipGravity()
    {
        // 反转重力状态
        gravityFlipped = !gravityFlipped;

        // 更新重力方向
        Physics.gravity = gravityFlipped ? new Vector3(0, 20.0f, 0) : new Vector3(0, -20.0f, 0);

        // 启动旋转过渡动画
        float targetZRotation = gravityFlipped ? 180f : 0f;
        StartCoroutine(SmoothRotateZ(targetZRotation));
    }

    IEnumerator SmoothRotateZ(float targetZRotation)
    {
        float currentZRotation = transform.rotation.eulerAngles.z;

        // 修正角度以避免180度跳变
        if (currentZRotation > 180f && targetZRotation == 0f)
        {
            currentZRotation -= 360f;
        }
        else if (currentZRotation < 0f && targetZRotation == 180f)
        {
            targetZRotation -= 360f;
        }

        // 平滑插值到目标角度，仅修改 Z 轴
        while (Mathf.Abs(currentZRotation - targetZRotation) > 0.1f)
        {
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentZRotation);
            yield return null;
        }

        // 强制校准，避免误差
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetZRotation);
    }


    bool CanFlipGravity()
    {
        // 只有在竖直速度很小（接近静止）时才能反转重力
        bool canFlip = Mathf.Abs(rb.linearVelocity.y) < 0.1f;
        Debug.Log("CanFlipGravity() = " + canFlip);
        return canFlip;
    }
}
