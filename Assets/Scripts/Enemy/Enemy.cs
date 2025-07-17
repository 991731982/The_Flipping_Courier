using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; // Reference to the player
    public float chaseRange = 10f; // The range within which the enemy will chase the player
    public float moveSpeed = 3f; // Speed at which the enemy will move towards the player
    public float stoppingDistance = 1f; // Distance at which the enemy stops before reaching the player

    private Rigidbody rb; // Reference to the Rigidbody component
    private bool isChasing = false;

    private bool canDamagePlayer = true; // Whether the enemy can damage the player

    void Start()
    {
        // Automatically find the player object with the "Player" tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found! Make sure the player has the 'Player' tag.");
        }

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if the player is within the chase range
        if (distanceToPlayer <= chaseRange && distanceToPlayer > stoppingDistance)
        {
            isChasing = true; // Start chasing
        }
        else
        {
            isChasing = false; // Stop chasing
        }

        // If the enemy is chasing the player, move towards the player
        if (isChasing)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        // Calculate the direction from the enemy to the player
        Vector3 direction = (player.position - transform.position).normalized;

        // Ignore the vertical component to keep the enemy level
        direction.y = 0;

        // Rotate the enemy to face the player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Preserve the current Y (vertical) velocity for gravity
        Vector3 velocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);

        // Apply velocity to the Rigidbody to move the enemy horizontally while maintaining gravity
        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Instantly respawn the player on contact
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
        yield return new WaitForSeconds(5f); // 5-second cooldown
        canDamagePlayer = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
