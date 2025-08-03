using UnityEngine;
using System.Collections;

public class DoorLever : MonoBehaviour
{
    public Vector3 moveDirection = new Vector3(0, 5, 0); // ÃÅÒÆ¶¯µÄ·½Ïò
    public float moveSpeed = 2.0f; // ÃÅÒÆ¶¯µÄËÙ¶È

    private Vector3 initialPosition; // ÃÅµÄ³õÊ¼Î»ÖÃ
    private bool isMoving = false;   // ·ÀÖ¹Í¬Ê±½øÐÐ¶à¸öÒÆ¶¯²Ù×÷
    private bool isOpen = false;     // ÃÅµÄ×´Ì¬£¨ÊÇ·ñÒÑ´ò¿ª£©

    // ³õÊ¼»¯
    void Start()
    {
        initialPosition = transform.position; // ±£´æ³õÊ¼Î»ÖÃ
    }

    // ´ò¿ªÃÅ
    public void OpenDoor()
    {
        if (!isMoving && !isOpen)
        {
            StartCoroutine(OpenDoorRoutine());
        }
    }

    // ¹Ø±ÕÃÅ
    public void CloseDoor()
    {
        if (!isMoving && isOpen)
        {
            StartCoroutine(CloseDoorRoutine());
        }
    }

    // ¿ØÖÆÃÅ´ò¿ªµÄ¶¯»­
    private IEnumerator OpenDoorRoutine()
    {
        isMoving = true;
        Vector3 targetPosition = initialPosition + moveDirection; // »ùÓÚ³õÊ¼Î»ÖÃ¼ÆËãÄ¿±êÎ»ÖÃ

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition; // È·±£×îÖÕÎ»ÖÃ¾«×¼
        isMoving = false;
        isOpen = true; // ¸üÐÂ×´Ì¬
        Debug.Log("Door opened!");
    }

    // ¿ØÖÆÃÅ¹Ø±ÕµÄ¶¯»­
    private IEnumerator CloseDoorRoutine()
    {
        isMoving = true;
        Vector3 targetPosition = initialPosition; // Ä¿±êÎ»ÖÃÊÇ³õÊ¼Î»ÖÃ

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition; // È·±£×îÖÕÎ»ÖÃ¾«×¼
        isMoving = false;
        isOpen = false; // ¸üÐÂ×´Ì¬
        Debug.Log("Door closed!");
    }

    // ÖØÖÃÃÅµÄ×´Ì¬ºÍÎ»ÖÃ£¨¿ÉÑ¡£©
    public void ResetDoor()
    {
        StopAllCoroutines(); // Í£Ö¹ËùÓÐÕýÔÚÔËÐÐµÄÐ­³Ì
        transform.position = initialPosition; // »Ö¸´µ½³õÊ¼Î»ÖÃ
        isMoving = false; // ÖØÖÃÒÆ¶¯×´Ì¬
        isOpen = false; // ÖØÖÃÃÅµÄ×´Ì¬
        Debug.Log("Door reset to initial position.");
    }
}
