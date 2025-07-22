using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Transform shootingPoint; // The point from which the player shoots
    public GameObject heavyBulletPrefab; // Bullet that makes objects heavier
    public GameObject lightBulletPrefab; // Bullet that makes objects lighter
    public LineRenderer lineRenderer; // Line to visualize shooting direction in 3D space
    public float bulletSpeed = 20f;
    public float lineLength = 5f; // Length of the aiming line
    public float fireRate = 0.2f; // Time between shots
    private float nextFireTime = 0f;

    void Update()
    {
        AimAtMouse();
        DrawAimLine();

        // Left-click to shoot heavy bullet
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot(heavyBulletPrefab);
        }

        // Right-click to shoot light bullet (gravity off)
        if (Input.GetMouseButton(1) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot(lightBulletPrefab);
        }
    }

    void AimAtMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z * -1f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosition);

        worldMousePos.z = shootingPoint.position.z; // Keep Z constant, rotate only in XY plane

        Vector2 direction = (worldMousePos - shootingPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shootingPoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void DrawAimLine()
    {
        if (lineRenderer == null) return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z * -1f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosition);

        worldMousePos.z = shootingPoint.position.z;

        Vector2 direction = (worldMousePos - shootingPoint.position).normalized;
        Vector3 lineEnd = shootingPoint.position + (Vector3)direction * lineLength;

        lineRenderer.SetPosition(0, shootingPoint.position);
        lineRenderer.SetPosition(1, lineEnd);
    }

    void Shoot(GameObject bulletPrefab)
    {
        GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        rb.useGravity = false; // Disable gravity on the bullet
        rb.linearVelocity = shootingPoint.right * bulletSpeed; // Shoot in the facing direction
    }
}