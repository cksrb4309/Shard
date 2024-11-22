using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform spawnTransform;
    public Monster testMob;

    public Ability testItem;

    private void Start()
    {
        // testMob.Setting(1f, 1f);

        // Inventory.GetAbility(testItem);

        StartCoroutine(SpawnCoroutine());
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
            yield return new WaitForSeconds(5);

            Spawn();
        }
    }
}