using UnityEngine;

public class RotateWithGravity : MonoBehaviour
{
    public GravityController playerGravityController; // 拖入玩家对象
    public Transform objectToRotate; // 拖入要旋转的物体（通常是子物体）
    public float rotationSpeed = 5f;

    private float targetRotationZ;

    void Update()
    {
        if (playerGravityController == null || objectToRotate == null)
            return;

        // 目标角度：正着是0，反转是180
        targetRotationZ = playerGravityController.gravityFlipped ? 180f : 0f;

        // 平滑插值旋转
        float currentZ = objectToRotate.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float newZ = Mathf.Lerp(currentZ, targetRotationZ, Time.deltaTime * rotationSpeed);
        objectToRotate.localEulerAngles = new Vector3(
            objectToRotate.localEulerAngles.x,
            objectToRotate.localEulerAngles.y,
            newZ
        );
    }
}
