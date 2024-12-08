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
        myTransform  = transform; // TODO 타이틀에서 진입 만들면 삭제하기

        Debug.Log("GameManager Awake");

        instance = this;

        int[] playerShips = new int[PlayerPrefs.GetInt("PlayerCount")];

        float range = 1.5f;

        int mainPlayer = PlayerPrefs.GetInt("MainPlayer");

        for (int i = 0; i < playerShips.Length; i++)
        {
            playerShips[i] = PlayerPrefs.GetInt("Player" + i.ToString());
            GameObject ship = Instantiate(ships[playerShips[i]]);

            // 위치는 랜덤하게 코어 근처로 지정
            ship.transform.position =
                coreTransform.position +
                new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f)).normalized * range;

            ship.transform.rotation = Quaternion.identity;

            AddPlayer(ship.transform);

            if (mainPlayer == i) 
            {
                Debug.Log(" 메인 플레이어 확인 ");

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

        Debug.Log("Stop : 현재 : " + instance.currentStopPlayer.ToString());

        if (instance.playerPositions.Count == instance.currentStopPlayer)
            Time.timeScale = 0;
    }
    public static void PlayTime()
    {
        Debug.Log("Play : 현재 : " + instance.currentStopPlayer.ToString());

        instance.currentStopPlayer--;

        Time.timeScale = 1;
    }

    // 게임 매니저에 플레이어를 추가한다
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

        Debug.Log("남은 플레이어 : " + instance.alivePlayer.ToString());

        // 다 죽은 경우
        if (instance.alivePlayer <= 0)
        {
            instance.StartCoroutine(instance.GameOverCoroutine());
        }
    }
    IEnumerator GameOverCoroutine()
    {
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "결정체를...");
        yield return new WaitForSeconds(0.6f);
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "지키지...");
        yield return new WaitForSeconds(0.6f);
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "못했습니다...");
        yield return new WaitForSeconds(0.6f);
        RealtimeCanvasUI.Notification(IconType.DeadEnding, "G A M E O V E R");

        // TODO 데드엔딩 스크린 트랜지션
        ScreenTransition.Play("DeadEnding_FadeOut", "DeadEnding_FadeIn", Color.black, Color.black, "Title", 0, 0);
    }
}