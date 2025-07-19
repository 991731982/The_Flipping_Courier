using UnityEngine;

public class MeltTrigger : MonoBehaviour
{
    public DoorMelt doorController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Eyeball"))
        {
            // play door anim
            doorController.TriggerDissolve();

            // remove collider
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                Destroy(myCollider);
            }
        }
    }
}
