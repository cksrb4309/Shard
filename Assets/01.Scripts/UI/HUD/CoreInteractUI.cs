using TMPro;
using UnityEngine;

public class CoreInteractUI : MonoBehaviour
{
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
    #region 버튼 할당 함수들
    public CoreUpgrade coreUpgrade;
    public void UpgradeMaxHealth()
    {
        Debug.Log("최대 체력 업글 시도");

        if (!IsUpgrade()) return;

        Debug.Log("최대 체력 업글 성공");

        coreUpgrade.UpgradeMaxHealth();
    }
    public void UpgrageDefense()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeDefense();
    }
    public void UpgradeHealthRegen()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeHealthRegen();
    }
    public void UpgradeUserAttackDamage()
    {
        Debug.Log("공격력 업글 시도");

        if (!IsUpgrade()) return;

        Debug.Log("공격력 업글 성공");
        coreUpgrade.UpgradeUserAttackDamage();
    }
    public void UpgradeUserAttackSpeed()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeUserAttackSpeed();
    }
    public void UpgradeUserCriticalChance()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeUserCriticalChance();
    }
    public void UpgradeUserCriticalDamage()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeUserCriticalDamage();
    }
    public void UpgradeUserMaxHealth()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeUserMaxHealth();
    }
    public void UpgradeUserHealthRegen()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeUserHealthRegen();
    }
    public void UpgradeUserMoveSpeed()
    {
        if (!IsUpgrade()) return;

        coreUpgrade.UpgradeUserMoveSpeed();
    }
    #endregion
    #region 비용 확인 함수
    public Inventory inventory;
    public TMP_Text[] coreUpgradeCostText;
    public int currentCost = 10;
    public float nextCostMultiplier = 1.5f;

    bool IsUpgrade()
    {
        if (inventory.GetEnergyCore() >= currentCost)
        {
            inventory.UseEnergyCore(currentCost);

            currentCost = (int)(currentCost * nextCostMultiplier);

            for (int i = 0; i < coreUpgradeCostText.Length; i++) coreUpgradeCostText[i].text = "Cost: "+currentCost.ToString();

            Debug.Log("비용 통과");

            return true;
        }

        Debug.Log("비용 막힘");

        return false;
    }
    private void Start()
    {
        for (int i = 0; i < coreUpgradeCostText.Length; i++) coreUpgradeCostText[i].text = "Cost: " + currentCost.ToString();
    }
    #endregion
}
