using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public string mainMenuScene = "MainMenu";
    public string gameScene = "Protect-Level1";

    public GameObject mainMenuUI;
    public AudioClip startGameSound;
    public AudioClip backgroundMusic;
    public UIPanelManager panelManager;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
    }

    public void LoadGameScene()
    {
        StartCoroutine(BeginPanelSequence());
    }

    private IEnumerator BeginPanelSequence()
    {
        // Optional: play button click SFX
        if (startGameSound != null)
        {
            audioSource.PlayOneShot(startGameSound);
            yield return new WaitForSeconds(0.5f); // Wait for SFX
        }

        // Hide main menu visuals
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        // Stop background music
        if (audioSource.isPlaying)
            audioSource.Stop();

        // Hook scene loading to panel completion
        panelManager.onStoryboardComplete.AddListener(() =>
        {
            StartCoroutine(LoadSceneAfterPanels());
        });

        // Start panel sequence
        panelManager.StartStoryboard();
    }

    private IEnumerator LoadSceneAfterPanels()
    {
        // Optional fade-out, loading screen, etc. can go here

        // Begin additive scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(gameScene, LoadSceneMode.Additive);

        yield return null; // Wait one frame
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameScene)
        {
            // Make new scene the active one
            SceneManager.SetActiveScene(scene);

            // Unload the main menu scene
            SceneManager.UnloadSceneAsync(mainMenuScene);

            // Update lighting for baked GI (optional)
            DynamicGI.UpdateEnvironment();

            // Cleanup callback
            SceneManager.sceneLoaded -= OnSceneLoaded;

            Debug.Log("Game scene loaded and main menu scene unloaded.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting.");
        Application.Quit();
    }
}