//using System.Diagnostics;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    [Header("Scene To Load")]
    public string sceneName;

    public void LoadScene()
    {
        Debug.Log("Button clicked, trying to load scene: " + sceneName);

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty. Please assign it in the inspector.");
        }
    }
}
