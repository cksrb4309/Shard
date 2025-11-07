using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ObjectPool : MonoBehaviour
{
    PoolEntry poolEntry;

    Queue<GameObject> queue = new Queue<GameObject>();

    bool isExpanding = false;

    private IEnumerator ExpandPoolAsync()
    {
        isExpanding = true;

        for (int i = 0; i < poolEntry.expansionSize; i++)
        {
            GameObject obj = Instantiate(poolEntry.prefab, poolEntry.parent);

            obj.SetActive(false);

            queue.Enqueue(obj);

            // 한 프레임마다 몇 개씩 생성할지 조절
            if (i % 10 == 0)
            {
                yield return null;
            }
        }

        isExpanding = false;
    }
    public void CreateObject()
    {
        for (int i = 0; i < poolEntry.initSize; i++)
        {
            GameObject obj = Instantiate(poolEntry.prefab, poolEntry.parent);

            obj.SetActive(false);

            queue.Enqueue(obj);
        }
    }
    public void Setting(PoolEntry poolEntry, Transform parent = null)
    {
        this.poolEntry = poolEntry;

        CreateObject();
    }
    // 오브젝트 가져오기 (GameObject를 직접 반환)
    public GameObject GetObject()
    {
        if (queue.Count == 0)
        {
            if (!isExpanding)
            {
                StartCoroutine(ExpandPoolAsync());
            }

            GameObject immediateObj = Instantiate(poolEntry.prefab, poolEntry.parent);

            return immediateObj;
        }

        GameObject obj = queue.Dequeue();

        obj.SetActive(true);

        return obj;
    }

    // 오브젝트 반환하기
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        queue.Enqueue(obj);
    }

    // 제네릭 메서드로 특정 컴포넌트를 가져오기
    public T GetObject<T>() where T : Component
    {
        return GetObject().GetComponent<T>();  // T 타입의 컴포넌트를 반환
    }
}
