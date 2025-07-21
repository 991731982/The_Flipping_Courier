using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class RenderQueue : MonoBehaviour
{
    public string layerName = "NoFog";
    public int customRenderQueue = 3002;
    public bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart)
        {
            ApplyToScene();
        }
    }

    [ContextMenu("Apply Render Queue to Layer Objects")]
    public void ApplyToScene()
    {
        int targetLayer = LayerMask.NameToLayer(layerName);

        if (targetLayer == -1)
        {
            Debug.LogWarning($"Layer \"{layerName}\" not found!");
            return;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == targetLayer)
            {
                ApplyRenderQueueToAllChildren(obj);
            }
        }

        Debug.Log($"Applied renderQueue {customRenderQueue} to all objects on layer \"{layerName}\".");
    }

    void ApplyRenderQueueToAllChildren(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterials == null)
                continue;

            Material[] newMats = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] != null)
                {
                    // Clone the material to avoid shared instance edits
                    newMats[i] = new Material(renderer.sharedMaterials[i]);
                    newMats[i].renderQueue = customRenderQueue;
                }
            }

            renderer.materials = newMats;
        }
    }
}