using UnityEngine;
using System.Collections;
public class EyeballMovement : MonoBehaviour
{
    [Header("Movement Targets")]
    public Transform startPoint;              // Inside the crate
    public Transform endPoint;                // Eye socket attachment point
    public Transform playerTransform;         // Reference to player for looking

    [Header("Eyelid Components")]
    public Transform upperEyelid;             // Upper eyelid transform
    public Transform lowerEyelid;             // Lower eyelid transform

    [Header("Animation Phases")]
    [Range(0.1f, 3f)] public float riseDelay = 0.5f;           // Delay before starting to rise
    [Range(0.5f, 5f)] public float riseDuration = 2f;          // Time to rise up
    [Range(0.5f, 3f)] public float lookAtPlayerDuration = 1.5f; // Time spent looking at player
    [Range(0.5f, 5f)] public float moveToSocketDuration = 2.5f; // Time to move to socket
    [Range(0.1f, 2f)] public float eyeCloseDuration = 1f;      // Time for eyelids to close

    [Header("Movement Curves")]
    public AnimationCurve riseEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve moveToSocketEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve eyeCloseEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Rise Settings")]
    [Range(1f, 15f)] public float riseHeight = 8f;             // How high above start point to rise
    [Range(0.1f, 2f)] public float floatAmplitude = 0.3f;      // Floating bob amplitude
    [Range(1f, 10f)] public float floatFrequency = 2f;         // Floating bob frequency

    [Header("Look At Player Settings")]
    [Range(0.1f, 5f)] public float lookRotationSpeed = 3f;     // How fast eyeball rotates to look at player
    [Range(0.1f, 2f)] public float menacingBobAmplitude = 0.2f; // Subtle menacing movement
    [Range(1f, 8f)] public float menacingBobFrequency = 3f;     // Frequency of menacing movement

    [Header("Eyelid Animation")]
    [Range(0f, 1f)] public float eyelidOpenAmount = 0.8f;      // How much eyelids open (0 = closed, 1 = fully open)
    [Range(0.1f, 2f)] public float eyelidBlinkSpeed = 4f;      // Speed of blinking during rise
    public Vector3 upperEyelidClosedRotation = new Vector3(45f, 0f, 0f); // Closed rotation for upper eyelid
    public Vector3 lowerEyelidClosedRotation = new Vector3(-45f, 0f, 0f); // Closed rotation for lower eyelid

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip riseSound;               // Sound when eyeball starts rising
    public AudioClip attachSound;             // Sound when eyeball attaches to socket
    public AudioClip menacingSound;           // Ambient menacing sound during look phase

    [Header("Effects")]
    public ParticleSystem dustParticles;      // Dust particles when rising from crate
    public ParticleSystem attachParticles;    // Particles when attaching to socket

    [Header("Debug")]
    public bool debugMode = true;
    public bool showGizmos = true;

    // Private variables
    private bool isAnimating = false;
    private Vector3 initialEyeballRotation;
    private Vector3 initialUpperEyelidRotation;
    private Vector3 initialLowerEyelidRotation;
    private Vector3 floatingPosition;
    private Coroutine currentAnimation;

    // Animation states
    public enum AnimationPhase
    {
        Waiting,
        Rising,
        LookingAtPlayer,
        MovingToSocket,
        Attaching,
        Complete
    }
    private AnimationPhase currentPhase = AnimationPhase.Waiting;

    private void Start()
    {
        // Store initial rotations
        initialEyeballRotation = transform.eulerAngles;
        if (upperEyelid != null)
        {
            initialUpperEyelidRotation = upperEyelid.eulerAngles;
        }
        if (lowerEyelid != null)
        {
            initialLowerEyelidRotation = lowerEyelid.eulerAngles;
        }

        // Start at the hidden position inside the crate
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }

        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // Initially close eyelids
        //CloseEyelids(true);

        if (debugMode)
        {
            Debug.Log("EyeballMovement: Initialized and ready for animation");
        }
    }

    private void Update()
    {
        // Add subtle floating movement during certain phases
        if (currentPhase == AnimationPhase.LookingAtPlayer || currentPhase == AnimationPhase.Rising)
        {
            AddFloatingMovement();
        }
    }

    private void AddFloatingMovement()
    {
        float amplitude = currentPhase == AnimationPhase.LookingAtPlayer ? menacingBobAmplitude : floatAmplitude;
        float frequency = currentPhase == AnimationPhase.LookingAtPlayer ? menacingBobFrequency : floatFrequency;

        Vector3 bobOffset = new Vector3(
            Mathf.Sin(Time.time * frequency) * amplitude * 0.3f,           // Slight horizontal sway
            Mathf.Sin(Time.time * frequency * 1.3f) * amplitude,          // Vertical bob
            Mathf.Cos(Time.time * frequency * 0.7f) * amplitude * 0.2f    // Slight depth movement
        );

        transform.position = floatingPosition + bobOffset;
    }

    // Call this externally (e.g., from Box script) to start the cinematic sequence
    public void BeginSlither()
    {
        if (isAnimating || startPoint == null || endPoint == null)
        {
            if (debugMode) Debug.LogWarning("EyeballMovement: Cannot start animation - already animating or missing targets");
            return;
        }

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(CinematicEyeballSequence());
    }

    private IEnumerator CinematicEyeballSequence()
    {
        isAnimating = true;
        currentPhase = AnimationPhase.Waiting;

        if (debugMode) Debug.Log("EyeballMovement: Starting cinematic sequence");

        // Phase 1: Initial delay (building suspense)
        yield return new WaitForSeconds(riseDelay);

        // Phase 2: Rise from crate with eyelid animation
        yield return StartCoroutine(RiseFromCrate());

        // Phase 3: Look at player menacingly
        yield return StartCoroutine(LookAtPlayer());

        // Phase 4: Move to eye socket
        yield return StartCoroutine(MoveToEyeSocket());

        // Phase 5: Attach and close eyelids
        yield return StartCoroutine(AttachAndClose());

        currentPhase = AnimationPhase.Complete;
        isAnimating = false;

        if (debugMode) Debug.Log("EyeballMovement: Cinematic sequence complete");
    }

    private IEnumerator RiseFromCrate()
    {
        currentPhase = AnimationPhase.Rising;
        if (debugMode) Debug.Log("EyeballMovement: Phase 1 - Rising from crate");

        // Play sound and effects
        if (audioSource && riseSound) audioSource.PlayOneShot(riseSound);
        if (dustParticles) dustParticles.Play();

        Vector3 startPos = startPoint.position;
        Vector3 riseTarget = startPos + Vector3.up * riseHeight;

        float timer = 0f;
        bool hasOpenedEyelids = false;

        while (timer < riseDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / riseDuration;
            float easedProgress = riseEasing.Evaluate(progress);

            // Move eyeball up
            floatingPosition = Vector3.Lerp(startPos, riseTarget, easedProgress);

            // Open eyelids gradually after 30% of rise
            if (progress > 0.3f && !hasOpenedEyelids)
            {
                hasOpenedEyelids = true;
                StartCoroutine(OpenEyelids());
            }

            // Add some dramatic blinking during rise
            if (progress > 0.5f && progress < 0.8f)
            {
                float blinkFactor = Mathf.Sin(Time.time * eyelidBlinkSpeed);
                if (blinkFactor > 0.7f)
                {
                    //StartCoroutine(QuickBlink());
                }
            }

            yield return null;
        }

        floatingPosition = riseTarget;
        if (debugMode) Debug.Log("EyeballMovement: Rise complete");
    }

    private IEnumerator LookAtPlayer()
    {
        currentPhase = AnimationPhase.LookingAtPlayer;
        if (debugMode) Debug.Log("EyeballMovement: Phase 2 - Looking at player");

        // Play menacing sound
        if (audioSource && menacingSound)
        {
            audioSource.clip = menacingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float timer = 0f;
        Quaternion startRotation = transform.rotation;

        while (timer < lookAtPlayerDuration)
        {
            timer += Time.deltaTime;

            // Look at player
            if (playerTransform != null)
            {
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookRotationSpeed * Time.deltaTime);
            }

            // Random menacing blinks
            if (Random.Range(0f, 1f) < 0.02f) // 2% chance per frame
            {
                //StartCoroutine(MenacingBlink());
            }

            yield return null;
        }

        // Stop menacing sound
        if (audioSource && audioSource.isPlaying && audioSource.clip == menacingSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (debugMode) Debug.Log("EyeballMovement: Look at player complete");
    }

    private IEnumerator MoveToEyeSocket()
    {
        currentPhase = AnimationPhase.MovingToSocket;
        if (debugMode) Debug.Log("EyeballMovement: Phase 3 - Moving to eye socket");

        Vector3 startPos = floatingPosition;
        Vector3 endPos = endPoint.position;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = endPoint.rotation;

        float timer = 0f;

        while (timer < moveToSocketDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / moveToSocketDuration;
            float easedProgress = moveToSocketEasing.Evaluate(progress);

            // Move and rotate towards socket
            floatingPosition = Vector3.Lerp(startPos, endPos, easedProgress);
            transform.rotation = Quaternion.Slerp(startRot, endRot, easedProgress);

            yield return null;
        }

        floatingPosition = endPos;
        transform.rotation = endRot;
        if (debugMode) Debug.Log("EyeballMovement: Move to socket complete");
    }

    private IEnumerator AttachAndClose()
    {
        currentPhase = AnimationPhase.Attaching;
        if (debugMode) Debug.Log("EyeballMovement: Phase 4 - Attaching and closing");

        // Final positioning
        transform.position = endPoint.position;
        transform.rotation = endPoint.rotation;

        // Play attach sound and effects
        if (audioSource && attachSound) audioSource.PlayOneShot(attachSound);
        if (attachParticles) attachParticles.Play();

        // Close eyelids slowly and menacingly
        yield return StartCoroutine(CloseEyelidsSlowly());

        if (debugMode) Debug.Log("EyeballMovement: Attach and close complete");
    }

    private IEnumerator OpenEyelids()
    {
        Debug.Log("Open eyelids");
        if (upperEyelid == null || lowerEyelid == null) yield break;

        float timer = 0f;
        float duration = 0.8f;

        Vector3 upperStart = upperEyelid.eulerAngles;
        Vector3 lowerStart = lowerEyelid.eulerAngles;
        Vector3 upperTarget = initialUpperEyelidRotation;
        Vector3 lowerTarget = initialLowerEyelidRotation;
        Debug.Log("UpperStart[" + upperStart.ToString() + "] LowerStart[" + lowerStart.ToString() + "] UpperTarget[" + upperTarget.ToString() + "] LowerTarget["+lowerTarget.ToString()+"]");

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            upperEyelid.eulerAngles = Vector3.Lerp(upperStart, upperTarget, progress);
            lowerEyelid.eulerAngles = Vector3.Lerp(lowerStart, lowerTarget, progress);

            yield return null;
        }
    }

    private IEnumerator CloseEyelidsSlowly()
    {
        Debug.Log("Close eyelids slowly");
        if (upperEyelid == null || lowerEyelid == null) yield break;

        float timer = 0f;

        Vector3 upperStart = upperEyelid.eulerAngles;
        Vector3 lowerStart = lowerEyelid.eulerAngles;
        Vector3 upperTarget = initialUpperEyelidRotation + upperEyelidClosedRotation;
        Vector3 lowerTarget = initialLowerEyelidRotation + lowerEyelidClosedRotation;

        while (timer < eyeCloseDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / eyeCloseDuration;
            float easedProgress = eyeCloseEasing.Evaluate(progress);

            upperEyelid.eulerAngles = Vector3.Lerp(upperStart, upperTarget, easedProgress);
            lowerEyelid.eulerAngles = Vector3.Lerp(lowerStart, lowerTarget, easedProgress);

            yield return null;
        }
    }

    private IEnumerator QuickBlink()
    {
        Debug.Log("Quick blink");
        if (upperEyelid == null || lowerEyelid == null) yield break;

        Vector3 upperOpen = upperEyelid.eulerAngles;
        Vector3 lowerOpen = lowerEyelid.eulerAngles;
        Vector3 upperClosed = upperOpen + upperEyelidClosedRotation * 0.5f;
        Vector3 lowerClosed = lowerOpen + lowerEyelidClosedRotation * 0.5f;

        // Close quickly
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            float progress = timer / 0.1f;

            upperEyelid.eulerAngles = Vector3.Lerp(upperOpen, upperClosed, progress);
            lowerEyelid.eulerAngles = Vector3.Lerp(lowerOpen, lowerClosed, progress);

            yield return null;
        }

        // Open quickly
        timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            float progress = timer / 0.1f;

            upperEyelid.eulerAngles = Vector3.Lerp(upperClosed, upperOpen, progress);
            lowerEyelid.eulerAngles = Vector3.Lerp(lowerClosed, lowerOpen, progress);

            yield return null;
        }
    }

    private IEnumerator MenacingBlink()
    {
        yield return StartCoroutine(QuickBlink());
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        if (Random.Range(0f, 1f) < 0.3f) // 30% chance for double blink
        {
            yield return StartCoroutine(QuickBlink());
        }
    }

    private void CloseEyelids(bool immediate = false)
    {
        Debug.Log("Close eyelids");
        if (upperEyelid == null || lowerEyelid == null) return;

        Vector3 upperTarget = initialUpperEyelidRotation + upperEyelidClosedRotation;
        Vector3 lowerTarget = initialLowerEyelidRotation + lowerEyelidClosedRotation;

        if (immediate)
        {
            upperEyelid.eulerAngles = upperTarget;
            lowerEyelid.eulerAngles = lowerTarget;
        }
        else
        {
            StartCoroutine(CloseEyelidsSlowly());
        }
    }

    // Public methods for external control
    public void StopAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        isAnimating = false;
        currentPhase = AnimationPhase.Waiting;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    public AnimationPhase GetCurrentPhase()
    {
        return currentPhase;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw path from start to end
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // Draw rise position
            Vector3 risePos = startPoint.position + Vector3.up * riseHeight;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(risePos, 0.5f);
            Gizmos.DrawLine(startPoint.position, risePos);

            // Draw start and end points
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(startPoint.position, 0.3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(endPoint.position, 0.3f);
        }

        // Draw current animation phase
        if (Application.isPlaying)
        {
            Gizmos.color = GetPhaseColor();
            Gizmos.DrawWireSphere(transform.position, 0.8f);
        }
    }

    private Color GetPhaseColor()
    {
        switch (currentPhase)
        {
            case AnimationPhase.Waiting: return Color.gray;
            case AnimationPhase.Rising: return Color.yellow;
            case AnimationPhase.LookingAtPlayer: return Color.red;
            case AnimationPhase.MovingToSocket: return Color.cyan;
            case AnimationPhase.Attaching: return Color.magenta;
            case AnimationPhase.Complete: return Color.green;
            default: return Color.white;
        }
    }
}