using System.Collections.Generic;
using UnityEngine;

public class CoreTeammateController : MonoBehaviour
{
    List<Teammate> teammates = new List<Teammate>();

    int attackDamageUpgradeCount = 1;
    int attackSpeedUpgradeCount = 1;

    public void Summon(Teammate teammate)
    {
        Teammate mate = Instantiate(teammate);

        mate.transform.position = transform.position;

        mate.AttackDamageUpgrade(attackDamageUpgradeCount);
        mate.AttackSpeedUpgrade(attackSpeedUpgradeCount);

        teammates.Add(mate);

        mate.Setting(transform.position);
    }
    public void AttackDamageUpgrade()
    {
        attackDamageUpgradeCount++;

        foreach (Teammate teammate in teammates) teammate.AttackDamageUpgrade(attackDamageUpgradeCount);
    }
    public void AttackSpeedUpgrade()
    {
        attackSpeedUpgradeCount++;

        foreach (Teammate teammate in teammates) teammate.AttackSpeedUpgrade(attackSpeedUpgradeCount);
    }
}
