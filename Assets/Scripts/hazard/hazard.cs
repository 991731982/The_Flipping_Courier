using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            //Debug.Log("Enemy destroyed by hazard!");
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            checkPointRespawn player = collision.gameObject.GetComponent<checkPointRespawn>();
            if (player != null)
            {
                player.RespawnAtCheckpoint();
                //Debug.Log("Player respawned at checkpoint due to hazard!");
            }
        }
    }
}
