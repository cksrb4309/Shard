using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get { return instance; }
    }
    public IAttackable LastHitMonster = null;

    List<Transform> playerPositions = new List<Transform>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 매니저에 플레이어를 추가한다
    public void AddPlayer(Transform playerTransform)
    {
        playerPositions.Add(playerTransform);
    }

    public Vector3 GetPlayerPosition(Vector3 position)
    {
        // position과 가장 가까운 플레이어의 위치를 반환한다
        float minRange = float.MaxValue;

        Vector3 ret = Vector3.zero;

        for (int i = 0; i < playerPositions.Count; i++)
        {
            float temp = Vector3.Magnitude(position - playerPositions[i].position);
            if (temp < minRange)
            {
                minRange = temp;

                ret = playerPositions[i].position;
            }
        }
        return ret;
    }
    public Transform GetPlayerTransform(Vector3 position)
    {
        // position과 가장 가까운 플레이어의 위치를 반환한다
        float minRange = float.MaxValue;

        Transform ret = null;

        for (int i = 0; i < playerPositions.Count; i++)
        {
            float temp = Vector3.Magnitude(position - playerPositions[i].position);
            if (temp < minRange)
            {
                minRange = temp;

                ret = playerPositions[i];
            }
        }
        return ret;
    }
}
