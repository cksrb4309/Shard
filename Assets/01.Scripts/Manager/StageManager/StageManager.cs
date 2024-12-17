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

    public ParticleSystem stageChangeParticle_1;
    public ParticleSystem stageChangeParticle_2;

    public Transform corePosition;

    Vignette vignette;

    int currentStage = -1;

    [HideInInspector] public bool isAroundPlayers = false;

    Color blackColor = Color.black;
    
    public void Awake()
    {
        instance = this;

        if (volume.profile.TryGet<Vignette>(out vignette))
        {
            ChangeStageColor(); // 스테이지 컬러 지정
        }
    }
    void ChangeStageColor()
    {
        vignette.color.value = stageColors[++currentStage];
    }
    public static void OnKillBoss()
    {
        if (instance.currentStage == 3) return;

        instance.OnKillBossExcute();
    }

    IEnumerator NextLevelCoroutine()
    {
        RealtimeCanvasUI.Notification(IconType.Charge, corePosition.position, "이번 영역에 대한 에너지를 모두 수집했습니다");

        yield return new WaitForSeconds(2f);

        RealtimeCanvasUI.Notification(IconType.Charge, "결정체에 돌아가서 다음 차원으로 이동해주세요");
    }

    void OnKillBossExcute()
    {
        StartCoroutine(WaitNextStageCoroutine());
    }
    public ParticleSystem coreChargeParticle_1;
    public ParticleSystem coreChargeParticle_2;
    public Transform coreTransform;
    IEnumerator WaitNextStageCoroutine()
    {
        // 만약 현재 스테이지가 2일 때는
        // 엔딩을 위한 적들을 꺼내야함
        if (currentStage == 2) {
            currentStage = 3; // 현재 스테이지 3으로 맞춰놓음 (다음 보스몹으로 인한 예외 방지를 위해)

            yield return new WaitForSeconds(1f); // 1초 지연

            SoundManager.Play("EndingBattleBGM", SoundType.Background);

            // 1번 코어가 충전을 시작해야함 TODO 코어 충전 시작 (파티클로 표시 및 Notification 출력)
            coreChargeParticle_1.Play(); // 파티클
            coreChargeParticle_2.Play(); // 파티클

            RealtimeCanvasUI.Notification(IconType.Charge, coreTransform.position, "결정체에 충분한 에너지가 모였습니다");
            yield return new WaitForSeconds(4f); // 4초 지연
            RealtimeCanvasUI.Notification(IconType.Charge, "결정체의 과부하가 시작합니다");
            yield return new WaitForSeconds(4f); // 4초 지연
            RealtimeCanvasUI.Notification(IconType.Charge, "괴생명체들이 이상함을 느끼고 모여들기 시작했습니다");
            yield return new WaitForSeconds(4f); // 4초 지연
            RealtimeCanvasUI.Notification(IconType.Charge, "결정체의 정화가 끝날 때까지 버텨주세요 !");
            // TODO 엔딩 배경음 변경

            // 2번 몬스터가 외곽에서 스폰해야함
            spawnManager.EndingSpawn(); // 스폰 켜기

            // 3번 언제까지 버틸 지에 대해서... (1. 코루틴 시간 처리 2. 적 처치 수 반영)
            StartCoroutine(EndingTimerCoroutine()); // 1번
        }
        // 현재 스테이지가 2가 아니라면 [0, 1 일 때] 다음 스테이지 준비를 해준다
        else
        {
            StartCoroutine(NextLevelCoroutine());

            searchNearPlayersObj.SetActive(true); // 찾기 활성화

            while (!isAroundPlayers) yield return null;

            blockGroup.StageClear(); // 블럭 다시 세팅 시작

            // 코어 쪽에 파티클 추가할지는 고민하는중...
            // 여기다가 파티클 추가
            stageChangeParticle_1.Play();
            stageChangeParticle_2.Play();

            StartCoroutine(NextStageColorCoroutine());

            yield return new WaitForSeconds(1f);

            spawnManager.StartMonsterSpawn(); // 몬스터 스폰 시작

            DifficultyManager.NextStageSetting(); // 난이도 설정
        }
    }
    public EndingExplosion endingExplosion;
    IEnumerator EndingTimerCoroutine()
    {
        yield return new WaitForSeconds(30f); 
        RealtimeCanvasUI.Notification(IconType.Charge, "30초 남았습니다");
        yield return new WaitForSeconds(25f);
        RealtimeCanvasUI.Notification(IconType.Charge, "5....");
        yield return new WaitForSeconds(1f);
        RealtimeCanvasUI.Notification(IconType.Charge, "4...");
        yield return new WaitForSeconds(1f);
        RealtimeCanvasUI.Notification(IconType.Charge, "3..");
        yield return new WaitForSeconds(1f);
        RealtimeCanvasUI.Notification(IconType.Charge, "2.");
        yield return new WaitForSeconds(1f);
        RealtimeCanvasUI.Notification(IconType.Charge, "1");
        yield return new WaitForSeconds(1f);

        // 코어의 과부하 공격 활성화
        // 폭 발

        endingExplosion.StartExplosion();

        // 스폰 정지
        spawnManager.ClearEnding();

        SoundManager.Play("HappyEndingBGM", SoundType.Background);

        yield return new WaitForSeconds(5f);

        RealtimeCanvasUI.Notification(IconType.HappyEnding, "축하합니다");
        yield return new WaitForSeconds(3f);
        RealtimeCanvasUI.Notification(IconType.HappyEnding, "성공적으로 정화 작업을 마치셨습니다");
        yield return new WaitForSeconds(5f);
        ScreenTransition.Play("HappyEnding_FadeOut", "HappyEnding_FadeIn", Color.white, Color.white, "Title", 1f, 2.5f);
        yield return new WaitForSeconds(1f);
        RealtimeCanvasUI.Notification(IconType.HappyEnding, "좋은 하루 되시길 바랍니다");
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
