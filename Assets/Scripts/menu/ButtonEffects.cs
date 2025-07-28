using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
[RequireComponent(typeof(Selectable))]
public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
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
        // Set smaller starting scale
        //hoverScale = Vector3.one; // Final visual scale when active
        originalScale = Vector3.one * 2f; // Smaller default scale
        transform.localScale = originalScale;
        targetScale = originalScale;

        graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.color = normalColor;

        targetColor = normalColor;

        // Add outline at runtime
        if (GetComponent<Outline>() == null)
        {
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, -2);
            outline.useGraphicAlpha = true;
            outline.enabled = false;
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

        Outline outline = GetComponent<Outline>();
        if (outline) outline.enabled = true;
    }

    private void ResetVisual()
    {
        targetScale = originalScale;
        targetColor = normalColor;

        Outline outline = GetComponent<Outline>();
        if (outline) outline.enabled = false;
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
        outline.effectDistance = new Vector2(outlineWidth, outlineWidth);
        outline.useGraphicAlpha = true;
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
}