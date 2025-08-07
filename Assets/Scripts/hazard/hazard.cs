using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hazard : MonoBehaviour


{

    [Header("Audio Settings")]
    public AudioClip hitSoundEffect;            // 拖入音效 clip
    private AudioSource audioSource;


    [Header("UI Settings")]
    public GameObject[] lifeIcons;              // UI life icons

    [Header("Effect Settings")]
    public GameObject hitParticleEffect;        // Particle effect prefab to play on hit

    [Header("Damage Settings")]
    [Tooltip("Use trigger for damage detection (default for plants). Uncheck for collision-based damage (water hazards).")]
    public bool useTriggerForDamage = true;     // Default to trigger (plants), false for collision (water)

    [Header("Invincibility Settings")]
    [Tooltip("Time in seconds player is invincible after taking damage")]
    public float invincibilityDuration = 1.5f;

    [Tooltip("How fast the player flashes during invincibility (flashes per second)")]
    public float flashRate = 8f;

    [Tooltip("Opacity during flash (0 = invisible, 1 = fully visible)")]
    [Range(0f, 1f)]
    public float flashOpacity = 0.3f;

    private static Dictionary<GameObject, int> playerHitCounts = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, bool> playerInvincible = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, Coroutine> playerFlashCoroutines = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, bool> playerInTrigger = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> playerLastHitTime = new Dictionary<GameObject, float>();
    private const int maxHits = 3;

    //public int health
    //{
    //    get
    //    {
    //        return _health;
    //    }
    //    set
    //    {
    //        Debug.Log("Changing the health - " + value.ToString());
    //        Debug.Log("Triggered from object[]");
    //        _health = value;
    //    }
    //}
    //private int _health = 3;
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Handle continuous damage for players in triggers
        if (useTriggerForDamage)
        {
            foreach (var kvp in playerInTrigger)
            {
                GameObject player = kvp.Key;
                bool inTrigger = kvp.Value;

                if (inTrigger && player != null)
                {
                    // Check if enough time has passed since last hit and player is not invincible
                    if (!IsPlayerInvincible(player))
                    {
                        float lastHitTime = playerLastHitTime.ContainsKey(player) ? playerLastHitTime[player] : 0f;
                        if (Time.time - lastHitTime >= invincibilityDuration)
                        {
                            HandleHazardInteraction(player);
                        }
                    }
                }
            }
        }
    }

    // Trigger-based damage detection (for plants with separate mesh/trigger colliders)
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerForDamage) return; // Skip if using collision-based damage

        if (other.CompareTag("Player"))
        {
            playerInTrigger[other.gameObject] = true;
            HandleHazardInteraction(other.gameObject);
        }
        else
        {
            HandleHazardInteraction(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useTriggerForDamage) return; // Skip if using collision-based damage

        if (other.CompareTag("Player"))
        {
            playerInTrigger[other.gameObject] = false;
        }
    }

    // Collision-based damage detection (for water hazards where collider does both physics + damage)
    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerForDamage) return; // Skip if using trigger-based damage

        HandleHazardInteraction(collision.gameObject);
    }

    // Unified hazard interaction logic
    private void HandleHazardInteraction(GameObject hitObject)
    {
        if (hitObject.CompareTag("Enemy"))
        {
            Destroy(hitObject);
        }
        else if (hitObject.CompareTag("Player"))
        {
            // Check if player is currently invincible
            if (IsPlayerInvincible(hitObject))
            {
                UnityEngine.Debug.Log("Player is invincible, ignoring damage");
                return;
            }

            GameObject playerObj = hitObject;

            // Play particle effect
            PlayHitEffect(playerObj);

            // Handle player damage and lives
            ProcessPlayerDamage(playerObj);
        }
    }

    private bool IsPlayerInvincible(GameObject player)
    {
        return playerInvincible.ContainsKey(player) && playerInvincible[player];
    }

    private void PlayHitEffect(GameObject playerObj)
    {
        if (hitParticleEffect != null)
        {
            UnityEngine.Debug.Log("Playing hit effect");
            Vector3 spawnPos = playerObj.transform.position + Vector3.up * 1f;
            GameObject particle = Instantiate(hitParticleEffect, spawnPos, Quaternion.identity);

            ParticleSystem ps = particle.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                UnityEngine.Debug.Log("Particle system playing successfully");
            }
            else
            {
                UnityEngine.Debug.Log("No ParticleSystem component found in prefab or children!");
            }

            Destroy(particle, 3f);
        }
        else
        {
            UnityEngine.Debug.Log("hitParticleEffect prefab not assigned!");
        }

        if (hitSoundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSoundEffect);
            UnityEngine.Debug.Log("Hit sound effect played.");
        }
        else
        {
            UnityEngine.Debug.Log("Hit sound not assigned or AudioSource missing.");
        }
    }

    private void ProcessPlayerDamage(GameObject playerObj)
    {
        // Initialize hit count for new players
        if (!playerHitCounts.ContainsKey(playerObj))
        {
            playerHitCounts[playerObj] = 0;
        }

        // Record the time of this hit
        playerLastHitTime[playerObj] = Time.time;

        // Increment hit count
        playerHitCounts[playerObj]++;
        int hits = playerHitCounts[playerObj];

        UnityEngine.Debug.Log($"Player hit hazard: {hits} time(s)");

        // Start invincibility frames and visual feedback
        StartInvincibilityFrames(playerObj);

        // Update UI to show remaining lives
        UpdateLifeUI(maxHits - hits);

        // Check if player reached max hits
        if (hits >= maxHits)
        {
            checkPointRespawn player = playerObj.GetComponent<checkPointRespawn>();
            if (player != null)
            {
                UnityEngine.Debug.Log("Player reached max hits, respawning...");

                // Make sure renderer is visible before respawning
                RestorePlayerVisibility(playerObj);

                player.RespawnAtCheckpoint();

                // Reset hit count and UI after respawn
                playerHitCounts[playerObj] = 0;
                UpdateLifeUI(maxHits);

                // Clear invincibility after respawn
                ClearPlayerInvincibility(playerObj);
            }
        }
    }

    private void StartInvincibilityFrames(GameObject player)
    {
        // Set player as invincible immediately
        playerInvincible[player] = true;

        // Stop any existing flash coroutine for this player
        if (playerFlashCoroutines.ContainsKey(player) && playerFlashCoroutines[player] != null)
        {
            StopCoroutine(playerFlashCoroutines[player]);
        }

        // Start new flash coroutine
        playerFlashCoroutines[player] = StartCoroutine(FlashPlayer(player));

        UnityEngine.Debug.Log($"Player invincible for {invincibilityDuration} seconds");
    }

    private IEnumerator FlashPlayer(GameObject player)
    {
        // Get all renderers for visual feedback
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            UnityEngine.Debug.Log("No renderers found on player - skipping flash effect");
            yield return new WaitForSeconds(invincibilityDuration);
            ClearPlayerInvincibility(player);
            yield break;
        }

        float elapsed = 0f;
        float flashInterval = 1f / (flashRate * 2f); // Multiply by 2 because we flash on/off
        bool isVisible = true;

        while (elapsed < invincibilityDuration)
        {
            elapsed += flashInterval;
            isVisible = !isVisible;

            // Apply visibility to all renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = isVisible ? true : false;

                    // Alternative method using material alpha if renderer.enabled doesn't work well
                    /*
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = isVisible ? 1f : flashOpacity;
                            mat.color = color;
                        }
                    }
                    */
                }
            }

            yield return new WaitForSeconds(flashInterval);
        }

        // Ensure player is fully visible when invincibility ends
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;

                // Reset alpha if using material method
                /*
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = 1f;
                        mat.color = color;
                    }
                }
                */
            }
        }

        // Clear invincibility
        ClearPlayerInvincibility(player);
        UnityEngine.Debug.Log("Player invincibility ended");
    }

    private void ClearPlayerInvincibility(GameObject player)
    {
        if (playerInvincible.ContainsKey(player))
        {
            playerInvincible[player] = false;
        }

        if (playerFlashCoroutines.ContainsKey(player))
        {
            if (playerFlashCoroutines[player] != null)
            {
                StopCoroutine(playerFlashCoroutines[player]);
            }
            playerFlashCoroutines[player] = null;
        }

        // Make sure player is visible when clearing invincibility
        RestorePlayerVisibility(player);
    }

    private void RestorePlayerVisibility(GameObject player)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    private void UpdateLifeUI(int livesLeft)
    {
        UnityEngine.Debug.Log($"Updating life UI: lives left = {livesLeft}");

        int totalIcons = lifeIcons.Length;
        for (int i = 0; i < totalIcons; i++)
        {
            // Reverse the index so icons disappear from right to left
            int reversedIndex = totalIcons - 1 - i;
            lifeIcons[reversedIndex].SetActive(i < livesLeft);
        }
    }

    // Helper method to reset specific player's hit count (useful for testing or special cases)
    public void ResetPlayerHitCount(GameObject player)
    {
        if (playerHitCounts.ContainsKey(player))
        {
            playerHitCounts[player] = 0;
            UpdateLifeUI(maxHits);
            UnityEngine.Debug.Log($"Reset hit count for player: {player.name}");
        }

        // Also clear invincibility
        ClearPlayerInvincibility(player);
    }

    // Helper method to get current hit count for a player
    public int GetPlayerHitCount(GameObject player)
    {
        return playerHitCounts.ContainsKey(player) ? playerHitCounts[player] : 0;
    }

    // Helper method to check if player is currently invincible (for other scripts)
    public bool IsPlayerCurrentlyInvincible(GameObject player)
    {
        return IsPlayerInvincible(player);
    }

    // Clean up coroutines when object is destroyed
    private void OnDestroy()
    {
        foreach (var coroutine in playerFlashCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        playerFlashCoroutines.Clear();
    }
}