using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StageManager : MonoBehaviour
{
    static StageManager instance = null;

    public SpawnManager spawnManager = null;

    public Color[] stageColors;

    public Volume volume; // Vignette Color�� �ٲٱ� ���� ����

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
            ChangeStageColor(); // �������� �÷� ����
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
        RealtimeCanvasUI.Notification(IconType.Charge, corePosition.position, "�̹� ������ ���� �������� ��� �����߽��ϴ�");

        yield return new WaitForSeconds(2f);

        RealtimeCanvasUI.Notification(IconType.Charge, "����ü�� ���ư��� ���� �������� �̵����ּ���");
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
        // ���� ���� ���������� 2�� ����
        // ������ ���� ������ ��������
        if (currentStage == 2) {
            currentStage = 3; // ���� �������� 3���� ������� (���� ���������� ���� ���� ������ ����)

            yield return new WaitForSeconds(1f); // 1�� ����

            SoundManager.Play("EndingBattleBGM", SoundType.Background);

            // 1�� �ھ ������ �����ؾ��� TODO �ھ� ���� ���� (��ƼŬ�� ǥ�� �� Notification ���)
            coreChargeParticle_1.Play(); // ��ƼŬ
            coreChargeParticle_2.Play(); // ��ƼŬ

            RealtimeCanvasUI.Notification(IconType.Charge, coreTransform.position, "����ü�� ����� �������� �𿴽��ϴ�");
            yield return new WaitForSeconds(4f); // 4�� ����
            RealtimeCanvasUI.Notification(IconType.Charge, "����ü�� �����ϰ� �����մϴ�");
            yield return new WaitForSeconds(4f); // 4�� ����
            RealtimeCanvasUI.Notification(IconType.Charge, "������ü���� �̻����� ������ �𿩵�� �����߽��ϴ�");
            yield return new WaitForSeconds(4f); // 4�� ����
            RealtimeCanvasUI.Notification(IconType.Charge, "����ü�� ��ȭ�� ���� ������ �����ּ��� !");
            // TODO ���� ����� ����

            // 2�� ���Ͱ� �ܰ����� �����ؾ���
            spawnManager.EndingSpawn(); // ���� �ѱ�

            // 3�� �������� ��ƿ ���� ���ؼ�... (1. �ڷ�ƾ �ð� ó�� 2. �� óġ �� �ݿ�)
            StartCoroutine(EndingTimerCoroutine()); // 1��
        }
        // ���� ���������� 2�� �ƴ϶�� [0, 1 �� ��] ���� �������� �غ� ���ش�
        else
        {
            StartCoroutine(NextLevelCoroutine());

            searchNearPlayersObj.SetActive(true); // ã�� Ȱ��ȭ

            while (!isAroundPlayers) yield return null;

            blockGroup.StageClear(); // �� �ٽ� ���� ����

            // �ھ� �ʿ� ��ƼŬ �߰������� ����ϴ���...
            // ����ٰ� ��ƼŬ �߰�
            stageChangeParticle_1.Play();
            stageChangeParticle_2.Play();

            StartCoroutine(NextStageColorCoroutine());

            yield return new WaitForSeconds(1f);

            spawnManager.StartMonsterSpawn(); // ���� ���� ����

            DifficultyManager.NextStageSetting(); // ���̵� ����
        }
    }
    public EndingExplosion endingExplosion;
    IEnumerator EndingTimerCoroutine()
    {
        yield return new WaitForSeconds(30f); 
        RealtimeCanvasUI.Notification(IconType.Charge, "30�� ���ҽ��ϴ�");
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

        // �ھ��� ������ ���� Ȱ��ȭ
        // �� ��

        endingExplosion.StartExplosion();

        // ���� ����
        spawnManager.ClearEnding();

        SoundManager.Play("HappyEndingBGM", SoundType.Background);

        yield return new WaitForSeconds(5f);

        RealtimeCanvasUI.Notification(IconType.HappyEnding, "�����մϴ�");
        yield return new WaitForSeconds(3f);
        RealtimeCanvasUI.Notification(IconType.HappyEnding, "���������� ��ȭ �۾��� ��ġ�̽��ϴ�");
        yield return new WaitForSeconds(5f);
        ScreenTransition.Play("HappyEnding_FadeOut", "HappyEnding_FadeIn", Color.white, Color.white, "Title", 1f, 2.5f);
        yield return new WaitForSeconds(1f);
        RealtimeCanvasUI.Notification(IconType.HappyEnding, "���� �Ϸ� �ǽñ� �ٶ��ϴ�");
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
