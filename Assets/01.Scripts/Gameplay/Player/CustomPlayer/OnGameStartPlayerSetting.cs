using UnityEngine;

// ���� ���� �� ������ ������ ��ũ��Ʈ !
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
        // ���׷��̵� �г� ����
        foreach (Ability ability in playerAttributes.upgradeAbilityArray)
        {
            GameSceneUIConnectManager.GetUpgradePanel().Setting(ability);
        }

        // ü�� �� ���� ����
        playerHealth.Connect(
            GameSceneUIConnectManager.GetHpSlider(),
            GameSceneUIConnectManager.GetHpText(),
            GameSceneUIConnectManager.GetSheildSlider());

        // ��ų ����
        GameSceneUIConnectManager.GetNormalAttackImage().sprite = normalAttack.skillIcon;
        subSkill.Connect(GameSceneUIConnectManager.GetSubSkillSlot());
        mainSkill.Connect(GameSceneUIConnectManager.GetMainSkillSlot());

        // �κ��丮 ���� [ �ھ� UI�� ���� ]
        GameSceneUIConnectManager.GetCoreInteractUI().Connect(inventory);

        GameSceneUIConnectManager.GetCoreUpgrade().Connect(inventory);


        GameSceneUIConnectManager.GetRealtimeCanvasUI().SetPlayerTransform(transform);
    }
}
