using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class CheckPoint : MonoBehaviour
{
    public Color activatedColor = Color.green; // Color after checkpoint activation
    public AudioClip checkpointSound; // Sound effect when checkpoint is activated
    public GameObject uiNotification; // Object used to display UI notification

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
                player.SetCheckpoint(transform.position);
                Debug.Log("Checkpoint reached at position: " + transform.position);

                // Reset all resettable objects
                ResetAllObjects();

                // Show notification UI
                ShowNotification();

                // Change checkpoint color
                ChangeCheckpointColor();

                // Play sound effect
                PlayCheckpointSound();

                // Trigger scale shake effect
                TriggerScaleShake();
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
            uiNotification.SetActive(true); // Show UI notification
            Debug.Log("UI Notification is displayed.");
            Invoke("HideNotification", 2f); // Hide UI after 2 seconds
        }
        else
        {
            Debug.LogWarning("UI Notification GameObject is not assigned!");
        }
    }

    private void HideNotification()
    {
        if (uiNotification != null)
        {
            uiNotification.SetActive(false);
            Debug.Log("UI Notification is hidden.");
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

    // Method to stop the shake effect
    public void StopShake()
    {
        if (scaleShaker != null)
        {
            scaleShaker.Stop();
        }
    }
}