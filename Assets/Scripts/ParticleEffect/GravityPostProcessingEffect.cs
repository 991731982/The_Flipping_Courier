using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Settings class for the gravity overlay effect
[System.Serializable]
public class GravityOverlaySettings
{
    [Header("Colors")]
    public Color upColor = new Color(0.3f, 0.5f, 0.9f, 0.05f);     // Ultra-soft blue for up
    public Color downColor = new Color(0.9f, 0.4f, 0.3f, 0.05f);   // Ultra-soft red for down

    [Header("Gradient")]
    [Range(0f, 1f)]
    public float gradientIntensity = 0.3f;
    [Range(0f, 2f)]
    public float gradientWidth = 2.0f;

    [Header("Animation")]
    [Range(0.5f, 3f)]
    public float transitionSpeed = 2.5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Arrows")]
    public bool showArrows = true;
    [Range(0f, 1f)]
    public float arrowOpacity = 0.15f;
    [Range(0.5f, 3f)]
    public float arrowScale = 1f;
    [Range(0.1f, 2f)]
    public float arrowAnimationSpeed = 0.8f;

    [Header("Breathing Effect")]
    public bool enableBreathing = true;
    [Range(0.1f, 1f)]
    public float breathingIntensity = 0.05f;
    [Range(0.1f, 2f)]
    public float breathingSpeed = 0.3f;
}

// Main post-processing effect component
public class GravityPostProcessingEffect : MonoBehaviour
{
    [Header("Settings")]
    public GravityOverlaySettings settings = new GravityOverlaySettings();

    [Header("References")]
    public GravityController gravityController;
    public Material overlayMaterial;

    private Camera cam;
    private bool currentGravityState;
    private float transitionProgress = 0f;
    private Coroutine transitionCoroutine;

    // Shader property IDs for performance
    private static readonly int UpColorID = Shader.PropertyToID("_UpColor");
    private static readonly int DownColorID = Shader.PropertyToID("_DownColor");
    private static readonly int TransitionID = Shader.PropertyToID("_Transition");
    private static readonly int GradientIntensityID = Shader.PropertyToID("_GradientIntensity");
    private static readonly int GradientWidthID = Shader.PropertyToID("_GradientWidth");
    private static readonly int ArrowOpacityID = Shader.PropertyToID("_ArrowOpacity");
    private static readonly int ArrowScaleID = Shader.PropertyToID("_ArrowScale");
    private static readonly int TimeID = Shader.PropertyToID("_Time");
    private static readonly int ArrowSpeedID = Shader.PropertyToID("_ArrowSpeed");
    private static readonly int EnableBreathingID = Shader.PropertyToID("_EnableBreathing");
    private static readonly int BreathingIntensityID = Shader.PropertyToID("_BreathingIntensity");
    private static readonly int BreathingSpeedID = Shader.PropertyToID("_BreathingSpeed");

    void Start()
    {
        cam = GetComponent<Camera>();
        if (!gravityController)
            gravityController = GravityController.FindFirstObjectByType<GravityController>();

        if (!overlayMaterial)
        {
            Debug.LogError("GravityPostProcessingEffect: No overlay material assigned!");
            return;
        }

        currentGravityState = gravityController.gravityFlipped;
        transitionProgress = currentGravityState ? 1f : 0f;

        UpdateShaderProperties();
    }

    void Update()
    {
        if (!gravityController || !overlayMaterial) return;

        // Check for gravity state change
        if (currentGravityState != gravityController.gravityFlipped)
        {
            currentGravityState = gravityController.gravityFlipped;
            StartTransition();
        }

        // Update time for arrow animations
        overlayMaterial.SetFloat(TimeID, Time.time);

        UpdateShaderProperties();
    }

    void StartTransition()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionCoroutine());
    }

    IEnumerator TransitionCoroutine()
    {
        float startProgress = transitionProgress;
        float targetProgress = currentGravityState ? 1f : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < 1f / settings.transitionSpeed)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime * settings.transitionSpeed;
            float curveValue = settings.transitionCurve.Evaluate(normalizedTime);

            transitionProgress = Mathf.Lerp(startProgress, targetProgress, curveValue);
            yield return null;
        }

        transitionProgress = targetProgress;
        transitionCoroutine = null;
    }

    void UpdateShaderProperties()
    {
        if (!overlayMaterial) return;

        overlayMaterial.SetColor(UpColorID, settings.upColor);
        overlayMaterial.SetColor(DownColorID, settings.downColor);
        overlayMaterial.SetFloat(TransitionID, transitionProgress);
        overlayMaterial.SetFloat(GradientIntensityID, settings.gradientIntensity);
        overlayMaterial.SetFloat(GradientWidthID, settings.gradientWidth);
        overlayMaterial.SetFloat(ArrowOpacityID, settings.showArrows ? settings.arrowOpacity : 0f);
        overlayMaterial.SetFloat(ArrowScaleID, settings.arrowScale);
        overlayMaterial.SetFloat(ArrowSpeedID, settings.arrowAnimationSpeed);
        overlayMaterial.SetFloat(EnableBreathingID, settings.enableBreathing ? 1f : 0f);
        overlayMaterial.SetFloat(BreathingIntensityID, settings.breathingIntensity);
        overlayMaterial.SetFloat(BreathingSpeedID, settings.breathingSpeed);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (overlayMaterial != null)
        {
            Graphics.Blit(source, destination, overlayMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    // Public methods for external control
    public void SetOverlayIntensity(float intensity)
    {
        settings.upColor.a = intensity;
        settings.downColor.a = intensity;
    }

    public void SetTransitionSpeed(float speed)
    {
        settings.transitionSpeed = speed;
    }

    public void ToggleArrows(bool show)
    {
        settings.showArrows = show;
    }

    public void ToggleBreathing(bool enable)
    {
        settings.enableBreathing = enable;
    }
}