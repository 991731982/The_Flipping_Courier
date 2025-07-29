using UnityEngine;
#if UNITY_EDITOR
public class TestingTeleporter : MonoBehaviour
{
    [Header("Teleport Targets")]
    public Transform[] teleportPositions; // Assign positions in Inspector

    void Update()
    {
        for (int i = 0; i < teleportPositions.Length && i < 9; i++) // 1-9 keys
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (teleportPositions[i] != null)
                {
                    transform.position = teleportPositions[i].position;
                    Debug.Log($"Teleported to position {i + 1}: {teleportPositions[i].name}", teleportPositions[i]);
                }
                else
                {
                    Debug.LogWarning($"Teleport position {i + 1} is not assigned.");
                }
            }
        }
    }
}
#endif