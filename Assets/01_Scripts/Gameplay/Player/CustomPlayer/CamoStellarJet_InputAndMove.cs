using UnityEngine;

public class CamoStellarJet_InputAndMove : CustomPlayerInputAndMove
{
    // CamoStellarJet에서는 공격이 눌리고 있을 때 공격하는 기능만이 필요하다
    private void Update()
    {
        if (normalAttackAction.action.IsPressed())
        {
            OnNormalAttack();
        }
        if (subSkillAction.action.IsPressed())
        {
            OnSubSkill();
        }
        if (mainSkillAction.action.IsPressed())
        {
            OnMainSkill();
        }
    }
}
