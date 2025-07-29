using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;
    public float rotationSpeed = 8f;

    [Header("Eye Movement Constraints")]
    public float maxHorizontalAngle = 45f;
    public float maxVerticalAngle = 30f;
    public float maxRollAngle = 15f;

    [Header("Angle Configuration")]
    public Vector3 centerRotation = new Vector3(0f, 0f, 0f);

    [Header("Debug Tools")]
    public bool autoFindCenterRotation = false;
    public KeyCode calibrateKey = KeyCode.C;
    [Space]
    [TextArea(3, 5)]
    public string debugInfo = "Position player in front of eye and press C to auto-calibrate center rotation";

    [Header("Dead Zone (Anti-Jitter)")]
    public float deadZoneRadius = 0.8f;
    public float minDistanceForRotation = 0.2f;

    [Header("Gravity Adaptation")]
    public bool adaptToPlayerGravity = true;
    public GravityController playerGravityController;

    private Vector3 lastValidDirection;
    private bool hasValidDirection = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (playerGravityController == null)
                    playerGravityController = playerObj.GetComponent<GravityController>();
            }
            else
            {
                Debug.LogError("Player not found! Tag your player as 'Player'");
                enabled = false;
                return;
            }
        }

        lastValidDirection = Vector3.forward;
    }

    void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(calibrateKey)) CalibrateEyeRotation();
        if (autoFindCenterRotation)
        {
            AutoCalibrateRotation();
            return;
        }

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer < minDistanceForRotation)
            return;

        Vector3 localDirection = transform.InverseTransformDirection(directionToPlayer.normalized);
        Vector3 adjustedDirection = new Vector3(localDirection.x, localDirection.z, localDirection.y);

        Vector3 targetRotation = centerRotation;

        // Check dead zone by angular threshold, not position magnitude
        if (adjustedDirection.magnitude > deadZoneRadius / distanceToPlayer)
        {
            if (hasValidDirection)
            {
                float directionChange = Vector3.Angle(lastValidDirection, adjustedDirection);
                if (directionChange < 90f)
                {
                    lastValidDirection = Vector3.Slerp(lastValidDirection, adjustedDirection, Time.deltaTime * 5f);
                }
            }
            else
            {
                lastValidDirection = adjustedDirection;
                hasValidDirection = true;
            }

            Vector3 smoothDirection = lastValidDirection;

            float horizontalAngle = Mathf.Atan2(smoothDirection.x, Mathf.Abs(smoothDirection.z)) * Mathf.Rad2Deg;
            horizontalAngle = Mathf.Clamp(horizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);

            float verticalAngle = Mathf.Atan2(smoothDirection.y, Mathf.Abs(smoothDirection.z)) * Mathf.Rad2Deg;
            verticalAngle = Mathf.Clamp(verticalAngle, -maxVerticalAngle, maxVerticalAngle);

            float rollAngle = 0f;
            if (adaptToPlayerGravity && playerGravityController != null)
            {
                float gravityInfluence = playerGravityController.gravityFlipped ? -1f : 1f;
                float distanceInfluence = Mathf.Clamp01(5f / distanceToPlayer);

                rollAngle = gravityInfluence * maxRollAngle * distanceInfluence;

                if (playerGravityController.gravityFlipped)
                {
                    verticalAngle *= 0.7f;
                }
            }

            targetRotation = new Vector3(
                centerRotation.x + verticalAngle,
                centerRotation.y + rollAngle,
                centerRotation.z + horizontalAngle
            );
        }

        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
        float speedMultiplier = Mathf.Clamp01(distanceToPlayer / 8f);
        float adjustedSpeed = rotationSpeed * speedMultiplier;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetQuaternion,
            Time.deltaTime * adjustedSpeed
        );
    }

    void CalibrateEyeRotation()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        Vector3 worldEulerAngles = lookRotation.eulerAngles;

        Quaternion parentInverse = transform.parent ? Quaternion.Inverse(transform.parent.rotation) : Quaternion.identity;
        Quaternion localLookRotation = parentInverse * lookRotation;
        Vector3 localEulerAngles = localLookRotation.eulerAngles;

        if (localEulerAngles.x > 180) localEulerAngles.x -= 360;
        if (localEulerAngles.y > 180) localEulerAngles.y -= 360;
        if (localEulerAngles.z > 180) localEulerAngles.z -= 360;

        centerRotation = localEulerAngles;
        debugInfo = $"Calibrated! Center Rotation: ({centerRotation.x:F1}, {centerRotation.y:F1}, {centerRotation.z:F1})";
        Debug.Log(debugInfo);
    }

    void AutoCalibrateRotation()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

        Quaternion parentInverse = transform.parent ? Quaternion.Inverse(transform.parent.rotation) : Quaternion.identity;
        Quaternion localLookRotation = parentInverse * lookRotation;

        transform.localRotation = localLookRotation;

        Vector3 currentEuler = localLookRotation.eulerAngles;
        if (currentEuler.x > 180) currentEuler.x -= 360;
        if (currentEuler.y > 180) currentEuler.y -= 360;
        if (currentEuler.z > 180) currentEuler.z -= 360;

        debugInfo = $"Auto-Calibrating... Current Rotation: ({currentEuler.x:F1}, {currentEuler.y:F1}, {currentEuler.z:F1})";
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);

            Gizmos.color = autoFindCenterRotation ? Color.green : Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.up * 1f);

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, deadZoneRadius);

            if (!autoFindCenterRotation)
            {
                Gizmos.color = Color.cyan;

                Vector3 leftBound = Quaternion.AngleAxis(-maxHorizontalAngle, transform.up) * transform.forward * 2f;
                Vector3 rightBound = Quaternion.AngleAxis(maxHorizontalAngle, transform.up) * transform.forward * 2f;

                Gizmos.DrawLine(transform.position, transform.position + leftBound);
                Gizmos.DrawLine(transform.position, transform.position + rightBound);

                Vector3 upBound = Quaternion.AngleAxis(maxVerticalAngle, transform.right) * transform.forward * 2f;
                Vector3 downBound = Quaternion.AngleAxis(-maxVerticalAngle, transform.right) * transform.forward * 2f;

                Gizmos.DrawLine(transform.position, transform.position + upBound);
                Gizmos.DrawLine(transform.position, transform.position + downBound);
            }

            if (autoFindCenterRotation)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(player.position, 0.5f);
            }
        }
    }
}