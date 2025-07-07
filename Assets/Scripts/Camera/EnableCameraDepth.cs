using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnableCameraDepth : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }
}
