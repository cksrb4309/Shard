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
            Debug.Log("Vignette�� ���������� �����Խ��ϴ�");

            ChangeStageColor(); // �������� �÷� ����
        }
        else
        {
            Debug.LogWarning("Vignette �������� ���� !!!");
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
        searchNearPlayersObj.SetActive(true); // ã�� Ȱ��ȭ

        while (!isAroundPlayers) yield return null;

        blockGroup.StageClear(); // �� �ٽ� ���� ����

        // �ھ� �ʿ� ��ƼŬ �߰������� ����ϴ���...
        // ����ٰ� ��ƼŬ �߰�
        stageChangeParticle.Play();

        StartCoroutine(NextStageColorCoroutine());

        yield return new WaitForSeconds(1f);

        spawnManager.StartMonsterSpawn(); // ���� ���� ����

        DifficultyManager.NextStageSetting(); // ���̵� ����
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
