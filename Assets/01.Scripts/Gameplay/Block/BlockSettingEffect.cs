using UnityEngine;
using UnityEngine.UIElements;

public class BlockSettingEffect : MonoBehaviour
{
    public ParticleSystem[] particles; // 30개의 파티클 대기 중
    public Transform[] positions;

    int currentPlayCount = 0;
    private void Start()
    {
        particles = new ParticleSystem[positions.Length];

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = positions[i].GetComponentInChildren<ParticleSystem>();

            positions[i].transform.parent = null;
        }
    }
    public void Play(Vector3 pos)
    {
        int index = currentPlayCount;

        // 현재 index를 참조하고, currentPlayCount를 증가
        currentPlayCount = (currentPlayCount + 1) % particles.Length;

        positions[index].transform.position = pos;

        particles[index].Play();
    }
}
