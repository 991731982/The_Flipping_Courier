using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindFan : MonoBehaviour
{
    public float windForce = 100f;  // 風的力量
    public Vector3 boxSize = new Vector3(5f, 3f, 3f); // 風的影響範圍 (長, 高, 深)
    public float massThreshold = 60f; // 影響質量的閾值
    private GameObject windEffect;

    void Start()
    {
        // 創建一個可視化的風範圍顯示
        windEffect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(windEffect.GetComponent<Collider>()); // 刪除碰撞體
        windEffect.transform.SetParent(transform);
        windEffect.transform.localScale = boxSize;

        // 設置透明材質
       MeshRenderer renderer = windEffect.GetComponent<MeshRenderer>();
        Material transparentMaterial = new Material(Shader.Find("Standard"));
        transparentMaterial.color = new Color(0f, 0f, 0f, 0f); // 半透明顏色
        transparentMaterial.SetFloat("_Mode", 3); // 設置為透明模式
        transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetInt("_ZWrite", 0);
        transparentMaterial.DisableKeyword("_ALPHATEST_ON");
        transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
        transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMaterial.renderQueue = 3000;

        renderer.material = transparentMaterial;
    
    }


    void FixedUpdate()
    {
        Vector3 windDirection = transform.right;
        Vector3 windOrigin = transform.position + windDirection * (boxSize.x / 2);
        Collider[] colliders = Physics.OverlapBox(windOrigin, boxSize / 2, transform.rotation);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && rb.mass < massThreshold)
            {
                rb.AddForce(windDirection * windForce, ForceMode.Acceleration);
            }
        }

        // 更新風的可視化區域位置
        windEffect.transform.position = windOrigin;
        windEffect.transform.rotation = transform.rotation;
    }

   /*void OnDrawGizmos()
    {
       // Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.right * (boxSize.x / 2), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }*/
}
