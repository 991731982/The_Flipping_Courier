using UnityEngine;
using System.Collections;

public class TriggerEnergyUpUI : MonoBehaviour
{
    public GameObject uiObjectToShow;   // 拖入要顯示的 UI GameObject
    public float showDuration = 3f;

    private bool hasTriggered = false;  // 用來記錄是否已經觸發過

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;  // 設為已觸發，防止再次觸發

            if (uiObjectToShow != null)
            {
                uiObjectToShow.SetActive(true);
                StartCoroutine(HideAfterDelay());
            }
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        uiObjectToShow.SetActive(false);
    }
}
