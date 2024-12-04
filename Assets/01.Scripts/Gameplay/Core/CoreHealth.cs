using UnityEngine;

public class CoreHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 1000f;
    float currentHealth;

    public float defense = 0;

    public float healthRegen = 1;

    public Material coreMaterial;
    private void Start()
    {
        currentHealth = maxHealth;

        coreMaterial.SetFloat("_DamageLevel", 0);
    }
    public void UpgradeMaxHealth()
    {
        float beforeMaxHealth = maxHealth;

        maxHealth *= 1.1f;

        currentHealth += maxHealth - beforeMaxHealth;

        coreMaterial.SetFloat("_DamageLevel", 1 - currentHealth / maxHealth);
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

        if (currentHealth < 0) currentHealth = 0;

        coreMaterial.SetFloat("_DamageLevel", 1 - currentHealth / maxHealth);

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
