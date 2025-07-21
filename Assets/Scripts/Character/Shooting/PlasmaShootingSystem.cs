using UnityEngine;

public class PlasmaShootingSystem : MonoBehaviour
{
    public LineRenderer beamLine;
    public LayerMask targetMask;
    public float maxRange = 10f;
    public float beamWidth = 0.2f;

    private GameObject currentTarget;
    private GravState targetGravState;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            FireBeam(Bullet.BulletType.Heavy);
        }
        else if (Input.GetMouseButton(1))
        {
            FireBeam(Bullet.BulletType.Light);
        }
        else
        {
           // StopBeam();
        }
    }

    void FireBeam(Bullet.BulletType beamType)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = (mousePos - transform.position).normalized;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, maxRange, targetMask))
        {
            // Visual beam
            beamLine.enabled = true;
            beamLine.SetPosition(0, transform.position);
            beamLine.SetPosition(1, hit.point);

            // Apply effect to target
           /* GravState gravState = hit.collider.GetComponent();
            if (gravState != null)
            {
                //gravState.ApplyWeightChange(beamType);
                ShowWeightAura(hit.collider.gameObject, beamType);
            }*/
        }
    }

    void ShowWeightAura(GameObject target, Bullet.BulletType type)
    {
        // Add particle aura around object
        // Change material emission color based on weight
    }
}