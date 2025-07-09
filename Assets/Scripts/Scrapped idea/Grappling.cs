using UnityEngine;

public class GrapplingMechanic : MonoBehaviour
{
    [Header("Grappling Settings")]
    public Transform grapplePoint;         // 抓钩点
    public float grappleRange = 15f;       // 抓钩范围
    public float ropeLength = 5f;          // 绳子的长度

    [Header("Physics Settings")]
    public float swingForce = 15f;         // 玩家推动摆动的力
    public float maxSwingAngle = 90f;      // 最大摆动角度限制
    public float airDrag = 0.1f;           // 空气阻力
    public float gravityMultiplier = 1.5f;// 自定义重力倍增
    public float pullForce = 20f;          // 拉向正下方的力

    private Rigidbody rb;                  // 玩家刚体
    private ConfigurableJoint joint;       // 实现绳索物理的关节
    private bool isSwinging = false;       // 是否正在抓钩摆动

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on the object!");
        }
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        if (isSwinging)
        {
            ApplyCustomGravity();
            ApplyTensionForce();

            if (!IsPlayerInputDetected())
            {
                ApplyPullToCenterForce();
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            TryToGrapple();
        }

        if (isSwinging)
        {
            HandleSwingingInput();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReleaseGrapple();
            }
        }
    }

    private void TryToGrapple()
    {
        if (grapplePoint == null)
        {
            Debug.LogWarning("Grapple point is not assigned!");
            return;
        }

        if (Vector3.Distance(transform.position, grapplePoint.position) <= grappleRange)
        {
            StartSwinging();
        }
        else
        {
            Debug.Log("Grapple point out of range.");
        }
    }

    private void StartSwinging()
    {
        isSwinging = true;

        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint.position;

        joint.linearLimit = new SoftJointLimit { limit = ropeLength };
        joint.linearLimitSpring = new SoftJointLimitSpring { spring = 0f, damper = 0f };

        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        rb.linearDamping = airDrag;
    }

    private void HandleSwingingInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput != 0)
        {
            Vector3 swingDirection = grapplePoint.position - transform.position;
            Vector3 perpendicularDirection = Vector3.Cross(swingDirection, Vector3.up).normalized;

            Vector3 swingForceVector = perpendicularDirection * horizontalInput * swingForce;

            if (Vector3.Angle(swingDirection, Vector3.down) < maxSwingAngle)
            {
                rb.AddForce(swingForceVector, ForceMode.Acceleration);
            }
        }
    }

    private void ApplyCustomGravity()
    {
        Vector3 gravity = Physics.gravity * gravityMultiplier;

        // 增加额外重力
        if (Vector3.Distance(transform.position, grapplePoint.position) < ropeLength * 0.8f)
        {
            gravity += Vector3.down * pullForce * 0.5f;
        }

        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    private void ApplyTensionForce()
    {
        if (grapplePoint == null) return;

        Vector3 directionToGrapple = grapplePoint.position - transform.position;
        float currentDistance = directionToGrapple.magnitude;

        if (currentDistance > ropeLength)
        {
            Vector3 tension = directionToGrapple.normalized * (currentDistance - ropeLength) * swingForce;
            rb.AddForce(tension, ForceMode.Acceleration);
        }
    }

    private void ApplyPullToCenterForce()
    {
        // 计算偏移
        Vector3 grappleToDownward = Vector3.down;
        Vector3 currentDirection = grapplePoint.position - transform.position;
        Vector3 downwardProjection = Vector3.ProjectOnPlane(currentDirection, grappleToDownward);

        // 动态调整力的大小
        float distanceToBottom = downwardProjection.magnitude;
        float dynamicPullForce = pullForce * Mathf.Clamp01(distanceToBottom / ropeLength);

        // 施加力
        rb.AddForce(-downwardProjection.normalized * dynamicPullForce, ForceMode.Acceleration);
    }


    private bool IsPlayerInputDetected()
    {
        return Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
    }

    private void ReleaseGrapple()
    {
        if (joint != null)
        {
            Destroy(joint);
        }

        isSwinging = false;
        rb.linearDamping = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (grapplePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grapplePoint.position, grappleRange);
        }
    }
}
