using System.Collections;
using UnityEngine;

public class TitleBlockSpawn : MonoBehaviour
{
    public Vector3 minRange;
    public Vector3 maxRange;
    public Vector2 spawnTerm;

    public Transform particleTransform;
    public ParticleSystem particle;
    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            Transform block = PoolingManager.Instance.GetObject("TitleBlock").transform;

            block.position = new Vector3(
                Random.Range(minRange.x, maxRange.x),
                Random.Range(minRange.y, maxRange.y),
                Random.Range(minRange.z, maxRange.z));

            particleTransform.position = block.position;

            particle.Play();

            yield return new WaitForSeconds(Random.Range(spawnTerm.x, spawnTerm.y));
        }
    }
}
