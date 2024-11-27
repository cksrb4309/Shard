using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NearestAttackableSelector : MonoBehaviour
{
    static NearestAttackableSelector instance = null;

    public LayerMask layerMask;

    List<IAttackable> fieldList = new List<IAttackable>();
    private void Awake()
    {
        instance = this;
    }

    public static IAttackable GetAttackable() => instance.Get();
    IAttackable Get()
    {
        fieldList.Clear();

        for (float radius = 1f; radius < 30f; radius += 3f)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    IAttackable attackable = colliders[i].GetComponent<IAttackable>();

                    if (attackable.IsAlive()) fieldList.Add(attackable);
                }
                if (fieldList.Count > 0) return fieldList[Random.Range(0, fieldList.Count)];
            }
        }
        return null;
    }

    public static List<IAttackable> GetAttackable(Vector3 position, float maxRadius, int count)
    {
        List<IAttackable> list = new List<IAttackable>();

        Collider[] colliders;

        for (float radius = 0.5f; radius < maxRadius; radius += 1)
        {
            colliders = Physics.OverlapSphere(position, radius, instance.layerMask);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    IAttackable attackable = colliders[i].GetComponent<IAttackable>();

                    if (attackable.IsAlive()) list.Add(attackable);
                }
                if (list.Count >= count) // 지정된 수량 이상으로 때릴 수 있는 것을 찾았을 때
                {
                    while (list.Count > count) list.RemoveAt(Random.Range(0, list.Count));

                    return list;
                }
            }
        }

        colliders = Physics.OverlapSphere(position, maxRadius, instance.layerMask);

        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                IAttackable attackable = colliders[i].GetComponent<IAttackable>();

                if (attackable.IsAlive()) list.Add(attackable);
            }
            if (list.Count >= count) // 지정된 수량 이상으로 때릴 수 있는 것을 찾았을 때
            {
                while (list.Count > count) list.RemoveAt(Random.Range(0, list.Count));

                return list;
            }
        }
        return list;
    }
    public static IAttackable GetAttackable(Vector3 position, float maxRadius)
    {
        List<IAttackable> list = new List<IAttackable>();

        Collider[] colliders;

        for (float radius = 0.5f; radius < maxRadius; radius += 1)
        {
            colliders = Physics.OverlapSphere(position, radius, instance.layerMask);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    IAttackable attackable = colliders[i].GetComponent<IAttackable>();

                    if (attackable.IsAlive()) list.Add(attackable);
                }
                if (list.Count >= 1) return list[Random.Range(0, list.Count)];
            }
        }

        colliders = Physics.OverlapSphere(position, maxRadius, instance.layerMask);

        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                IAttackable attackable = colliders[i].GetComponent<IAttackable>();

                if (attackable.IsAlive()) list.Add(attackable);
            }
            if (list.Count >= 1) return list[Random.Range(0, list.Count)];
        }
        return null;
    }
    public static IAttackable GetAttackable(Vector3 position, float maxRadius, List<IAttackable> ignoredList)
    {
        List<IAttackable> list = new List<IAttackable>();

        Collider[] colliders;
        /*
        for (float radius = 0.5f; radius < maxRadius; radius += 1)
        {
            colliders = Physics.OverlapSphere(position, radius, instance.layerMask);

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    IAttackable attackable = colliders[i].GetComponent<IAttackable>();
                    if (attackable.IsAlive()) 
                    {
                        bool ignored = false;
                        for (int j = 0; j < ignoredList.Count; j++)
                        {
                            if (ignoredList[j].Equals(attackable)) 
                            {
                                ignored = true;

                                break;
                            }
                        }
                        if (!ignored) list.Add(attackable);
                    }
                }
                if (list.Count >= 1) 
                {
                    return list[Random.Range(0, list.Count)];
                }
            }
        }*/

        colliders = Physics.OverlapSphere(position, maxRadius, instance.layerMask);

        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                IAttackable attackable = colliders[i].GetComponent<IAttackable>();
                if (attackable.IsAlive())
                {
                    bool ignored = false;
                    for (int j = 0; j < ignoredList.Count; j++)
                    {
                        if (ignoredList[j].Equals(attackable))
                        {
                            ignored = true;

                            break;
                        }
                    }
                    if (!ignored) list.Add(attackable);
                }
            }
            if (list.Count >= 1)
            {
                return list[Random.Range(0, list.Count)];
            }
        }

        return null;
    }
}
