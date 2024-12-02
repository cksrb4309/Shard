using UnityEngine;

public class BossMonster : DefaultMonster
{
    public override void Dead()
    {
        base.Dead();

        StageManager.OnKillBoss();
    }
}
