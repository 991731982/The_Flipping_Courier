using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDragPrompt : MonoBehaviour
{
    private Camera cam;

    [Tooltip("A prefab containing the image you wish to have displayed. If the image is reverse, use the Flip function in the Sprite Renderer.")]
    public GameObject displayObject;

    [Tooltip("How far away the player should be for the tip to appear in U.")]
    public float distanceToAppear;
    private GameObject instance;

    [Tooltip("How far away from the object this script is attached to the image should appear, along 3 axes, in U.")]
    public Vector3 appearOffset;


    public CubeCharacterController characterController;

    private void Start()
    {
        cam = Camera.main;

        if (characterController == null)
        {
            characterController = FindFirstObjectByType<CubeCharacterController>();
        }

    }

    private void Update()
    {

        float distanceToPlayer = Mathf.Abs((transform.position - characterController.transform.position).magnitude);
        //Debug.Log("Distance to player["+distanceToPlayer.ToString()+"]");
        if (distanceToPlayer <= distanceToAppear)
        {
            if (instance == null)
            {
                instance = Instantiate(displayObject, transform.position + appearOffset, Quaternion.identity);
            }
            //instance.transform.LookAt(cam.transform);
        }
        else if (instance != null)
        {
            Destroy(instance);
        }
    }
}
