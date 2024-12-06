using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    static ParticleManager instance = null;
    public ParticleSystem[] hit_Effects;
    public ParticleSystem[] other_Effects;

    private void Awake()
    {
        instance = this;
    }

    public static void Play(Vector3 position, HitEffectName particleName)
    {
        instance.hit_Effects[(int)particleName].transform.position = position;
        instance.hit_Effects[(int)particleName].Play();
    }
    public static void Play(Vector3 position, OtherEffectName particleName)
    {
        instance.other_Effects[(int)particleName].transform.position = position;
        instance.other_Effects[(int)particleName].Play();
    }
}
public enum HitEffectName
{
    CamoStellarJetNormalAttackHit,
    CamoStellarJetSubSkillHit,
    CamoStellarJetMainSkillHit,
}
public enum OtherEffectName
{
    // MonsterSpawnEffect,
}