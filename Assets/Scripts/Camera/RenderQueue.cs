using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RenderQueue : MonoBehaviour
{
    public string layerName = "NoFog";         // Still optional if needed
    public int customRenderQueue = 3002;
    public bool applyOnStart = true;
    public bool applyToThisObjectOnly = true;  //Apply directly to this GameObject & children

    void Start()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying && !applyOnStart) return;
#endif

        if (applyOnStart)
        {
            ApplyRenderQueue();
        }
    }

    [ContextMenu("Apply Render Queue")]
    public void ApplyRenderQueue()
    {
        if (applyToThisObjectOnly)
        {
            ApplyRenderQueueToAllChildren(gameObject);
            Debug.Log($"Applied renderQueue {customRenderQueue} to this GameObject and its children.");
        }
        else
        {
            ApplyToLayerObjects();
        }
    }

    private void ApplyToLayerObjects()
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

    private void ApplyRenderQueueToAllChildren(GameObject root)
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
                    newMats[i] = new Material(renderer.sharedMaterials[i]);
                    newMats[i].renderQueue = customRenderQueue;
                }
            }

            renderer.materials = newMats;
        }
    }
}