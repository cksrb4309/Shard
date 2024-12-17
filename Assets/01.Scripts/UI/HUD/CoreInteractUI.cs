using TMPro;
using UnityEngine;

public class CoreInteractUI : MonoBehaviour
{
    public Inventory inventory;
    public ParticleSystem successParticle;
    public void Connect(Inventory inventory)
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
    public TMP_Text maxHealthLevelUpCountText;
    public void UpgradeMaxHealth()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeMaxHealth();

        maxHealthLevelUpCountText.text = (int.Parse(maxHealthLevelUpCountText.text)+1).ToString();
    }
    public TMP_Text defenseLevelUpCountText;
    public void UpgrageDefense()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeDefense();

        defenseLevelUpCountText.text = (int.Parse(defenseLevelUpCountText.text) + 1).ToString();
    }
    public TMP_Text healthRegenLevelUpCountText;
    public void UpgradeHealthRegen()
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeHealthRegen();

        healthRegenLevelUpCountText.text = (int.Parse(healthRegenLevelUpCountText.text) + 1).ToString();
    }
    public void UpgradeUserStat(AbilityAndText info)
    {
        if (!IsCoreUpgrade()) return;

        coreUpgrade.UpgradeUserStat(info.ability);

        info.text.text = (int.Parse(info.text.text) + 1).ToString();
    }
    #endregion
    #region ��ȯ ��ư �Ҵ� �Լ���
    public CoreTeammateController teammateController;
    public void Summon(Teammate teammate)
    {
        if (!IsSummonUpgrade()) return;

        teammateController.Summon(teammate);
    }
    public TMP_Text summonAttackDamageLevelCountText;
    public void SummonAttackDamageUpgrade()
    {
        if (!IsSummonUpgrade()) return;

        teammateController.AttackDamageUpgrade();

        summonAttackDamageLevelCountText.text = (int.Parse(summonAttackDamageLevelCountText.text) + 1).ToString();
    }
    public TMP_Text summonAttackSpeedLevelCountText;
    public void SummonAttackSpeedUpgrade()
    {
        if (!IsSummonUpgrade()) return;

        teammateController.AttackSpeedUpgrade();

        summonAttackSpeedLevelCountText.text = (int.Parse(summonAttackSpeedLevelCountText.text) + 1).ToString();
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
            successParticle.Play();

            inventory.UseEnergyCore(currentCoreCost);

            currentCoreCost = (int)(currentCoreCost * nextCoreCostMultiplier);

            coreUpgradeCostText.text = "�ʿ� ���� : "+currentCoreCost.ToString();

            return true;
        }

        RealtimeCanvasUI.NotificationText("����� ���ڶ��ϴ�");

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
            successParticle.Play();

            inventory.UseSoulShard(currentSummonCost);

            currentSummonCost = (int)(currentSummonCost * nextSummonCostMultiplier);

            summonCostText.text = "�ʿ� ��ȥ : " + currentSummonCost.ToString();

            return true;
        }

        RealtimeCanvasUI.NotificationText("����� ���ڶ��ϴ�");

        return false;
    }
    #endregion

    private void Start()
    {
        coreUpgradeCostText.text = "�ʿ� ���� : " + currentCoreCost.ToString();
        summonCostText.text = "�ʿ� ��ȥ : " + currentSummonCost.ToString();
    }
}
