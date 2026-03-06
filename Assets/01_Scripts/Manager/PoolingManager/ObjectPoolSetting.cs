using UnityEngine;
using System.Collections.Generic;
public class ObjectPoolSetting : MonoBehaviour
{
    public List<PoolEntry> entrys;
    private void Start()
    {
        for (int i = 0; i < entrys.Count; i++)
        {
            PoolingManager.Instance.CreatePool(entrys[i], transform);
        }
    }
}
[System.Serializable]
public class PoolEntry
{
    public string name;
    public GameObject prefab;
    public int initSize;
    public int expansionSize;

    [HideInInspector] public Transform parent;
}