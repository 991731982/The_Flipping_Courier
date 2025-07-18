using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineConnector : MonoBehaviour
{
    public Transform objA;
    public Transform objB;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
    }

    void Update()
    {
        if (objA != null && objB != null)
        {
            line.SetPosition(0, objA.position);
            line.SetPosition(1, objB.position);
        }
    }
}
