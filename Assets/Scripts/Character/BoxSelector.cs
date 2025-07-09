using System.Collections;
using UnityEngine;
using TMPro;

public class BoxSelector : MonoBehaviour
{
    public TextMeshProUGUI countdownText;// ���õ���ʱ�� TextMeshProUGUI ���
    private GameObject selectedObject;
    private bool isSelecting = false;
    private float countdownTime = 6f; // ����ʱʱ��

    void Update()
    {
        // ������ C ��ʱ��ʼѡ��
        if (Input.GetKeyDown(KeyCode.C))
        {
            isSelecting = true;
            countdownTime = 6f; // ���õ���ʱʱ��
            countdownText.gameObject.SetActive(true); // ��ʾ����ʱ�ı�
            StartCoroutine(CountdownCoroutine()); // ��������ʱЭ��
        }

        // �������ѡ���Ұ�ס C ����ѡ������� box
        if (isSelecting && Input.GetKey(KeyCode.C))
        {
            SelectBox();
        }

        // �ɿ� C ��ʱ��ֹͣѡ������ 6 ���ָ�����
        if (isSelecting && Input.GetKeyUp(KeyCode.C) && selectedObject != null)
        {
            StartCoroutine(StopSelectedObjectTemporarily());
            selectedObject = null;
            isSelecting = false;
        }

    
    }

    void SelectBox()
    {
        // ���ҳ��������д��� "box" ��ǩ������
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("float");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject box in boxes)
        {
            float distance = Vector3.Distance(transform.position, box.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                selectedObject = box;
            }
        }

        // ��ѡ��������ʾѡ�е�����
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    IEnumerator StopSelectedObjectTemporarily()
    {
        if (selectedObject != null)
        {
            Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ��ʱ��ֹ����
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;

                // �ȴ� 6 ��
                yield return new WaitForSeconds(6f);

                // �ָ�����
                rb.isKinematic = false;
            }
        }
    }

    IEnumerator CountdownCoroutine()
    {
        while (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            countdownText.text = "Time Remaining: " + Mathf.Ceil(countdownTime).ToString() + "s";
            yield return null;
        }

        // ����ʱ�����������ı�
        countdownText.gameObject.SetActive(false);
    }
}
