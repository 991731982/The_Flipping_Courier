using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class GravState : MonoBehaviour
{
    private float originalMass;  // 初始质量
    private float originalDrag;  // 初始阻力
    private Bullet.BulletType originalState = Bullet.BulletType.Light; // 初始状态（可自定义）

    private Bullet.BulletType currentState;  // **現在是 private**

    private Rigidbody rb;

    [Header("子弹效果参数")]
    public float lightDrag = 3f;   // Light 子弹时的空气阻力
    public float heavyMultiplier = 3f;  // Heavy 子弹的质量倍数
    public float heavyFallBoost = 20f;  // Heavy 子弹额外加速度
    public float effectDuration = 5f;   // 效果持续时间（秒）

    // **提供 public 讀取屬性**
    public Bullet.BulletType CurrentState
    {
        get { return currentState; }  // 外部只能讀取，無法修改
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalMass = rb.mass;
            originalDrag = rb.linearDamping;
            currentState = originalState;  // 初始狀態

            UnityEngine.Debug.Log($"{gameObject.name} 初始质量: {originalMass}, 初始阻力: {originalDrag}");
        }
        else
        {
            UnityEngine.Debug.LogError($"{gameObject.name} 没有 Rigidbody，无法受子弹影响！");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            UnityEngine.Debug.Log($"{gameObject.name} 被 {bullet.bulletType} 子弹击中！");
            ApplyEffect(bullet.bulletType);
            Destroy(other.gameObject); // 销毁子弹
        }
    }

    void ApplyEffect(Bullet.BulletType bulletType)
    {
        if (rb == null) return;

        if (bulletType == Bullet.BulletType.Heavy)
        {
            currentState = Bullet.BulletType.Heavy;
            rb.mass = originalMass * heavyMultiplier;
            rb.linearDamping = 0f;
            rb.useGravity = true;
            UnityEngine.Debug.Log($"{gameObject.name} 变成 Heavy, mass={rb.mass}, drag={rb.linearDamping}");
        }
        else // bulletType == Bullet.BulletType.Light
        {
            currentState = Bullet.BulletType.Light;
            rb.mass = originalMass * 0.5f;
            rb.linearDamping = lightDrag;
            rb.useGravity = true;
            UnityEngine.Debug.Log($"{gameObject.name} 变成 Light, mass={rb.mass}, drag={rb.linearDamping}");
        }

        StopAllCoroutines();  // 防止效果疊加
        StartCoroutine(RevertEffectAfterTime(effectDuration));
    }

    IEnumerator RevertEffectAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);

        rb.mass = originalMass;
        rb.linearDamping = originalDrag;
        currentState = originalState;  // 恢复初始状态

        UnityEngine.Debug.Log($"{gameObject.name} 效果结束，恢复原始状态: mass={rb.mass}, drag={rb.linearDamping}");
    }

    void FixedUpdate()
    {
        if (rb != null && currentState == Bullet.BulletType.Heavy)
        {
            Vector3 gravityDir = Physics.gravity.normalized;
            rb.AddForce(gravityDir * heavyFallBoost, ForceMode.Acceleration);
        }
    }
}