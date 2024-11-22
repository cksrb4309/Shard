using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;


public class ObjectPool : MonoBehaviour
{
    [HideInInspector] public Transform parent = null;

    public GameObject prefab;  // 미리 생성할 오브젝트의 원본(prefab)
    public int initialSize = 10;  // 초기 풀 사이즈

    private Queue<GameObject> pool = new Queue<GameObject>();

    // 풀 초기화
    private void Start()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // 오브젝트 가져오기 (GameObject를 직접 반환)
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

    // 오브젝트 반환하기
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    // 제네릭 메서드로 특정 컴포넌트를 가져오기
    public T GetObject<T>() where T : Component
    {
        GameObject obj = GetObject();  // 기존 GetObject 메서드를 이용해 오브젝트를 가져옴
        return obj.GetComponent<T>();  // T 타입의 컴포넌트를 반환
    }
}
