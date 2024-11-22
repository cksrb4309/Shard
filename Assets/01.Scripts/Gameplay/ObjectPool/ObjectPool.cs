using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;


public class ObjectPool : MonoBehaviour
{
    [HideInInspector] public Transform parent = null;

    public GameObject prefab;  // �̸� ������ ������Ʈ�� ����(prefab)
    public int initialSize = 10;  // �ʱ� Ǯ ������

    private Queue<GameObject> pool = new Queue<GameObject>();

    // Ǯ �ʱ�ȭ
    private void Start()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // ������Ʈ �������� (GameObject�� ���� ��ȯ)
    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject newObj = GameObject.Instantiate(prefab, parent);

            newObj.SetActive(true);

            return newObj;
        }
    }

    // ������Ʈ ��ȯ�ϱ�
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    // ���׸� �޼���� Ư�� ������Ʈ�� ��������
    public T GetObject<T>() where T : Component
    {
        GameObject obj = GetObject();  // ���� GetObject �޼��带 �̿��� ������Ʈ�� ������
        return obj.GetComponent<T>();  // T Ÿ���� ������Ʈ�� ��ȯ
    }
}
