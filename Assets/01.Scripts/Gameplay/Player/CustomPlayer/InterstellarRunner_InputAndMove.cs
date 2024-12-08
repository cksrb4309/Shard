using UnityEngine;

public class InterstellarRunner_InputAndMove : CustomPlayerInputAndMove
{
    public override void Start()
    {
        base.Start();
    }

    bool isCharged = false;
    bool isNormalAttack = false;
    bool isSubSkill = false;
    bool isMainSkill = false;

    void Update()
    {
        if (!isSubSkill && !isMainSkill)
        {
            if (normalAttackAction.action.IsPressed())
            {
                if (!isCharged) 
                {
                    isCharged = true;

                    isNormalAttack = true;

                    OnNormalAttack();
                }
            }
            else
            {
                if (isCharged && isNormalAttack)
                {
                    isCharged = false;

                    OnNormalAttack();

                    isNormalAttack = false;
                }
            }
        }

        if (!isNormalAttack && !isMainSkill)
        {
            if (subSkillAction.action.IsPressed())
            {
                if (!isCharged)
                {
                    isCharged = true;

                    isSubSkill = true;

                    OnSubSkill();
                }
            }
            else
            {
                if (isCharged && isSubSkill)
                {
                    isCharged = false;

                    OnSubSkill();

                    isSubSkill = false;
                }
            }
        }
        if (!isNormalAttack && !isSubSkill)
        {
            if (mainSkillAction.action.IsPressed())
            {
                if (!isCharged)
                {
                    isCharged = true;

                    isMainSkill = true;

                    OnMainSkill();
                }
            }
            else
            {
                if (isCharged && isMainSkill)
                {
                    isCharged = false;

                    OnMainSkill();

                    isMainSkill = false;
                }
            }
        }
    }
}
