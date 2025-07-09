using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Transform shootingPoint; // 角色的射击点
    public GameObject heavyBulletPrefab; // 让物体变重的子弹
    public GameObject lightBulletPrefab; // 让物体变轻的子弹
    public LineRenderer lineRenderer; // 3D 空间中的射击方向指示线
    public float bulletSpeed = 20f;
    public float lineLength = 5f; // 线的长度
    public float fireRate = 0.2f; // 射击间隔
    private float nextFireTime = 0f;

    void Update()
    {
        AimAtMouse();
        DrawAimLine();

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // 左键射击 (超重子弹)
        {
            nextFireTime = Time.time + fireRate;
            Shoot(heavyBulletPrefab);
        }

        if (Input.GetMouseButton(1) && Time.time >= nextFireTime) // 右键射击 (变轻无重力子弹)
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

        worldMousePos.z = shootingPoint.position.z; // 保持Z轴不变，仅在XY平面旋转

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

        rb.useGravity = false; // 关闭子弹重力
        rb.linearVelocity = shootingPoint.right * bulletSpeed; // 沿X-Y轴方向发射
    }
}
