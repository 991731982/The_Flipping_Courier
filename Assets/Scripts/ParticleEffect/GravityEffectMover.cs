using UnityEngine;

public class FloatingParticleOnScreen : MonoBehaviour
{
    public GravityController gravityController;

    [Header("Screen Anchoring")]
    public Vector2 screenAnchor = new Vector2(0.5f, 0.8f);
    public float topYOffset = 0.1f;
    public float bottomYOffset = -0.1f;
    public float depth = 10f;

    [Header("Vertical Movement Settings")]
    public float verticalMoveSpeed = 5f;

    [Header("Eye Look Rotation (X axis)")]
    public float lookUpAngle = 30f;    // 朝上看时仰头
    public float lookDownAngle = -30f; // 朝下看时俯头
    public float rotationSpeed = 5f;

    private float currentYOffset;
    private float targetYOffset;

    private float currentXRotation;
    private float targetXRotation;

    void Start()
    {
        bool flipped = gravityController.gravityFlipped;

        currentYOffset = flipped ? topYOffset : bottomYOffset;
        currentXRotation = flipped ? lookUpAngle : lookDownAngle;
        transform.rotation = Quaternion.Euler(currentXRotation, 0f, 0f);
    }

    void Update()
    {
        if (gravityController == null || Camera.main == null)
            return;

        bool flipped = gravityController.gravityFlipped;

        targetYOffset = flipped ? topYOffset : bottomYOffset;
        targetXRotation = flipped ? lookUpAngle : lookDownAngle;

        currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset, Time.deltaTime * verticalMoveSpeed);
        currentXRotation = Mathf.LerpAngle(currentXRotation, targetXRotation, Time.deltaTime * rotationSpeed);
        transform.rotation = Quaternion.Euler(currentXRotation, 0f, 0f);

        Vector3 screenPos = new Vector3(
            Screen.width * screenAnchor.x,
            Screen.height * (screenAnchor.y + currentYOffset),
            depth
        );

        transform.position = Camera.main.ScreenToWorldPoint(screenPos);
    }
}
