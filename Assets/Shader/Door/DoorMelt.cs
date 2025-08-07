using UnityEngine;

public class DissolveDoorController : MonoBehaviour
{
    public Renderer doorRenderer; 
    public Collider doorCollider; 
    public string dissolveProperty = "DoorDisappear"; 
    public float dissolveDuration = 2f;

    private Material doorMaterial;
    private float dissolveTimer = 0f;
    private bool isDissolving = false;

    void Start()
    {
        doorMaterial = doorRenderer.material;
        doorMaterial.SetFloat(dissolveProperty, 0f);
        TriggerDissolve();
    }

    void Update()
    {
        if (isDissolving)
        {
            Debug.Log("Test");
            dissolveTimer += Time.deltaTime;
            float t = dissolveTimer / dissolveDuration;
            float dissolveValue = Mathf.Clamp01(t);
            doorMaterial.SetFloat(dissolveProperty, dissolveValue);

            if (t >= 1f)
            {
                isDissolving = false;

                // remove door collider
                if (doorCollider != null)
                {
                    Destroy(doorCollider);
                }
            }
        }
    }

    public void TriggerDissolve()
    {
        Debug.Log("TEST2");
        isDissolving = true;
        dissolveTimer = 0f;
    }
}
