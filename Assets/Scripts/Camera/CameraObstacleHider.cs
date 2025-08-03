using UnityEngine;
using System.Collections.Generic;

public class CameraObstacleHandler : MonoBehaviour
{
    public Transform player;              // Reference to the player object
    public LayerMask obstacleLayer;       // LayerMask used to detect obstacles with raycast

    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    private List<GameObject> currentObstacles = new List<GameObject>();

    void Update()
    {
        // Cast a ray from the camera to the player
        Vector3 direction = player.position - transform.position;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, direction.magnitude, obstacleLayer);

        // Restore previously transparent obstacles to their original state
        for (int i = currentObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = currentObstacles[i];

            // If the object no longer exists, remove it from the list
            if (obstacle == null)
            {
                currentObstacles.RemoveAt(i);
                continue;
            }

            ResetObstacle(obstacle);
        }

        // Process all obstacles currently between the camera and the player
        foreach (RaycastHit hit in hits)
        {
            GameObject obstacle = hit.collider.gameObject;

            // Avoid applying transparency more than once
            if (!currentObstacles.Contains(obstacle))
            {
                SetObstacleTransparent(obstacle);
                currentObstacles.Add(obstacle);
            }
        }
    }

    // Makes the obstacle semi-transparent
    void SetObstacleTransparent(GameObject obstacle)
    {
        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Save the original material (only once)
            if (!originalMaterials.ContainsKey(obstacle))
            {
                originalMaterials[obstacle] = renderer.material;
            }

            // Create a new material instance for transparency
            Material transparentMaterial = new Material(renderer.material);

            // Set rendering mode to transparent
            transparentMaterial.SetFloat("_Mode", 2);
            transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMaterial.SetInt("_ZWrite", 1); // Depth writing enabled to avoid weird overlaps
            transparentMaterial.DisableKeyword("_ALPHATEST_ON");
            transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
            transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMaterial.renderQueue = 3000;

            // Set alpha to 30% transparency
            Color color = transparentMaterial.color;
            color.a = 0.3f;
            transparentMaterial.color = color;

            // Apply the transparent material
            renderer.material = transparentMaterial;
        }
    }

    // Restores the obstacle’s original material
    void ResetObstacle(GameObject obstacle)
    {
        if (obstacle == null) return;

        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null && originalMaterials.ContainsKey(obstacle))
        {
            // Restore original material
            renderer.material = originalMaterials[obstacle];
            originalMaterials.Remove(obstacle); // Remove from dictionary
        }
    }
}