using System;
using UnityEngine;
using UnityEngine.Events;

public class CoreUpgrade : MonoBehaviour
{
    public CoreHealth coreHealth;
    public Inventory inventory;

    public Ability coreUpgradeAttackDamage;
    public Ability coreUpgradeAttackSpeed;
    public Ability coreUpgradeCriticalChance;
    public Ability coreUpgradeCriticalDamage;
    public Ability coreUpgradeMaxHealth;
    public Ability coreUpgradeHealthRegen;
    public Ability coreUpgradeMoveSpeed;

    #region �ھ� ��ȭ
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
    #region ���� ��ȭ
    public void UpgradeUserAttackDamage()
    {
        Debug.Log("---------------!!!!!!!  ���ݷ� ����");
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
    public void UpgradeUserStat(Ability ability)
    {
        inventory.GetAbilityApply(ability);
    }
    #endregion
}