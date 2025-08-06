using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetClickToScene : MonoBehaviour
{
    [Tooltip("点击后要切换的场景名")]
    public string targetSceneName;

    private void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
          
            GameObject bgm = GameObject.Find("BackgroundMusic");
            if (bgm != null)
            {
                Destroy(bgm);
            }

            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("not assign scene name!");
        }
    }
}
