using System;
using UnityEngine;
using UnityEngine.Events;

public class CoreUpgrade : MonoBehaviour
{
    public CoreHealth coreHealth;
    public Inventory inventory;

    public TempAbility coreUpgradeAttackDamage;
    public TempAbility coreUpgradeAttackSpeed;
    public TempAbility coreUpgradeCriticalChance;
    public TempAbility coreUpgradeCriticalDamage;
    public TempAbility coreUpgradeMaxHealth;
    public TempAbility coreUpgradeHealthRegen;
    public TempAbility coreUpgradeMoveSpeed;

    #region 코어 강화
    public void UpgradeMaxHealth()
    {
        coreHealth.UpgradeMaxHealth();
    }
    public void UpgradeDefense()
    {
        coreHealth.UpgradeDefense();
    }
    public void UpgradeHealthRegen()
    {
        coreHealth.UpgradeHealthRegen();
    }
    #endregion
    #region 유저 강화
    public void UpgradeUserAttackDamage()
    {
        Debug.Log("---------------!!!!!!!  공격력 증가");
        inventory.GetAbilityApply(coreUpgradeAttackDamage);
    }
    public void UpgradeUserAttackSpeed()
    {
        inventory.GetAbilityApply(coreUpgradeAttackSpeed);
    }
    public void UpgradeUserCriticalChance()
    {
        inventory.GetAbilityApply(coreUpgradeCriticalChance);
    }
    public void UpgradeUserCriticalDamage()
    {
        inventory.GetAbilityApply(coreUpgradeCriticalDamage);
    }
    public void UpgradeUserMaxHealth()
    {
        inventory.GetAbilityApply(coreUpgradeMaxHealth);
    }
    public void UpgradeUserHealthRegen()
    {
        inventory.GetAbilityApply(coreUpgradeHealthRegen);
    }
    public void UpgradeUserMoveSpeed()
    {
        inventory.GetAbilityApply(coreUpgradeMoveSpeed);
    }
    #endregion
}