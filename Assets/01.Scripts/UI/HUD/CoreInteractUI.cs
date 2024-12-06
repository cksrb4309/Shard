using TMPro;
using UnityEngine;

public class CoreInteractUI : MonoBehaviour
{
    public Inventory inventory;

    public void UIConnect(Inventory inventory)
    {
        this.inventory = inventory;
    }

    #region 패널 활성화 및 비활성화
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
    #region 활성화 탭 전환
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
    #region 코어 버튼 할당 함수들
    public CoreUpgrade coreUpgrade;
    public void UpgradeMaxHealth()
    {
        Debug.Log("최대 체력 업글 시도");

        if (!IsCoreUpgrade()) return;

        Debug.Log("최대 체력 업글 성공");

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
        Debug.Log("공격력 업글 시도");

        if (!IsCoreUpgrade()) return;

        Debug.Log("공격력 업글 성공");
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
    #region 소환 버튼 할당 함수들
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
    #region 코어 업글 비용 확인 함수
    public TMP_Text coreUpgradeCostText;
    public int currentCoreCost = 10;
    public float nextCoreCostMultiplier = 1.5f;
    bool IsCoreUpgrade()
    {
        if (inventory.GetEnergyCore() >= currentCoreCost)
        {
            inventory.UseEnergyCore(currentCoreCost);

            currentCoreCost = (int)(currentCoreCost * nextCoreCostMultiplier);

            coreUpgradeCostText.text = "필요 파편 : "+currentCoreCost.ToString();

            return true;
        }
        return false;
    }
    #endregion
    #region 소환 비용 확인 함수
    public TMP_Text summonCostText;
    public int currentSummonCost = 5;
    public float nextSummonCostMultiplier = 1.3f;
    bool IsSummonUpgrade()
    {
        if (inventory.GetSoulShard() >= currentSummonCost)
        {
            inventory.UseSoulShard(currentSummonCost);

            currentSummonCost = (int)(currentSummonCost * nextSummonCostMultiplier);

            summonCostText.text = "필요 영혼 : " + currentSummonCost.ToString();

            return true;
        }
        return false;
    }
    #endregion

    private void Start()
    {
        coreUpgradeCostText.text = "필요 파편 : " + currentCoreCost.ToString();
        summonCostText.text = "필요 영혼 : " + currentSummonCost.ToString();
    }
}
