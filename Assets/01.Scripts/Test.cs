using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform spawnTransform;
    public Monster testMob;

    public List<Ability> abilities;

    public Transform startTransform;
    public Transform endTransform;

    public Vector3 dir = Vector3.zero;
    public Vector3 axis = Vector3.zero;

    public float angle = 0f;
    public bool start = false;

    private void FixedUpdate()
    {
        if (start == true)
        {
            start = false;

            foreach (Ability ability in abilities)
                Inventory.GetAbility(ability);
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


    public Transform[] beforeTransformArray;
    public Transform[] afterTransformArray;
    public Transform rotateTransform;
    public void Excute()
    {
        for (int i = 0; i < afterTransformArray.Length; i++)
        {
            afterTransformArray[i].position = rotateTransform.rotation * beforeTransformArray[i].position;
        }
    }
}