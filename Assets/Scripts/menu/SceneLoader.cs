using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string nextSceneName = "PanelMenu";
    public string playerTag = "Player"; // Make sure your player has this tag

    // Call this method manually (e.g., via a button)
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    // Quit the game
    public void QuitGame()
    {
        Debug.Log("Game is exiting.");
        Application.Quit();
    }

    // Automatically load the next scene when the player enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered trigger. Loading scene: " + nextSceneName);
            LoadNextScene();
        }
    }
}

