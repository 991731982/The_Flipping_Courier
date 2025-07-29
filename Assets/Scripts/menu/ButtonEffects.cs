using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
[RequireComponent(typeof(Selectable))]
public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    [Header("Feature Toggles")]
    public bool enableOutline = true;
    public bool forceDefaultSize = true;

    [Header("Animation Settings")]
    public Vector3 hoverScale = new Vector3(2.7f, 2.7f, 1f);
    public float scaleSpeed = 10f;

    [Header("Color Feedback")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public float colorLerpSpeed = 10f;

    [Header("Outline Glow Settings")]
    public Color outlineHoverColor = new Color(1f, 1f, 0.5f, 1f); // soft yellow glow
    public float outlineWidth = 1.5f;

    [Header("Audio Feedback")]
    public AudioClip hoverSFX;
    public AudioClip clickSFX;
    public AudioSource audioSource;

    private Vector3 originalScale;
    private Vector3 sceneScale; // Store the original scale from scene
    private Vector3 targetScale;
    private Color targetColor;
    private Graphic graphic;
    private Outline outline;

    private bool isHovered = false;
    private bool isSelected = false;
    private bool lostFocus = false;

    private static ButtonEffects currentActiveButton;

    void Start()
    {
        // Store the original scale from the scene
        sceneScale = transform.localScale;

        // Set scale based on forceDefaultSize toggle
        if (forceDefaultSize)
        {
            originalScale = Vector3.one * 2f; // Smaller default scale
            transform.localScale = originalScale;
        }
        else
        {
            originalScale = sceneScale; // Use the scale set in scene
        }

        targetScale = originalScale;

        graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.color = normalColor;

        targetColor = normalColor;

        // Setup outline based on toggle
        if (enableOutline)
        {
            SetupOutline();
        }
        else
        {
            // Remove outline if it exists and toggle is off
            Outline existingOutline = GetComponent<Outline>();
            if (existingOutline != null)
            {
                DestroyImmediate(existingOutline);
            }
        }
    }

    void Update()
    {
        // Smooth transitions
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
        if (graphic != null)
            graphic.color = Color.Lerp(graphic.color, targetColor, Time.unscaledDeltaTime * colorLerpSpeed);

        // Auto-restore keyboard navigation if none selected
        if (EventSystem.current.currentSelectedGameObject == null && !lostFocus)
        {
            lostFocus = true;
            Invoke(nameof(RestoreKeyboardNavigation), 0.1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        lostFocus = false;

        if (currentActiveButton != null && currentActiveButton != this)
            currentActiveButton.ClearEffects();

        currentActiveButton = this;
        isHovered = true;
        isSelected = false;

        ApplyActiveVisual();

        if (hoverSFX && audioSource)
            audioSource.PlayOneShot(hoverSFX);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentActiveButton == this)
            currentActiveButton = null;

        isHovered = false;
        if (!isSelected)
            ResetVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSFX && audioSource)
            audioSource.PlayOneShot(clickSFX);

        GetComponent<Selectable>().Select();
    }

    public void OnSelect(BaseEventData eventData)
    {
        lostFocus = false;

        if (currentActiveButton != null && currentActiveButton != this)
            currentActiveButton.ClearEffects();

        currentActiveButton = this;
        isSelected = true;
        isHovered = false;

        ApplyActiveVisual();

        if (hoverSFX && audioSource)
            audioSource.PlayOneShot(hoverSFX);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (currentActiveButton == this)
            currentActiveButton = null;

        isSelected = false;
        if (!isHovered)
            ResetVisual();
    }

    private void ApplyActiveVisual()
    {
        targetScale = hoverScale;
        targetColor = hoverColor;

        if (enableOutline)
        {
            EnableOutline();
        }
    }

    private void ResetVisual()
    {
        targetScale = originalScale;
        targetColor = normalColor;

        if (enableOutline)
        {
            DisableOutline();
        }
    }

    private void ClearEffects()
    {
        isHovered = false;
        isSelected = false;
        ResetVisual();
    }

    void OnDestroy()
    {
        if (currentActiveButton == this)
            currentActiveButton = null;
    }

    private void RestoreKeyboardNavigation()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            Selectable[] allSelectables = FindObjectsByType<Selectable>(FindObjectsSortMode.None);
            foreach (var sel in allSelectables)
            {
                if (sel.interactable && sel.navigation.mode != Navigation.Mode.None)
                {
                    EventSystem.current.SetSelectedGameObject(sel.gameObject);
                    lostFocus = false;
                    return;
                }
            }
        }
    }

    private void SetupOutline()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectColor = outlineHoverColor;
        outline.effectDistance = new Vector2(outlineWidth, -outlineWidth);
        outline.useGraphicAlpha = true;
        outline.enabled = false; // Start disabled
    }

    private void EnableOutline()
    {
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    private void DisableOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    // Public methods to toggle features at runtime
    public void ToggleOutline(bool enable)
    {
        enableOutline = enable;

        if (enableOutline)
        {
            SetupOutline();
            if (isHovered || isSelected)
                EnableOutline();
        }
        else
        {
            DisableOutline();
            if (outline != null)
            {
                DestroyImmediate(outline);
                outline = null;
            }
        }
    }

    public void ToggleForceDefaultSize(bool force)
    {
        forceDefaultSize = force;

        if (forceDefaultSize)
        {
            originalScale = Vector3.one * 2f;
        }
        else
        {
            originalScale = sceneScale;
        }

        // Update current scale if not active
        if (!isHovered && !isSelected)
        {
            targetScale = originalScale;
        }
    }
}