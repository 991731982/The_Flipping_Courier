using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class CheckPoint : MonoBehaviour
{
    public Color activatedColor = Color.green; // Color after checkpoint activation
    public AudioClip checkpointSound; // Sound effect when checkpoint is activated
    public GameObject uiNotification; // Object used to display UI notification
    public ParticleSystem particleEffect; // Particle effect to play with shake
    public float uiShowDuration = 3f; // Duration to show UI notification
    public float particleEffectDuration = 3f; // Duration for particle effect

    [Header("UI Settings")]
    [Tooltip("If true, UI will be positioned at checkpoint. If false, UI should be Screen Space")]
    public bool useWorldSpaceUI = true;
    public Vector3 uiOffset = Vector3.up * 2f; // Offset from checkpoint position for UI
    public float worldCanvasScale = 0.01f; // Scale for world space canvas (try 0.001f to 0.1f)
    public bool setCustomRenderQueue = true; // Enable custom render queue for UI
    public int renderQueue = 3002; // Render queue value for UI elements

    [Header("Scale Shake Settings")]
    [Tooltip("Enable scale shake effect when checkpoint is reached")]
    public bool enableScaleShake = true;

    private Renderer checkpointRenderer;
    private AudioSource audioSource;
    private MMScaleShaker scaleShaker; // Reference to the MMScaleShaker component
    private bool isActivated = false; // Prevent repeated triggering

    private void Start()
    {
        // Get Renderer and AudioSource components
        checkpointRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        scaleShaker = GetComponent<MMScaleShaker>(); // Get the MMScaleShaker component

        // If UI object is set, hide it by default
        if (uiNotification != null)
        {
            uiNotification.SetActive(false);
            Debug.Log("UI Notification is set and hidden at the start.");
        }
        else
        {
            Debug.LogWarning("UI Notification is not assigned!");
        }

        // Check if particle effect is assigned
        if (particleEffect == null)
        {
            Debug.LogWarning("Particle Effect is not assigned!");
        }

        // Check if AudioSource exists
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing on the CheckPoint object!");
        }
        else
        {
            Debug.Log("AudioSource component found on CheckPoint object.");
        }

        // Check if MMScaleShaker exists
        if (scaleShaker == null)
        {
            Debug.LogWarning("MMScaleShaker component is missing on the CheckPoint object!");
        }
        else
        {
            Debug.Log("MMScaleShaker component found on CheckPoint object.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player and the checkpoint hasn't been activated yet
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true; // Set activation state
            checkPointRespawn player = other.GetComponent<checkPointRespawn>();

            if (player != null)
            {
                // Store player's current position (X and Z) but keep checkpoint's Y for customization
                Vector3 playerPosition = other.transform.position;
                Vector3 checkpointSafePosition = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);

                player.SetCheckpoint(checkpointSafePosition);
                Debug.Log("Checkpoint reached. Player X,Z saved: " + playerPosition.x + ", " + playerPosition.z + " with checkpoint Y: " + transform.position.y);

                // Reset all resettable objects
                ResetAllObjects();

                // Change checkpoint color
                ChangeCheckpointColor();

                // Play sound effect
                PlayCheckpointSound();

                // Trigger all effects independently
                TriggerScaleShake();
                ShowNotification();
                TriggerParticleEffect();
            }
            else
            {
                Debug.LogWarning("Player object does not have a checkPointRespawn component!");
            }
        }
    }

    private void TriggerScaleShake()
    {
        if (!enableScaleShake || scaleShaker == null)
        {
            return;
        }

        // Simply start the shake using the MMScaleShaker's configured settings
        scaleShaker.StartShaking();

        Debug.Log("Scale shake effect triggered on checkpoint activation!");
    }

    private void ResetAllObjects()
    {
        Debug.Log("Resetting all objects to their original positions...");
        ResettableObject[] resettableObjects = FindObjectsOfType<ResettableObject>();
        foreach (ResettableObject obj in resettableObjects)
        {
            obj.ResetPosition();
            Debug.Log("Object reset: " + obj.gameObject.name);
        }
    }

    private void ShowNotification()
    {
        if (uiNotification != null)
        {
            // Position UI at checkpoint location if using world space
            if (useWorldSpaceUI)
            {
                // Set position
                uiNotification.transform.position = transform.position + uiOffset;

                // Set proper scale for world space canvas
                uiNotification.transform.localScale = Vector3.one * worldCanvasScale;

                // Make UI face the camera
                if (Camera.main != null)
                {
                    Vector3 directionToCamera = Camera.main.transform.position - uiNotification.transform.position;
                    uiNotification.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }

                Debug.Log($"UI positioned at: {uiNotification.transform.position} with scale: {worldCanvasScale}");
            }

            // Set custom render queue for UI materials
            if (setCustomRenderQueue)
            {
                SetUIRenderQueue();
            }

            uiNotification.SetActive(true); // Show UI notification
            Debug.Log("UI Notification is displayed at checkpoint position.");
            StartCoroutine(HideNotificationAfterDelay());
        }
        else
        {
            Debug.LogWarning("UI Notification GameObject is not assigned!");
        }
    }

    private void SetUIRenderQueue()
    {
        // Get all Image and RawImage components in the UI notification
        UnityEngine.UI.Image[] images = uiNotification.GetComponentsInChildren<UnityEngine.UI.Image>();
        UnityEngine.UI.RawImage[] rawImages = uiNotification.GetComponentsInChildren<UnityEngine.UI.RawImage>();
        UnityEngine.UI.Text[] texts = uiNotification.GetComponentsInChildren<UnityEngine.UI.Text>();

        // Set render queue for Image components
        foreach (UnityEngine.UI.Image img in images)
        {
            if (img.material != null)
            {
                Material materialCopy = new Material(img.material);
                materialCopy.renderQueue = renderQueue;
                img.material = materialCopy;
                Debug.Log($"Set render queue {renderQueue} for Image: {img.name}");
            }
        }

        // Set render queue for RawImage components
        foreach (UnityEngine.UI.RawImage rawImg in rawImages)
        {
            if (rawImg.material != null)
            {
                Material materialCopy = new Material(rawImg.material);
                materialCopy.renderQueue = renderQueue;
                rawImg.material = materialCopy;
                Debug.Log($"Set render queue {renderQueue} for RawImage: {rawImg.name}");
            }
        }

        // Set render queue for Text components
        foreach (UnityEngine.UI.Text txt in texts)
        {
            if (txt.material != null)
            {
                Material materialCopy = new Material(txt.material);
                materialCopy.renderQueue = renderQueue;
                txt.material = materialCopy;
                Debug.Log($"Set render queue {renderQueue} for Text: {txt.name}");
            }
        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(uiShowDuration);
        if (uiNotification != null)
        {
            uiNotification.SetActive(false);
            Debug.Log("UI Notification is hidden.");
        }
    }

    private void TriggerParticleEffect()
    {
        if (particleEffect != null)
        {
            particleEffect.Play();
            Debug.Log("Particle effect triggered with shake!");
            StartCoroutine(StopParticleAfterDelay());
        }
        else
        {
            Debug.LogWarning("Particle Effect is not assigned!");
        }
    }

    private IEnumerator StopParticleAfterDelay()
    {
        yield return new WaitForSeconds(particleEffectDuration);
        if (particleEffect != null)
        {
            particleEffect.Stop();
            Debug.Log("Particle effect stopped.");
        }
    }

    private void ChangeCheckpointColor()
    {
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = activatedColor; // Set to activation color
            Debug.Log("Checkpoint color changed to activatedColor.");
        }
        else
        {
            Debug.LogWarning("Renderer component is missing on the CheckPoint object!");
        }
    }

    private void PlayCheckpointSound()
    {
        if (checkpointSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(checkpointSound); // Play sound effect
            Debug.Log("Checkpoint sound played!");
        }
        else
        {
            if (checkpointSound == null)
                Debug.LogWarning("AudioClip is missing! Please assign a valid AudioClip.");
            if (audioSource == null)
                Debug.LogWarning("AudioSource is missing! Please ensure the object has an AudioSource component.");
        }
    }

    // Method to manually trigger shake (for testing or external calls)
    public void ManualTriggerShake()
    {
        if (scaleShaker != null)
        {
            TriggerScaleShake();
        }
    }

    // Method to manually trigger all effects
    public void ManualTriggerAllEffects()
    {
        TriggerScaleShake();
        ShowNotification();
        TriggerParticleEffect();
    }

    // Method to stop the shake effect
    public void StopShake()
    {
        if (scaleShaker != null)
        {
            scaleShaker.Stop();
        }
    }
}