using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    private Dictionary<string, object> objectPoolList = new Dictionary<string, object>();
    public static PoolingManager Instance;

    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    // 싱글톤 패턴 구현
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

    // 풀 생성
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

    // 오브젝트 가져오기 (GameObject를 반환)
    public GameObject GetObject(string poolName)
    {
        if (pools.ContainsKey(poolName))
        {
            return pools[poolName].GetObject();
        }

        Debug.LogWarning("너! 오브젝트 풀 세팅 안했잖아 !! : " + poolName);
        return null;
    }

    // 특정 컴포넌트를 가져오기 (T 타입 반환)
    public T GetObject<T>(string poolName) where T : Component
    {
        if (pools.ContainsKey(poolName))
        {
            return pools[poolName].GetObject<T>();
        }
        return null;
    }

    // 오브젝트 반환하기
    public void ReturnObject(string poolName, GameObject obj)
    {
        if (pools.ContainsKey(poolName))
        {
            pools[poolName].ReturnObject(obj);
        }
        else
        {
            Destroy(obj);  // 풀에 해당하는 오브젝트가 없으면 파괴
        }
    }
}

[System.Serializable]
public struct PoolSet
{
    public string Name;
    public GameObject prefab;
}