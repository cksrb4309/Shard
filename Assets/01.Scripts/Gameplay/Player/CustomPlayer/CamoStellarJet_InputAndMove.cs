using UnityEngine;

public class CamoStellarJet_InputAndMove : CustomPlayerInputAndMove
{
    // CamoStellarJet������ ������ ������ ���� �� �����ϴ� ��ɸ��� �ʿ��ϴ�
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
