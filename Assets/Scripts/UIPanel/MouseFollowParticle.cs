using UnityEngine;

public class MouseFollowParticle : MonoBehaviour
{
    public Camera mainCamera;
    private static MouseFollowParticle instance;

    void Awake()
    {
      
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
       
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10f;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition;
    }
}
