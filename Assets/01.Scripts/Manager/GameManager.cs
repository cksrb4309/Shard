using System.Collections;
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
    Transform playerTransform = null;
    private void Awake()
    {
        instance = this;   
        /*
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }*/
    }

    // ���� �Ŵ����� �÷��̾ �߰��Ѵ�
    public void AddPlayer(Transform playerTransform)
    {
        playerPositions.Add(playerTransform);
    }

    public Vector3 GetPlayerPosition(Vector3 position)
    {
        // position�� ���� ����� �÷��̾��� ��ġ�� ��ȯ�Ѵ�
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
        // position�� ���� ����� �÷��̾��� ��ġ�� ��ȯ�Ѵ�
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
    public static Transform GetPlayerTransform() => instance.playerTransform;
    public static void SetPlayerTransform(Transform playerTransform) => instance.playerTransform = playerTransform;
    public static IAttackable GetLastHit()
    {
        return instance.LastHitMonster;
    }
    public static void SetLastHit(IAttackable attackable)
    {
        Debug.Log("������ ���� Attackable ó��");

        instance.LastHitMonster = attackable;
    }
}
