using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StageManager : MonoBehaviour
{
    static StageManager instance = null;

    public SpawnManager spawnManager = null;

    public Color[] stageColors;

    public Volume volume; // Vignette Color를 바꾸기 위한 볼륨

    public BlockGroup blockGroup;

    public GameObject searchNearPlayersObj;

    public ParticleSystem stageChangeParticle;

    Vignette vignette;

    int currentStage = -1;

    [HideInInspector] public bool isAroundPlayers = false;

    Color blackColor = Color.black;
    public void Awake()
    {
        instance = this;

        if (volume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.Log("Vignette를 성공적으로 가져왔습니다");

            ChangeStageColor(); // 스테이지 컬러 지정
        }
        else
        {
            Debug.LogWarning("Vignette 가져오기 실패 !!!");
        }
    }
    void ChangeStageColor()
    {
        vignette.color.value = stageColors[++currentStage];
    }
    public static void OnKillBoss()
    {
        instance.OnKillBossExcute();
    }
    void OnKillBossExcute()
    {
        StartCoroutine(WaitNextStageCoroutine());
    }
    IEnumerator WaitNextStageCoroutine()
    {
        searchNearPlayersObj.SetActive(true); // 찾기 활성화

        while (!isAroundPlayers) yield return null;

        blockGroup.StageClear(); // 블럭 다시 세팅 시작

        // 코어 쪽에 파티클 추가할지는 고민하는중...
        // 여기다가 파티클 추가
        stageChangeParticle.Play();

        StartCoroutine(NextStageColorCoroutine());

        yield return new WaitForSeconds(1f);

        spawnManager.StartMonsterSpawn(); // 몬스터 스폰 시작

        DifficultyManager.NextStageSetting(); // 난이도 설정
    }
    IEnumerator NextStageColorCoroutine()
    {
        float t = 0;

        Color st = stageColors[currentStage];
        Color ed = stageColors[++currentStage];

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            vignette.color.value = Color.Lerp(st, blackColor, t);

            yield return null;
        }

        t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime * 2f;

            vignette.color.value = Color.Lerp(ed, blackColor, t);

            yield return null;
        }

        vignette.color.value = ed;
    }
}
