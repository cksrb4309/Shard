using UnityEngine;
using UnityEngine.UIElements;

public class BlockSettingEffect : MonoBehaviour
{
    public ParticleSystem[] particles; // 30���� ��ƼŬ ��� ��
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

        // ���� index�� �����ϰ�, currentPlayCount�� ����
        currentPlayCount = (currentPlayCount + 1) % particles.Length;

        positions[index].transform.position = pos;

        particles[index].Play();

        Debug.Log("�ε��� : " + index.ToString() + " / ��ġ : " + pos.ToString());
    }
}
