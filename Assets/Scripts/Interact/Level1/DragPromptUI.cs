using UnityEngine;
using UnityEngine.UI;

public class DragPromptUI : MonoBehaviour
{
    [Header("Prompt Settings")]
    public GameObject promptSprite; // Assign your sprite GameObject here
    public float showDistance = 5f; // Distance to show prompt
    public Transform player; // Player reference

    void Update()
    {
        if (player == null || promptSprite == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Show/hide prompt based on distance
        if (distance <= showDistance && !promptSprite.activeInHierarchy)
        {
            promptSprite.SetActive(true);
        }
        else if (distance > showDistance && promptSprite.activeInHierarchy)
        {
            promptSprite.SetActive(false);
        }
    }
}