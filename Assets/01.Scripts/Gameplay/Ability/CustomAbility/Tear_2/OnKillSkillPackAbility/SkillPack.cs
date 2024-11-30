using UnityEngine;

public class SkillPack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerSkill[] playerSkills = other.GetComponents<PlayerSkill>();

        foreach (PlayerSkill playerSkill in playerSkills) playerSkill.ResetCooltime();

        PoolingManager.Instance.ReturnObject("SkillPack", gameObject);
    }
}
