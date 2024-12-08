using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get { return instance; }
    }
    public IAttackable LastHitMonster = null;

    public Transform coreTransform = null;
    
    public SearchNearPlayers searchNearPlayers;

    public TrackingPlayer trackingPlayer;

    public FollowCursor followCursor;

    List<Transform> playerPositions = new List<Transform>();

    Transform myTransform = null;

    public GameObject[] ships;

    int alivePlayer = 0;

    private void Awake()
    {
        myTransform  = transform; // TODO Ÿ��Ʋ���� ���� ����� �����ϱ�

        Debug.Log("GameManager Awake");

        instance = this;

        int[] playerShips = new int[PlayerPrefs.GetInt("PlayerCount")];

        float range = 1.5f;

        int mainPlayer = PlayerPrefs.GetInt("MainPlayer");

        for (int i = 0; i < playerShips.Length; i++)
        {
            playerShips[i] = PlayerPrefs.GetInt("Player" + i.ToString());
            GameObject ship = Instantiate(ships[playerShips[i]]);

            // ��ġ�� �����ϰ� �ھ� ��ó�� ����
            ship.transform.position =
                coreTransform.position +
                new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f)).normalized * range;

            ship.transform.rotation = Quaternion.identity;

            AddPlayer(ship.transform);

            if (mainPlayer == i) 
            {
                Debug.Log(" ���� �÷��̾� Ȯ�� ");

                SetMyTransform(ship.transform);
                trackingPlayer.Connect(ship.transform);
                followCursor.Connect(ship.transform);
            }

            alivePlayer++;
        }
    }
    int currentStopPlayer = 0;
    public static void StopTime()
    {
        instance.currentStopPlayer++;

        Debug.Log("Stop : ���� : " + instance.currentStopPlayer.ToString());

        if (instance.playerPositions.Count == instance.currentStopPlayer)
            Time.timeScale = 0;
    }
    public static void PlayTime()
    {
        Debug.Log("Play : ���� : " + instance.currentStopPlayer.ToString());

        instance.currentStopPlayer--;

        Time.timeScale = 1;
    }

    // ���� �Ŵ����� �÷��̾ �߰��Ѵ�
    public void AddPlayer(Transform playerTransform)
    {
        playerPositions.Add(playerTransform);

        searchNearPlayers.SetPlayerCount(playerPositions.Count);
    }
    public static Transform GetUserTransform() => instance.myTransform;
    public static Transform GetCoreTransform() => instance.coreTransform;
    public static Transform GetMonsterTargetTransform(Vector3 position) => instance.GetMonsterTargetTransformExcute(position);
    public Transform GetMonsterTargetTransformExcute(Vector3 position)
    {
        float minRange = Vector3.Magnitude(position - coreTransform.position);

        Transform ret = coreTransform;

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
    public void SetMyTransform(Transform playerTransform) => myTransform = playerTransform;
    public static IAttackable GetLastHit() => instance.LastHitMonster;
    public static void SetLastHit(IAttackable attackable) => instance.LastHitMonster = attackable;
    public static void PlayerKill()
    {
        foreach (Transform playerTransform in instance.playerPositions)
        {
            playerTransform.GetComponent<IDamageable>().TakeDamage(float.MaxValue);
        }
    }
    public static void PlayerDie()
    {
        instance.alivePlayer--;

        Debug.Log("���� �÷��̾� : " + instance.alivePlayer.ToString());

        // �� ���� ���
        if (instance.alivePlayer <= 0)
        {
            instance.StartCoroutine(instance.GameOverCoroutine());
        }
    }
    IEnumerator GameOverCoroutine()
    {
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "����ü��...");
        yield return new WaitForSeconds(0.6f);
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "��Ű��...");
        yield return new WaitForSeconds(0.6f);
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "���߽��ϴ�...");
        yield return new WaitForSeconds(0.6f);
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "G A M E O V E R");

        // TODO ���忣�� ��ũ�� Ʈ������
        ScreenTransition.Play("DeadEnding_FadeOut", "DeadEnding_FadeIn", Color.black, Color.black, "Title", 0, 0);
    }
}