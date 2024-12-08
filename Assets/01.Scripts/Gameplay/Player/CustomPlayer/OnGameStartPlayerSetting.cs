using UnityEngine;

// 게임 시작 시 세팅을 시작할 스크립트 !
public class OnGameStartPlayerSetting : MonoBehaviour
{
    public PlayerAttributes playerAttributes;
    public PlayerHealth playerHealth;
    public PlayerSkill normalAttack;
    public PlayerSkill subSkill;
    public PlayerSkill mainSkill;
    public Inventory inventory;
    private void Start()
    {
        // 업그레이드 패널 세팅
        foreach (Ability ability in playerAttributes.upgradeAbilityArray)
        {
            GameSceneUIConnectManager.GetUpgradePanel().Setting(ability);
        }

        // 체력 및 쉴드 세팅
        playerHealth.Connect(
            GameSceneUIConnectManager.GetHpSlider(),
            GameSceneUIConnectManager.GetHpText(),
            GameSceneUIConnectManager.GetSheildSlider());

        // 스킬 세팅
        GameSceneUIConnectManager.GetNormalAttackImage().sprite = normalAttack.skillIcon;
        subSkill.Connect(GameSceneUIConnectManager.GetSubSkillSlot());
        mainSkill.Connect(GameSceneUIConnectManager.GetMainSkillSlot());

        // 인벤토리 세팅 [ 코어 UI와 연결 ]
        GameSceneUIConnectManager.GetCoreInteractUI().Connect(inventory);

        GameSceneUIConnectManager.GetCoreUpgrade().Connect(inventory);


        GameSceneUIConnectManager.GetRealtimeCanvasUI().SetPlayerTransform(transform);
    }
}
