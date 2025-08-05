using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
//using static System.Net.Mime.MediaTypeNames;
//using System.Diagnostics;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    public class ButtonScenePair
    {
        public Button button;
        public string sceneName;
        public bool useStoryboard = false;
    }

    [Header("Scene Info")]
    public string mainMenuScene = "MainMenu";

    [Header("Audio Settings")]
    public AudioClip startGameSound;
    public AudioClip backgroundMusic;

    [Header("UI References")]
    public GameObject mainMenuUI;
    public UIPanelManager panelManager;

    [Header("Button Scene Mapping")]
    public List<ButtonScenePair> buttonScenePairs;

    private AudioSource audioSource;

    private void Start()
    {
        
        GameObject existingBGM = GameObject.Find("BackgroundMusic");
        if (existingBGM == null)
        {
           
            GameObject musicObject = new GameObject("BackgroundMusic");
            DontDestroyOnLoad(musicObject); 

            audioSource = musicObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;

            if (backgroundMusic != null)
            {
                audioSource.clip = backgroundMusic;
                audioSource.Play();
            }
        }
        else
        {
          
            audioSource = existingBGM.GetComponent<AudioSource>();
        }

        
        foreach (var pair in buttonScenePairs)
        {
            if (pair.button != null && !string.IsNullOrEmpty(pair.sceneName))
            {
                pair.button.onClick.AddListener(() =>
                {
                    if (pair.useStoryboard && panelManager != null)
                    {
                        StartCoroutine(PlayStoryboardThenLoadScene(pair.sceneName));
                    }
                    else
                    {
                        StartCoroutine(LoadSceneDirectly(pair.sceneName));
                    }
                });
            }
        }
    }


    private IEnumerator PlayStoryboardThenLoadScene(string sceneName)
    {
        if (startGameSound != null)
        {
            audioSource.PlayOneShot(startGameSound);
            yield return new WaitForSeconds(0.5f);
        }

        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (audioSource.isPlaying)
            audioSource.Stop();

        // Panel finish triggers scene load
        UnityAction loadSceneAction = null;
        loadSceneAction = () =>
        {
            panelManager.onStoryboardComplete.RemoveListener(loadSceneAction);
            StartCoroutine(LoadSceneDirectly(sceneName));
        };

        panelManager.onStoryboardComplete.AddListener(loadSceneAction);
        panelManager.StartStoryboard();
    }

    private IEnumerator LoadSceneDirectly(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        yield return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);
        SceneManager.UnloadSceneAsync(mainMenuScene);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DynamicGI.UpdateEnvironment();
        Debug.Log("Scene loaded: " + scene.name);
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting.");
        Application.Quit();
    }
}
