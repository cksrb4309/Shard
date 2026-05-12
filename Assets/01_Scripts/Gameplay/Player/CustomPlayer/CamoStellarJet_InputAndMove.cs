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
        if (WasSubSkillPressedThisFrame())
        {
            OnSubSkill();
        }
        if (mainSkillAction.action.WasPressedThisFrame())
        {
            OnMainSkill();
        }
    }
}
