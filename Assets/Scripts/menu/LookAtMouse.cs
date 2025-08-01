using UnityEngine;

public class EyeFollowMousePrecise : MonoBehaviour
{
    public float maxYawAngle = 30f;          // 最大偏转角
    public float smoothSpeed = 10f;          // 平滑程度
    public float angleOffset = -5f;          // ✅ 默认偏移角度，负数往左偏，正数往右偏

    private float currentAngle = 0f;

    void Update()
    {
        float mouseX01 = Mathf.Clamp01(Input.mousePosition.x / Screen.width);
        float mouseXNormalized = mouseX01 * 2f - 1f;

        // ✅ 映射 + 偏移角度
        float targetAngle = mouseXNormalized * maxYawAngle + angleOffset;

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
        transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}
