using UnityEngine;
using System.Collections.Generic;
public class ObjectPoolSetting : MonoBehaviour
{
    public List<ObjectPoolElement> objectPoolElements;
    public bool isParentKeep = true;
    private void Start()
    {
        if (!isParentKeep) transform.SetParent(null);
        for (int i = 0; i < objectPoolElements.Count; i++)
        {
            PoolingManager.Instance.CreatePool(objectPoolElements[i].name, objectPoolElements[i].prefab, objectPoolElements[i].cnt, transform);
        }
    }
}
[System.Serializable]
public struct ObjectPoolElement
{
    public string name;
    public GameObject prefab;
    public int cnt;
}