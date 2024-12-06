using TMPro;
using UnityEngine;

public class CoreInteractUI : MonoBehaviour
{
    public Inventory inventory;

    public void UIConnect(Inventory inventory)
    {
        this.inventory = inventory;
    }

    #region �г� Ȱ��ȭ �� ��Ȱ��ȭ
    public GameObject mainPanel;
    public void ShowPanel()
    {
        mainPanel.SetActive(true);
    }
    public void HidePanel()
    {
        mainPanel.SetActive(false);
    }
    #endregion
    #region Ȱ��ȭ �� ��ȯ
    public GameObject coreUpgradePanel;
    public GameObject summonPanel;

    public void ActivateCoreUpgradePanel()
    {
        coreUpgradePanel.SetActive(true);
        summonPanel.SetActive(false);
    }
    public void ActivateSummonPanel()
    {
        coreUpgradePanel.SetActive(false);
        summonPanel.SetActive(true);
    }
    #endregion
    #region �ھ� ��ư �Ҵ� �Լ���
    public CoreUpgrade coreUpgrade;
    public void UpgradeMaxHealth()
    {
        Debug.Log("�ִ� ü�� ���� �õ�");

        if (!IsCoreUpgrade()) return;

        Debug.Log("�ִ� ü�� ���� ����");

        coreUpgrade.UpgradeMaxHealth();
    }
    public void UpgrageDefense()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeDefense();
    }
    public void UpgradeHealthRegen()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeHealthRegen();
    }
    public void UpgradeUserAttackDamage()
    {
        Debug.Log("���ݷ� ���� �õ�");

        if (!IsCoreUpgrade()) return;

        Debug.Log("���ݷ� ���� ����");
        coreUpgrade.UpgradeUserAttackDamage();
    }
    public void UpgradeUserAttackSpeed()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserAttackSpeed();
    }
    public void UpgradeUserCriticalChance()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserCriticalChance();
    }
    public void UpgradeUserCriticalDamage()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserCriticalDamage();
    }
    public void UpgradeUserMaxHealth()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserMaxHealth();
    }
    public void UpgradeUserHealthRegen()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserHealthRegen();
    }
    public void UpgradeUserMoveSpeed()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserMoveSpeed();
    }
    public void UpgradeUserStat(Ability ability)
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserStat(ability);
    }
    #endregion
    #region ��ȯ ��ư �Ҵ� �Լ���
    public CoreTeammateController teammateController;
    public void Summon(Teammate teammate)
    {
        if (!IsSummonUpgrade()) return;

        teammateController.Summon(teammate);
    }
    public void SummonAttackDamageUpgrade()
    {
        if (!IsSummonUpgrade()) return;

        teammateController.AttackDamageUpgrade();
    }
    public void SummonAttackSpeedUpgrade()
    {
        if (!IsSummonUpgrade()) return;

        teammateController.AttackSpeedUpgrade();
    }
    #endregion
    #region �ھ� ���� ��� Ȯ�� �Լ�
    public TMP_Text coreUpgradeCostText;
    public int currentCoreCost = 10;
    public float nextCoreCostMultiplier = 1.5f;
    bool IsCoreUpgrade()
    {
        if (inventory.GetEnergyCore() >= currentCoreCost)
        {
            inventory.UseEnergyCore(currentCoreCost);

            currentCoreCost = (int)(currentCoreCost * nextCoreCostMultiplier);

            coreUpgradeCostText.text = "�ʿ� ���� : "+currentCoreCost.ToString();

            return true;
        }
        return false;
    }
    #endregion
    #region ��ȯ ��� Ȯ�� �Լ�
    public TMP_Text summonCostText;
    public int currentSummonCost = 5;
    public float nextSummonCostMultiplier = 1.3f;
    bool IsSummonUpgrade()
    {
        if (inventory.GetSoulShard() >= currentSummonCost)
        {
            inventory.UseSoulShard(currentSummonCost);

            currentSummonCost = (int)(currentSummonCost * nextSummonCostMultiplier);

            summonCostText.text = "�ʿ� ��ȥ : " + currentSummonCost.ToString();

            return true;
        }
        return false;
    }
    #endregion

    private void Start()
    {
        coreUpgradeCostText.text = "�ʿ� ���� : " + currentCoreCost.ToString();
        summonCostText.text = "�ʿ� ��ȥ : " + currentSummonCost.ToString();
    }
}
