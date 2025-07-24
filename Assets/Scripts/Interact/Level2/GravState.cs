using UnityEngine;
using System.Collections;

public class GravState : MonoBehaviour
{
    [Header("Effect Follow Target")]
    public Transform effectFollowTarget; 

    public enum GravityState { Normal, Heavy, Light }

    [Header("Physics Settings")]
    private float originalMass;
    private float originalDrag;
    private bool originalGravity;

    [Header("Effect Parameters")]
    public bool useTimer = true;           // Toggle for timer vs permanent effects
    public float lightDrag = 3f;
    public float heavyMultiplier = 3f;
    public float heavyFallBoost = 20f;
    public float effectDuration = 5f;      // Only used when useTimer is true

    [Header("Shader Materials")]
    public Material heavyOverlayMaterial;    // Drag and drop your heavy effect shader (additive/overlay)
    public Material lightOverlayMaterial;    // Drag and drop your light effect shader (additive/overlay)

    [Header("Shader Blending Options")]
    public bool useMultipleMaterials = true;  // Use multiple materials for overlay effects
    public bool replaceOriginalMaterial = false;  // If true, completely replace material instead of adding overlay

    [Header("Visual Effects")]
    public GameObject stateChangeEffect; // Optional particle effect when state changes

    private GravityState currentState = GravityState.Normal;
    private Rigidbody rb;
    private Renderer objectRenderer;
    private Material[] originalMaterials;  // Store all original materials

    // Public property to read current state
    public GravityState CurrentState
    {
        get { return currentState; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();

        if (rb != null)
        {
            originalMass = rb.mass;
            originalDrag = rb.linearDamping;
            originalGravity = rb.useGravity;
        }

        if (objectRenderer != null)
        {
            // Store all original materials
            originalMaterials = objectRenderer.materials;
        }

        Debug.Log($"{gameObject.name} initialized - Mass: {originalMass}, Drag: {originalDrag}, State: {currentState}");
    }

    void OnTriggerEnter(Collider other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            Debug.Log($"{gameObject.name} hit by {bullet.bulletType} beam!");
            ProcessStateChange(bullet.bulletType);
            Destroy(other.gameObject);
        }
    }

    void ProcessStateChange(Bullet.BulletType bulletType)
    {
        GravityState newState = CalculateNewState(bulletType);

        if (newState != currentState)
        {
            ChangeState(newState);
        }
        else
        {
            // Same state - refresh the timer if using timer mode
            if (useTimer && currentState != GravityState.Normal)
            {
                StopAllCoroutines();
                StartCoroutine(RevertToNormalAfterTime(effectDuration));
                Debug.Log($"{gameObject.name} hit by same beam type, timer refreshed!");
            }
            else
            {
                Debug.Log($"{gameObject.name} hit by same beam type, staying in {currentState} state.");
            }
        }
    }

    GravityState CalculateNewState(Bullet.BulletType bulletType)
    {
        // New logic: Stay in same state if hit by same bullet type
        // Only change when hit by opposite bullet type
        switch (currentState)
        {
            case GravityState.Normal:
                // From Normal: Go to Heavy or Light based on bullet type
                return bulletType == Bullet.BulletType.Heavy ? GravityState.Heavy : GravityState.Light;

            case GravityState.Heavy:
                // From Heavy: Only change if hit by Light bullet (goes to Normal first)
                return bulletType == Bullet.BulletType.Light ? GravityState.Normal : GravityState.Heavy;

            case GravityState.Light:
                // From Light: Only change if hit by Heavy bullet (goes to Normal first)
                return bulletType == Bullet.BulletType.Heavy ? GravityState.Normal : GravityState.Light;

            default:
                return GravityState.Normal;
        }
    }

    void ChangeState(GravityState newState)
    {
        currentState = newState;

        // Stop any existing coroutines
        StopAllCoroutines();

        // Apply physics changes
        ApplyPhysicsChanges(newState);

        // Apply shader changes
        ApplyShaderChanges(newState);

        // Show visual effect
        ShowStateChangeEffect(newState);

        // Start revert timer only if useTimer is enabled and not normal state
        if (useTimer && newState != GravityState.Normal)
        {
            StartCoroutine(RevertToNormalAfterTime(effectDuration));
        }

        Debug.Log($"{gameObject.name} changed to {newState} state" +
                 (useTimer && newState != GravityState.Normal ? $" (will revert in {effectDuration}s)" : " (permanent)"));
    }

    void ApplyPhysicsChanges(GravityState state)
    {
        if (rb == null) return;

        switch (state)
        {
            case GravityState.Normal:
                rb.mass = originalMass;
                rb.linearDamping = originalDrag;
                rb.useGravity = originalGravity;
                break;

            case GravityState.Heavy:
                rb.mass = originalMass * heavyMultiplier;
                rb.linearDamping = 0f;
                rb.useGravity = true;
                break;

            case GravityState.Light:
                rb.mass = originalMass * 0.5f;
                rb.linearDamping = lightDrag;
                rb.useGravity = true;
                break;
        }

        Debug.Log($"{gameObject.name} physics updated - Mass: {rb.mass}, Drag: {rb.linearDamping}, Gravity: {rb.useGravity}");
    }

    void ApplyShaderChanges(GravityState state)
    {
        if (objectRenderer == null || originalMaterials == null) return;

        switch (state)
        {
            case GravityState.Normal:
                // Restore original materials only
                objectRenderer.materials = originalMaterials;
                break;

            case GravityState.Heavy:
                ApplyOverlayEffect(heavyOverlayMaterial);
                break;

            case GravityState.Light:
                ApplyOverlayEffect(lightOverlayMaterial);
                break;
        }

        Debug.Log($"{gameObject.name} shader effect applied for {state} state");
    }

    void ApplyOverlayEffect(Material overlayMaterial)
    {
        if (overlayMaterial == null)
        {
            Debug.LogWarning($"{gameObject.name} - Overlay material not assigned!");
            return;
        }

        if (replaceOriginalMaterial)
        {
            // Replace mode: completely replace with the effect material
            objectRenderer.material = overlayMaterial;
        }
        else if (useMultipleMaterials)
        {
            // Additive mode: add overlay material to existing materials
            Material[] newMaterials = new Material[originalMaterials.Length + 1];

            // Copy original materials
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                newMaterials[i] = originalMaterials[i];
            }

            // Add overlay material as the last material
            newMaterials[newMaterials.Length - 1] = overlayMaterial;

            objectRenderer.materials = newMaterials;
        }
        else
        {
            // Blend mode: modify the main material properties (if supported)
            BlendWithOriginalMaterial(overlayMaterial);
        }
    }

    void BlendWithOriginalMaterial(Material effectMaterial)
    {
        // This method attempts to blend properties from the effect material
        // with the original material. Useful for shaders that support blending.

        if (originalMaterials.Length > 0)
        {
            Material blendedMaterial = new Material(originalMaterials[0]);

            // Example: Blend colors (adjust based on your shader properties)
            if (effectMaterial.HasProperty("_Color") && blendedMaterial.HasProperty("_Color"))
            {
                Color originalColor = blendedMaterial.color;
                Color effectColor = effectMaterial.color;
                blendedMaterial.color = Color.Lerp(originalColor, effectColor, 0.5f);
            }

            // Example: Add emission (adjust based on your shader properties)
            if (effectMaterial.HasProperty("_EmissionColor") && blendedMaterial.HasProperty("_EmissionColor"))
            {
                blendedMaterial.SetColor("_EmissionColor", effectMaterial.GetColor("_EmissionColor"));
                blendedMaterial.EnableKeyword("_EMISSION");
            }

            objectRenderer.material = blendedMaterial;
        }
    }

    void ShowStateChangeEffect(GravityState state)
    {
        if (stateChangeEffect != null)
        {
            // 创建空对象用于跟随目标
            Transform parent = effectFollowTarget != null ? effectFollowTarget : transform;

            // 实例化粒子作为目标的子物体
            GameObject effect = Instantiate(stateChangeEffect, parent.position, Quaternion.identity, parent);

            // 居中放置并保持缩放正常
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localScale = Vector3.one;

            // 设置粒子颜色
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                switch (state)
                {
                    case GravityState.Heavy:
                        main.startColor = Color.red;
                        break;
                    case GravityState.Light:
                        main.startColor = Color.cyan;
                        break;
                    case GravityState.Normal:
                        main.startColor = Color.white;
                        break;
                }
            }

            // 自动销毁粒子
            Destroy(effect, 2f);
        }
    }


    IEnumerator RevertToNormalAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Double-check that timer mode is still enabled and object isn't already normal
        if (useTimer && currentState != GravityState.Normal)
        {
            Debug.Log($"{gameObject.name} effect duration expired, reverting to Normal state");
            ChangeState(GravityState.Normal);
        }
    }

    void FixedUpdate()
    {
        // Apply extra downward force for heavy objects
        if (rb != null && currentState == GravityState.Heavy)
        {
            Vector3 gravityDir = Physics.gravity.normalized;
            rb.AddForce(gravityDir * heavyFallBoost, ForceMode.Acceleration);
        }
    }

    // Public method to manually change state (useful for testing)
    public void SetState(GravityState newState)
    {
        ChangeState(newState);
    }

    // Public method to check if object can be affected (useful for other scripts)
    public bool CanChangeState(Bullet.BulletType bulletType)
    {
        GravityState wouldBecomeState = CalculateNewState(bulletType);
        return wouldBecomeState != currentState;
    }
}
