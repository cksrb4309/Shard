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

    Vignette vignette;

    int currentStage = 0;

    [HideInInspector] public bool isAroundPlayers = false;


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
        vignette.color.value = stageColors[currentStage];
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
        while (!isAroundPlayers) yield return null;

        blockGroup.StageClear(); // �� �ٽ� ���� ����

        // �ھ� �ʿ� ��ƼŬ �߰������� ����ϴ���...
        // ����ٰ� ��ƼŬ �߰�

        yield return new WaitForSeconds(1f);

        spawnManager.StartMonsterSpawn();
    }
}
