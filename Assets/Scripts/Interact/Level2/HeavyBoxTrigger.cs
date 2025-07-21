using System.Diagnostics;
using UnityEngine;

public class HeavyBoxTrigger : MonoBehaviour
{
    public float massThreshold = 400f; // ÔO¶¨Ù|Á¿éT™‘£¬´óì¶ß@‚€Öµ²ÅÓ|°lÏÂ½µ
    public float dropDistance = 10f; // ÏÂ½µ¾àëx
    public float dropSpeed = 5f; // ÏÂ½µËÙ¶È£¨¿ÉÕ{Õû£¬ÔOžé 0 ×ƒË²égÏÂ½µ£©

    private FracturedObject fracturedObject;

    public Vector3 explodingObject;

    private bool shouldDrop = false;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position + Vector3.down * dropDistance; // Ó‹ËãÏÂ½µááµÄÎ»ÖÃ
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;

        // ™z²éÊÇ·ñÊÇ Box£¬KÇÒÆäÙ|Á¿ÊÇ·ñ³¬ß^éT™‘
        if (collision.gameObject.CompareTag("Box") && rb != null && rb.mass >= massThreshold)
        {
            UnityEngine.Debug.Log("ÖØÎï Box ×²“ô£¡°×É«ºÐ×Óé_Ê¼ÏÂ½µ£¡");
            shouldDrop = true;
        }
    }

    void Update()
    {
        if (shouldDrop)
        {
            // ×Œ°×É«ºÐ×Ó¾ÂýÏÂ½µ
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, dropSpeed * Time.deltaTime);

            // ™z²éÊÇ·ñµ½ß_Ä¿˜ËÎ»ÖÃ
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                shouldDrop = false; // Í£Ö¹ÒÆ„Ó
            }
        }

        //fracturedObject.Explode(explodingObject,20);
    }

}
