using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Eyeball Reference")]
    public EyeballMovement eyeball;

    [Header("Door Movement")]
    public Vector3 moveDirection = new Vector3(0f, 5f, 0f);
    public float moveSpeed = 2f;
    public float triggerRange = 0.02f;

    [Header("Sound")]
    public AudioSource doorOpenSound;  // ✅ 拖音效的 AudioSource

    private bool doorOpened = false;

    private void Update()
    {
        if (doorOpened || eyeball == null)
            return;

        if (Vector3.Distance(eyeball.transform.position, eyeball.endPoint.position) <= triggerRange)
        {
            StartCoroutine(OpenDoor());
            doorOpened = true;
        }
    }

    private IEnumerator OpenDoor()
    {
        if(doorOpened)
            yield break;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + moveDirection;

        // ✅ 播放开门音效（如果存在）
        if (doorOpenSound != null)
        {
            doorOpenSound.Play();
            Debug.Log("🔊 Door opening sound played!");
        }
        else
        {
            Debug.LogWarning("⚠️ doorOpenSound 未设置，无法播放开门音效！");
        }

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log("Door opened!");
    }
}
