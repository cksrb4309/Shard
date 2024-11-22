using UnityEngine;

public class CoreHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 1000f;
    float currentHealth;

    public float defense = 0;

    public float healthRegen = 1;

    private void Start()
    {
        currentHealth = maxHealth;
    }
    public void UpgradeMaxHealth()
    {
        float beforeMaxHealth = maxHealth;
        maxHealth *= 1.1f;

        currentHealth += maxHealth - beforeMaxHealth;
    }
    public void UpgradeDefense()
    {
        defense += 2;
    }
    public void UpgradeHealthRegen()
    {
        healthRegen *= 1.2f;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (!IsAlive()) Dead();
    }
    public void TakeDebuff(float damage, StatusEffect statusEffect)
    {
        TakeDamage(damage);
    }
    public bool IsAlive() => currentHealth > 0;
    void Dead()
    {
        // TODO 결정체의 체력이 모두 달았을 때
    }
}
