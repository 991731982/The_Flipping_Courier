using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float moveSpeed = 3f;
    public float stoppingDistance = 1f;

    [Header("Vision Cone Settings")]
    public float fieldOfViewAngle = 90f;
    public LayerMask obstructionMask; // Set this to Environment layer in Inspector

    private Rigidbody rb;
    private bool isChasing = false;
    private bool canDamagePlayer = true;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found! Make sure the player has the 'Player' tag.");
        }

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange && distanceToPlayer > stoppingDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0f;

            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle < fieldOfViewAngle / 2f)
            {
                // Perform raycast from "eye" position
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
                if (!Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    isChasing = true;
                }
                else
                {
                    isChasing = false;
                }
            }
            else
            {
                isChasing = false;
            }
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        Vector3 velocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            checkPointRespawn playerRespawn = collision.gameObject.GetComponent<checkPointRespawn>();
            if (playerRespawn != null)
            {
                playerRespawn.RespawnAtCheckpoint();
                Debug.Log("Player instantly killed and respawned by enemy.");
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamagePlayer = false;
        yield return new WaitForSeconds(5f);
        canDamagePlayer = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Visualize vision cone
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftBoundary * chaseRange);
        Gizmos.DrawRay(transform.position, rightBoundary * chaseRange);
    }
}