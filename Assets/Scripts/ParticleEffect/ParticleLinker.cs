using UnityEngine;

public class ParticleLinker : MonoBehaviour
{
    public Transform objA;
    public Transform objB;
    public ParticleSystem particle;

    void Update()
    {
        if (objA && objB && particle)
        {
            transform.position = objA.position;
            transform.rotation = Quaternion.LookRotation(objB.position - objA.position);

            var main = particle.main;
            float distance = Vector3.Distance(objA.position, objB.position);
            main.startSpeed = distance / main.startLifetime.constant;

            if (!particle.isPlaying)
                particle.Play();
        }
    }
}
