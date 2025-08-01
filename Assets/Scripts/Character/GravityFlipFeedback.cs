using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

public class GravityFlipFeedback : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    public float ringExpansionSpeed = 8f;
    public float ringFadeDuration = 0.4f;
    public float rimLightIntensity = 2f;
    public float rimLightDuration = 0.6f;

    [Header("Screen Effects")]
    public bool enableScreenEffects = true;

    [Header("Audio Feedback")]
    public AudioClip flipUpSound;     // W键触发
    public AudioClip flipDownSound;   // S键触发
    private AudioSource audioSource;

    [Header("Colors")]
    public Color normalGravityColor = new Color(1f, 0.42f, 0.21f, 1f);     // Orange
    public Color flippedGravityColor = new Color(0.29f, 0.56f, 0.89f, 1f); // Blue

    [Header("Arrow Feedback")]
    public GameObject arrowPrefab;
    public float arrowMoveDistance = 1f;
    public float arrowMoveDuration = 0.4f;
    public int arrowCount = 3;
    public float arrowSpacing = 0.3f;

    private Camera mainCamera;
    private Light rimLight;
    private GravityController gravityController;

    private LineRenderer ringRenderer;
    private int ringSegments = 32;

    private ParticleSystem trailParticles;

    void Start()
    {
        mainCamera = Camera.main;
        gravityController = GetComponent<GravityController>();

        SetupRimLight();
        SetupRingEffect();
        SetupTrailParticles();


        mainCamera = Camera.main;
        gravityController = GetComponent<GravityController>();

        SetupRimLight();
        SetupRingEffect();
        SetupTrailParticles();

        // 添加 AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void SetupRimLight()
    {
        GameObject rimLightObj = new GameObject("RimLight");
        rimLightObj.transform.SetParent(transform);
        rimLightObj.transform.localPosition = Vector3.zero;

        rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Point;
        rimLight.range = 3f;
        rimLight.intensity = 0f;
        rimLight.shadows = LightShadows.None;
        rimLight.color = normalGravityColor;
    }

    void SetupRingEffect()
    {
        GameObject ringObj = new GameObject("GravityRing");
        ringObj.transform.SetParent(transform);
        ringObj.transform.localPosition = Vector3.zero;

        ringRenderer = ringObj.AddComponent<LineRenderer>();
        ringRenderer.material = CreateRingMaterial();
        ringRenderer.startWidth = 0.1f;
        ringRenderer.endWidth = 0.1f;
        ringRenderer.positionCount = ringSegments + 1;
        ringRenderer.useWorldSpace = false;
        ringRenderer.enabled = false;

        for (int i = 0; i <= ringSegments; i++)
        {
            float angle = i * 2f * Mathf.PI / ringSegments;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.1f;
            ringRenderer.SetPosition(i, pos);
        }
    }

    void SetupTrailParticles()
    {
        GameObject particleObj = new GameObject("TrailParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;

        trailParticles = particleObj.AddComponent<ParticleSystem>();
        var main = trailParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = normalGravityColor;
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = trailParticles.emission;
        emission.enabled = false;

        var shape = trailParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
    }

    Material CreateRingMaterial()
    {
        Shader unlitShader = Shader.Find("Unlit/Color");
        Material ringMat = new Material(unlitShader);
        ringMat.color = normalGravityColor;
        return ringMat;
    }

    public void TriggerGravityFlipFeedback(bool isFlippedUp)
    {
        Color currentColor = isFlippedUp ? flippedGravityColor : normalGravityColor;

        rimLight.color = currentColor;
        ringRenderer.material.color = currentColor;

        var main = trailParticles.main;
        main.startColor = currentColor;

        StartCoroutine(RingExpansionEffect());
        StartCoroutine(RimLightPulse());
        TriggerParticleBurst(isFlippedUp);

        if (enableScreenEffects)
        {
            StartCoroutine(SubtleScreenFlash(currentColor));
        }

        SpawnArrows(isFlippedUp);

        // 播放音效
        if (audioSource != null)
        {
            AudioClip selectedClip = isFlippedUp ? flipUpSound : flipDownSound;
            if (selectedClip != null)
            {
                audioSource.PlayOneShot(selectedClip);
            }
        }
    }


    IEnumerator RingExpansionEffect()
    {
        ringRenderer.enabled = true;
        float elapsed = 0f;
        float startRadius = 0.1f;
        float endRadius = 2.5f;

        while (elapsed < ringFadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / ringFadeDuration;

            float currentRadius = Mathf.Lerp(startRadius, endRadius, progress);
            for (int i = 0; i <= ringSegments; i++)
            {
                float angle = i * 2f * Mathf.PI / ringSegments;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * currentRadius;
                ringRenderer.SetPosition(i, pos);
            }

            Color currentColor = ringRenderer.material.color;
            currentColor.a = 1f - progress;
            ringRenderer.material.color = currentColor;

            yield return null;
        }

        ringRenderer.enabled = false;
        Color resetColor = ringRenderer.material.color;
        resetColor.a = 1f;
        ringRenderer.material.color = resetColor;
    }

    IEnumerator RimLightPulse()
    {
        float elapsed = 0f;
        float halfDuration = rimLightDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / halfDuration;
            rimLight.intensity = Mathf.Lerp(0f, rimLightIntensity, progress);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / halfDuration;
            rimLight.intensity = Mathf.Lerp(rimLightIntensity, 0f, progress);
            yield return null;
        }

        rimLight.intensity = 0f;
    }

    void TriggerParticleBurst(bool isFlippedUp)
    {
        var velocityOverLifetime = trailParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;

        Vector3 gravityDirection = isFlippedUp ? Vector3.up : Vector3.down;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(gravityDirection.y * 3f);

        trailParticles.Emit(12);
    }

    void SpawnArrows(bool isFlippedUp)
    {
        float direction = isFlippedUp ? 1f : -1f;
        Vector3[] sides = { Vector3.left, Vector3.right };
        Color arrowColor = isFlippedUp ? flippedGravityColor : normalGravityColor;

        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, isFlippedUp ? 180f : 0f);

        foreach (Vector3 side in sides)
        {
            for (int i = 0; i < arrowCount; i++)
            {
                Vector3 spawnOffset = side * 0.6f + Vector3.down * (i * arrowSpacing * direction);
                Vector3 startPos = transform.position + spawnOffset;
                GameObject arrow = Instantiate(arrowPrefab, startPos, arrowRotation);

                SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = arrowColor;

                StartCoroutine(AnimateArrow(arrow, direction));
            }
        }
    }



    IEnumerator AnimateArrow(GameObject arrow, float direction)
    {
        float elapsed = 0f;
        Vector3 startPos = arrow.transform.position;
        Vector3 endPos = startPos + Vector3.up * arrowMoveDistance * direction;

        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        Color startColor = sr.color;

        while (elapsed < arrowMoveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / arrowMoveDuration;

            arrow.transform.position = Vector3.Lerp(startPos, endPos, progress);

            Color faded = startColor;
            faded.a = Mathf.Lerp(1f, 0f, progress);
            sr.color = faded;

            yield return null;
        }

        Destroy(arrow);
    }

    IEnumerator SubtleScreenFlash(Color flashColor)
    {
        GameObject flashObj = new GameObject("ScreenFlash");
        Canvas canvas = flashObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        UnityEngine.UI.Image flashImage = flashObj.AddComponent<UnityEngine.UI.Image>();
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        RectTransform rect = flashImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        float flashDuration = 0.15f;
        float elapsed = 0f;
        float maxAlpha = 0.1f;

        while (elapsed < flashDuration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (flashDuration * 0.3f);
            Color currentColor = flashImage.color;
            currentColor.a = Mathf.Lerp(0f, maxAlpha, progress);
            flashImage.color = currentColor;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < flashDuration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (flashDuration * 0.7f);
            Color currentColor = flashImage.color;
            currentColor.a = Mathf.Lerp(maxAlpha, 0f, progress);
            flashImage.color = currentColor;
            yield return null;
        }

        Destroy(flashObj);
    }
}