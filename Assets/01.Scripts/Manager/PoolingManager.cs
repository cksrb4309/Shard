using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    private Dictionary<string, object> objectPoolList = new Dictionary<string, object>();
    public static PoolingManager Instance;

    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    // �̱��� ���� ����
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Ǯ ����
    public void CreatePool(string poolName, GameObject prefab, int initialSize, Transform tr = null)
    {
        if (!pools.ContainsKey(poolName))
        {
            GameObject poolObj = new GameObject(poolName + " Pool");

            poolObj.transform.SetParent(tr != null ? tr : transform);

            ObjectPool pool = poolObj.AddComponent<ObjectPool>();

            pool.parent = tr;

            pool.prefab = prefab;
            pool.initialSize = initialSize;
            pools.Add(poolName, pool);
        }
    }

    // ������Ʈ �������� (GameObject�� ��ȯ)
    public GameObject GetObject(string poolName)
    {
        if (pools.ContainsKey(poolName))
        {
            return pools[poolName].GetObject();
        }

        Debug.LogWarning("��! ������Ʈ Ǯ ���� �����ݾ� !! : " + poolName);
        return null;
    }

    // Ư�� ������Ʈ�� �������� (T Ÿ�� ��ȯ)
    public T GetObject<T>(string poolName) where T : Component
    {
        if (pools.ContainsKey(poolName))
        {
            return pools[poolName].GetObject<T>();
        }
        return null;
    }

    // ������Ʈ ��ȯ�ϱ�
    public void ReturnObject(string poolName, GameObject obj)
    {
        if (pools.ContainsKey(poolName))
        {
            pools[poolName].ReturnObject(obj);
        }
        else
        {
            Destroy(obj);  // Ǯ�� �ش��ϴ� ������Ʈ�� ������ �ı�
        }
    }
}

[System.Serializable]
public struct PoolSet
{
    public string Name;
    public GameObject prefab;
}