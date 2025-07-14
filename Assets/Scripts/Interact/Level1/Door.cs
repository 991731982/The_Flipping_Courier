using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Eyeball Reference")]
    public EyeballMovement eyeball;          // Eyeball movement script

    [Header("Door Movement")]
    public Vector3 moveDirection = new Vector3(0f, 5f, 0f); // How far / which way to move
    public float moveSpeed = 2f;                      // Slide speed
    public float triggerRange = 0.02f;                   // Distance considered "arrived"

    private bool doorOpened = false;                        // Ensures one time open

    private void Update()
    {
        if (doorOpened || eyeball == null)
            return;

        // When eyeball is close enough to its end socket, open door
        if (Vector3.Distance(eyeball.transform.position, eyeball.endPoint.position) <= triggerRange)
        {
            StartCoroutine(OpenDoor());
            doorOpened = true;
        }
    }

    private IEnumerator OpenDoor()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + moveDirection;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null; // wait for next frame
        }

        transform.position = targetPos; // snap exactly
        Debug.Log("Door opened!");
    }
}