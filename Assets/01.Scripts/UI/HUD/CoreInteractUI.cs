using TMPro;
using UnityEngine;

public class CoreInteractUI : MonoBehaviour
{
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
    #region ��ư �Ҵ� �Լ���
    public CoreUpgrade coreUpgrade;
    public void UpgradeMaxHealth()
    {
        if (!IsUpgrade()) return;

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
        if (!IsUpgrade()) return;

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
    #region ��� Ȯ�� �Լ�
    public Inventory inventory;
    public TMP_Text[] costTextArray;
    public int currentCost = 10;
    public float nextCostMultiplier = 1.5f;

    bool IsUpgrade()
    {
        if (inventory.GetEnergyCore() > currentCost)
        {
            inventory.UseEnergyCore(currentCost);

            currentCost = (int)(currentCost * nextCostMultiplier);

            for (int i = 0; i < costTextArray.Length; i++) costTextArray[i].text = "Cost: "+currentCost.ToString();

            return true;
        }

        return false;
    }
    private void Start()
    {
        for (int i = 0; i < costTextArray.Length; i++) costTextArray[i].text = "Cost: " + currentCost.ToString();
    }
    #endregion
}
