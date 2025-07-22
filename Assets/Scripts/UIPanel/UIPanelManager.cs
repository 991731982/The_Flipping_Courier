using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
public class UIPanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelData
    {
        public Sprite panelImage;
        public float displayTime = 3f;
    }

    [Header("Optional Default Panel Set")]
    public List<PanelData> defaultPanels = new List<PanelData>();
    private List<PanelData> currentPanels;

    public Image panelDisplay;
    public GameObject panelCanvas;
    public UnityEvent onStoryboardComplete;

    public bool useTimer = true;
    public bool useConfirmKey = false;
    public KeyCode confirmKey = KeyCode.Space;

    private int currentPanelIndex = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    public string sceneToLoad;

    private CubeCharacterController playerController;

    void Update()
    {
        if (!isPlaying) return;

        // Allow manual panel progression
        if (useConfirmKey && Input.GetKeyDown(confirmKey))
        {
            NextPanel();
            return;
        }

        // Auto advance using timer
        if (useTimer && currentPanels != null)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= currentPanels[currentPanelIndex].displayTime)
            {
                NextPanel();
            }
        }
    }

    public void StartStoryboard()
    {
        StartStoryboard(defaultPanels);
    }

    public void StartStoryboard(List<PanelData> panelsToPlay)
    {
        if (panelsToPlay == null || panelsToPlay.Count == 0)
        {
            Debug.LogWarning("No panels to play.");
            return;
        }

        currentPanels = panelsToPlay;

        /*playerController = FindAnyObjectByType<CubeCharacterController>();
        if (playerController != null)
            playerController.DisableInput();*/

        panelCanvas.SetActive(true);
        currentPanelIndex = 0;
        isPlaying = true;
        ShowCurrentPanel();
    }

    void ShowCurrentPanel()
    {
        if (currentPanels == null || currentPanelIndex >= currentPanels.Count) return;

        panelDisplay.sprite = currentPanels[currentPanelIndex].panelImage;
        timer = 0f;
    }

    void NextPanel()
    {
        currentPanelIndex++;
        if (currentPanels == null || currentPanelIndex >= currentPanels.Count)
        {
            EndStoryboard();
        }
        else
        {
            ShowCurrentPanel();
        }
    }

    void EndStoryboard()
    {
        isPlaying = false;
        panelCanvas.SetActive(false);

        /*if (playerController != null)
            playerController.EnableInput();*/

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAfterPanels());
        }

        onStoryboardComplete?.Invoke();
    }

    private IEnumerator LoadSceneAfterPanels()
    {
        yield return null;
        SceneManager.LoadScene(sceneToLoad);
    }
}