using UnityEngine;

public class TriggerEffectOnce : MonoBehaviour
{
    public ParticleSystem particleEffect; // 直接掛場景中的特效物件
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            particleEffect.Play();
            StartCoroutine(StopAfterSeconds(3f));
        }
    }

    private System.Collections.IEnumerator StopAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        particleEffect.Stop();
        // 可選：隱藏或關閉
        // particleEffect.gameObject.SetActive(false);
    }
}
