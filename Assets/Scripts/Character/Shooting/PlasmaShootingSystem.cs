using UnityEngine;

public class PlasmaShootingSystem : MonoBehaviour
{
    [Header("Plasma Beam Settings")]
    public LineRenderer beamLine;
    public LayerMask targetMask = -1;
    public float maxRange = 10f;
    public float beamWidth = 0.2f;
    public float fireRate = 0.1f;

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

    void Start()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = Camera.FindFirstObjectByType<Camera>();

        if (beamLine != null)
        {
            beamLine.enabled = false;
            beamLine.startWidth = beamWidth;
            beamLine.endWidth = beamWidth;
        }
    }

    void Update()
    {
        bool firing = false;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // Left click - Heavy beam
        {
            FireBeam(Bullet.BulletType.Heavy);
            firing = true;
            nextFireTime = Time.time + fireRate;
        }
        else if (Input.GetMouseButton(1) && Time.time >= nextFireTime) // Right click - Light beam
        {
            FireBeam(Bullet.BulletType.Light);
            firing = true;
            nextFireTime = Time.time + fireRate;
        }

        if (!firing)
        {
            StopBeam();
        }
    }

    void FireBeam(Bullet.BulletType beamType)
    {
        if (playerCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = playerCamera.nearClipPlane;
        Vector3 worldMousePos = playerCamera.ScreenToWorldPoint(mousePos);
        Vector3 direction = (worldMousePos - transform.position).normalized;

        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(transform.position, direction, out hit, maxRange, targetMask))
        {
            endPoint = hit.point;

            // Apply effect to target
            GravState gravState = hit.collider.GetComponent<GravState>();
            if (gravState != null)
            {
                ApplyGravityEffect(gravState, beamType);
            }

            // Show hit effect
            ShowHitEffect(hit.point, beamType);
        }
        else
        {
            endPoint = transform.position + direction * maxRange;
        }

        // Visual beam
        ShowBeam(transform.position, endPoint, beamType);

        // Audio
        PlayBeamSound(beamType);
    }

    void ApplyGravityEffect(GravState gravState, Bullet.BulletType beamType)
    {
        // Create a fake bullet to trigger the existing system
        GameObject tempBullet = new GameObject("TempBullet");
        tempBullet.transform.position = gravState.transform.position;

        Bullet bulletComponent = tempBullet.AddComponent<Bullet>();
        bulletComponent.bulletType = beamType;

        // Add collider to trigger the effect
        SphereCollider collider = tempBullet.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.1f;

        // The bullet will be destroyed by GravState's OnTriggerEnter
    }

    void ShowBeam(Vector3 start, Vector3 end, Bullet.BulletType beamType)
    {
        if (beamLine == null) return;

        beamLine.enabled = true;
        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);

        // Change beam color based on type
        if (beamType == Bullet.BulletType.Heavy)
        {
            beamLine.material.color = Color.red;
        }
        else
        {
            beamLine.material.color = Color.cyan;
        }
    }

    void ShowHitEffect(Vector3 hitPoint, Bullet.BulletType beamType)
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
                main.startColor = beamType == Bullet.BulletType.Heavy ? Color.red : Color.cyan;
            }
        }
    }

    void PlayBeamSound(Bullet.BulletType beamType)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = beamType == Bullet.BulletType.Heavy ? heavyBeamSound : lightBeamSound;
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    void StopBeam()
    {
        if (beamLine != null)
        {
            beamLine.enabled = false;
        }

        if (currentHitEffect != null)
        {
            Destroy(currentHitEffect);
            currentHitEffect = null;
        }
    }
}