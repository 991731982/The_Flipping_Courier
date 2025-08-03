using UnityEngine;
using UnityEngine.UI;

public class PlanetHover : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool rotateByDefault = false;
    public float defaultRotationSpeed = 15f;
    public float hoverRotationSpeed = 30f;

    [Header("Hover Scale")]
    public float scaleMultiplier = 1.2f;

    [Header("UI Settings")]
    public string planetDescription;
    public GameObject uiPanel;
    public UnityEngine.UI.Text uiText;

    [Header("Hover Image")]
    public GameObject hoverImage; 

    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;

        if (uiPanel != null)
            uiPanel.SetActive(false);

        if (hoverImage != null)
            hoverImage.SetActive(false);
    }

    void Update()
    {
        float currentRotationSpeed = rotateByDefault ? defaultRotationSpeed : 0f;
        if (isHovered)
            currentRotationSpeed = hoverRotationSpeed;

        if (currentRotationSpeed != 0f)
        {
            transform.Rotate(Vector3.up * currentRotationSpeed * Time.deltaTime);
        }
    }

    void OnMouseEnter()
    {
        isHovered = true;
        transform.localScale = originalScale * scaleMultiplier;

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            uiText.text = planetDescription;
        }

        if (hoverImage != null)
            hoverImage.SetActive(true); 
    }

    void OnMouseExit()
    {
        isHovered = false;
        transform.localScale = originalScale;

        if (uiPanel != null)
            uiPanel.SetActive(false);

        if (hoverImage != null)
            hoverImage.SetActive(false); 
    }
}
