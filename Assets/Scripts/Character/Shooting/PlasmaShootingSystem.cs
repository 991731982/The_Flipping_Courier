using UnityEngine;

public class PlasmaShootingSystem : MonoBehaviour
{
    [Header("Plasma Beam Settings")]
    public LineRenderer beamLine;
    public LayerMask targetMask = -1;
    public float maxRange = 10f;
    public float beamWidth = 0.2f;
    public float fireRate = 0.1f;
    public float aimLineLength = 5f; // Length of the aiming line

    [Header("Visual Effects")]
    public GameObject beamStartEffect; // Optional particle effect at shooting point
    public GameObject beamHitEffect;   // Optional particle effect at hit point

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip heavyBeamSound;
    public AudioClip lightBeamSound;

    private float nextFireTime = 0f;
    private Camera playerCamera;
    private GameObject currentHitEffect;
    private bool isFiring = false;

    void Start()
    {
        // Try multiple ways to find the camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogWarning("Camera.main not found, trying FindObjectOfType<Camera>");
            playerCamera = Camera.FindFirstObjectByType<Camera>();

        }
        if (playerCamera == null)
        {
            Debug.LogWarning("FindObjectOfType<Camera> failed, looking for camera tagged 'MainCamera'");
            GameObject cameraObj = GameObject.FindWithTag("MainCamera");
            if (cameraObj != null)
            {
                playerCamera = cameraObj.GetComponent<Camera>();
            }
        }

        if (playerCamera == null)
        {
            Debug.LogError("No camera found! PlasmaShootingSystem needs a camera to work.");
            Debug.LogError("Make sure you have a camera in the scene and it's either:");
            Debug.LogError("1. Set as Camera.main");
            Debug.LogError("2. Tagged with 'MainCamera'");
            Debug.LogError("3. Is a Camera component in the scene");
        }
        else
        {
            Debug.Log($"Camera found: {playerCamera.name}");
        }

        if (beamLine != null)
        {
            beamLine.enabled = true; // Keep it enabled for aiming line
            beamLine.startWidth = beamWidth;
            beamLine.endWidth = beamWidth;
            beamLine.positionCount = 2;
            Debug.Log("LineRenderer configured successfully");
        }
        else
        {
            Debug.LogError("LineRenderer not assigned to PlasmaShootingSystem!");
        }
    }

    void Update()
    {
        isFiring = false;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // Left click - Heavy beam
        {
            FireBeam(true); // true = heavy beam
            isFiring = true;
            nextFireTime = Time.time + fireRate;
        }
        else if (Input.GetMouseButton(1) && Time.time >= nextFireTime) // Right click - Light beam
        {
            FireBeam(false); // false = light beam
            isFiring = true;
            nextFireTime = Time.time + fireRate;
        }
        else
        {
            // Show aiming line when not firing
            DrawAimLine();
        }

        if (!isFiring)
        {
            StopBeamEffects(); // Only stop effects, not the line renderer
        }
    }

    void DrawAimLine()
    {
        if (beamLine == null || playerCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = playerCamera.nearClipPlane;
        Vector3 worldMousePos = playerCamera.ScreenToWorldPoint(mousePos);
        Vector3 direction = (worldMousePos - transform.position).normalized;

        Vector3 aimEndPoint = transform.position + direction * aimLineLength;

        beamLine.enabled = true;
        beamLine.SetPosition(0, transform.position);
        beamLine.SetPosition(1, aimEndPoint);

        // Set aiming line color (neutral color)
        beamLine.material.color = Color.white;
    }

    void FireBeam(bool isHeavy)
    {
        Debug.Log("FireBeam called");

        if (playerCamera == null)
        {
            Debug.LogError("No camera available for beam firing!");
            return;
        }

        if (beamLine == null)
        {
            Debug.LogError("LineRenderer not assigned!");
            return;
        }

        if (transform == null)
        {
            Debug.LogError("Transform is null!");
            return;
        }

        Debug.Log("Getting mouse position...");
        Vector3 mousePos = Input.mousePosition;
        Debug.Log($"Mouse position: {mousePos}");

        mousePos.z = playerCamera.nearClipPlane;
        Debug.Log($"Camera near clip plane: {playerCamera.nearClipPlane}");

        Vector3 worldMousePos = playerCamera.ScreenToWorldPoint(mousePos);
        Debug.Log($"World mouse position: {worldMousePos}");
        Debug.Log($"Transform position: {transform.position}");

        Vector3 direction = (worldMousePos - transform.position).normalized;
        Debug.Log($"Direction: {direction}");

        string beamType = isHeavy ? "Heavy" : "Light";
        Debug.Log($"Firing {beamType} beam from {transform.position} towards {direction}");

        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(transform.position, direction, out hit, maxRange, targetMask))
        {
            endPoint = hit.point;
            Debug.Log($"Beam hit: {hit.collider.name} at {hit.point}");

            // Apply effect to target
            GravState gravState = hit.collider.GetComponent<GravState>();
            if (gravState != null)
            {
                ApplyGravityEffect(gravState, isHeavy);
            }
            else
            {
                Debug.Log($"Hit object {hit.collider.name} has no GravState component");
            }

            // Show hit effect
            ShowHitEffect(hit.point, isHeavy);
        }
        else
        {
            endPoint = transform.position + direction * maxRange;
            Debug.Log($"Beam reached max range: {endPoint}");
        }

        // Visual beam
        ShowBeam(transform.position, endPoint, isHeavy);

        // Audio
        PlayBeamSound(isHeavy);
    }

    void ApplyGravityEffect(GravState gravState, bool isHeavy)
    {
        // Create a fake bullet to trigger the existing system
        GameObject tempBullet = new GameObject("TempBullet");
        tempBullet.transform.position = gravState.transform.position;

        Bullet bulletComponent = tempBullet.AddComponent<Bullet>();
        // Set the bullet type based on whether it's heavy or light
        if (bulletComponent != null)
        {
            // You may need to adjust this based on how your Bullet class works
            // For now, we'll try to set the bulletType if it exists
            var bulletTypeField = bulletComponent.GetType().GetField("bulletType");
            if (bulletTypeField != null)
            {
                // Try to find the enum values - this might need adjustment based on your actual enum
                var enumType = bulletTypeField.FieldType;
                if (enumType.IsEnum)
                {
                    var enumValues = System.Enum.GetValues(enumType);
                    if (enumValues.Length >= 2)
                    {
                        bulletTypeField.SetValue(bulletComponent, enumValues.GetValue(isHeavy ? 0 : 1));
                    }
                }
            }
        }

        // Add collider to trigger the effect
        SphereCollider collider = tempBullet.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.1f;

        // The bullet will be destroyed by GravState's OnTriggerEnter
    }

    void ShowBeam(Vector3 start, Vector3 end, bool isHeavy)
    {
        if (beamLine == null) return;

        beamLine.enabled = true;
        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);

        // Change beam color based on type
        if (isHeavy)
        {
            beamLine.material.color = Color.red;
        }
        else
        {
            beamLine.material.color = Color.cyan;
        }
    }

    void ShowHitEffect(Vector3 hitPoint, bool isHeavy)
    {
        if (beamHitEffect != null)
        {
            if (currentHitEffect != null)
                Destroy(currentHitEffect);

            currentHitEffect = Instantiate(beamHitEffect, hitPoint, Quaternion.identity);

            // Color the effect based on beam type
            ParticleSystem ps = currentHitEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = isHeavy ? Color.red : Color.cyan;
            }
        }
    }

    void PlayBeamSound(bool isHeavy)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = isHeavy ? heavyBeamSound : lightBeamSound;
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    void StopBeamEffects()
    {
        // Only destroy hit effects, keep the line renderer for aiming
        if (currentHitEffect != null)
        {
            Destroy(currentHitEffect);
            currentHitEffect = null;
        }
    }
}
