using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform spawnTransform;
    public Monster testMob;

    public List<TempAbility> abilities;

    public Transform startTransform;
    public Transform endTransform;

    public Vector3 dir = Vector3.zero;
    public Vector3 axis = Vector3.zero;

    public float angle = 0f;
    public bool start = false;
    private void Start()
    {
        // testMob.Setting(1f, 1f);

        for (int i = 0; i < 10; i++)
        {
            foreach (TempAbility ability in abilities)
                Inventory.GetAbility(ability);
        }

        StartCoroutine(SpawnCoroutine());
    }
    private void FixedUpdate()
    {
        if (start == true)
        {
            start = false;

            //Quaternion.LookRotation((endTransform.position - startTransform.position).normalized).ToAngleAxis(out angle, out axis);

            Debug.Log((Quaternion.Euler(new Vector3(0,angle,0)) * dir).ToString());
        }

    }
    void Spawn()
    {
        Monster monster = Instantiate(testMob, spawnTransform.position, spawnTransform.rotation);

        monster.Setting(1f, 1f);
    }
    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            Spawn();
        }
    }
}