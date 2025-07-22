using TMPro;
using UnityEngine;

public class GraveStateDebugUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI stateDisplayText;
    public TextMeshProUGUI instructionsText;

    [Header("Target Object")]
    public GravState targetGravState; // Drag the object you want to monitor

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = Camera.FindFirstObjectByType<Camera>();
        }

        if (instructionsText != null)
        {
            instructionsText.text = "Left Click: Heavy Beam (Red)\nRight Click: Light Beam (Cyan)\n\nState Cycle:\nNormal -> Heavy/Light -> Normal";
        }
    }

    void Update()
    {
        UpdateStateDisplay();

        // Optional: Automatically find target under mouse cursor
        if (targetGravState == null && Input.GetMouseButtonDown(0))
        {
            FindTargetUnderMouse();
        }
    }

    void UpdateStateDisplay()
    {
        if (stateDisplayText == null) return;

        if (targetGravState != null)
        {
            string stateColor = GetStateColor(targetGravState.CurrentState);
            string stateName = targetGravState.CurrentState.ToString();

            stateDisplayText.text = "Target: " + targetGravState.gameObject.name + "\n" +
                                    "State: <color=" + stateColor + ">" + stateName + "</color>";

            Rigidbody rb = targetGravState.GetComponent<Rigidbody>();
            if (rb != null)
            {
                stateDisplayText.text += "\nMass: " + rb.mass.ToString("F1") + "\nDrag: " + rb.linearDamping.ToString("F1");
            }
        }
        else
        {
            stateDisplayText.text = "No target selected\nLeft-click on an object to monitor it";
        }
    }

    string GetStateColor(GravState.GravityState state)
    {
        switch (state)
        {
            case GravState.GravityState.Normal: return "white";
            case GravState.GravityState.Heavy: return "red";
            case GravState.GravityState.Light: return "cyan";
            default: return "white";
        }
    }

    void FindTargetUnderMouse()
    {
        if (playerCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GravState gravState = hit.collider.GetComponent<GravState>();
            if (gravState != null)
            {
                targetGravState = gravState;
                Debug.Log("Now monitoring: " + targetGravState.gameObject.name);
            }
        }
    }

    // Public method to set target (can be called from other scripts)
    public void SetTarget(GravState newTarget)
    {
        targetGravState = newTarget;
    }
}