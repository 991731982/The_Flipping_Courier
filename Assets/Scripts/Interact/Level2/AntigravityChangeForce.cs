using System.Diagnostics;
using UnityEngine;

public class AntigravityChangeForce : MonoBehaviour
{
    private Rigidbody rb;
    public GravityController gravityController;
    private GravState gravState;

    public float baseAntiGravityForce = 0.5f; // 反重力基礎強度

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (gravityController == null)
        {
            gravityController = FindObjectOfType<GravityController>();
        }

        gravState = GetComponent<GravState>();
    }

    void Update()
    {
        if (gravityController != null)
        {
            ApplyOppositeGravity();
        }
    }

    private void ApplyOppositeGravity()
    {
        if (rb == null) return;

        // 取得當前物體的質量
        float currentMass = rb.mass;

        // 計算新的反重力強度（質量越大，反重力越強）
        float modifiedAntiGravityForce = baseAntiGravityForce * (currentMass / 1.0f); // 1.0f 是原始質量基準

        // 禁用 Unity 內建重力
        rb.useGravity = false;

        // 設置速度為 0，防止物體隨機亂飄
        rb.linearVelocity = Vector3.zero;

        // 施加增強的反重力
        if (gravityController.gravityFlipped)
        {
            rb.AddForce(new Vector3(0, -modifiedAntiGravityForce, 0), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(new Vector3(0, modifiedAntiGravityForce, 0), ForceMode.Acceleration);
        }

       // UnityEngine.Debug.Log($"{gameObject.name} 當前質量: {currentMass}, 反重力強度: {modifiedAntiGravityForce}");
    }
}
