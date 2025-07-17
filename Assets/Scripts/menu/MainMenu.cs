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
        if (panelManager != null)
        {
           // StartCoroutine(BeginPanelSequence());
        }
        else
        {
            StartCoroutine(LoadSceneDirectly());
        }
    }

    /*private IEnumerator BeginPanelSequence()
    {
        // Optional: play start sound
        if (startGameSound != null)
        {
            audioSource.PlayOneShot(startGameSound);
            yield return new WaitForSeconds(0.5f);
        }

        // Stop music and hide UI
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (audioSource.isPlaying) audioSource.Stop();

        // Hook into panel finish event
        panelManager.onStoryboardComplete.RemoveAllListeners(); // Avoid stacking listeners
        panelManager.onStoryboardComplete.AddListener(() =>
        {
            StartCoroutine(LoadScene());
        });

        panelManager.StartStoryboard();
    }*/

    private IEnumerator LoadSceneDirectly()
    {
        // Optional: play start sound
        if (startGameSound != null)
        {
            //audioSource.PlayOneShot(startGameSound);
            yield return new WaitForSeconds(0.5f);
        }

        if (audioSource.isPlaying) audioSource.Stop();

        if (mainMenuUI != null) mainMenuUI.SetActive(false);

        yield return LoadScene();
    }

    private IEnumerator LoadScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(gameScene, LoadSceneMode.Additive);
        yield return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameScene)
        {
            SceneManager.SetActiveScene(scene);
            SceneManager.UnloadSceneAsync(mainMenuScene);
            DynamicGI.UpdateEnvironment();
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
