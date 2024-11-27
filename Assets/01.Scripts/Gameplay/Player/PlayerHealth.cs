using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public PlayerStatus playerStatus;
    public TMP_Text hpText;
    public Slider hpSlider;
    
    float maxHp;
    float hp;
    float hpRegen = 0;
    private void Start()
    {
        GameManager.Instance.AddPlayer(transform);

        StartCoroutine(RegenCoroutine());
    }
    float MaxHp
    {
        get { return maxHp; }
        set
        {
            maxHp = value;

            hpSlider.maxValue = value;
        }
    }
    float Hp
    {
        get { return hp; }
        set
        {
            hp = Mathf.Clamp(value, 0, maxHp);

            hpSlider.value = hp;

            hpText.text = hp.ToString("F0") + " / " + maxHp.ToString("F0");
        }
    }
    public void InitializeHp(float maxHp)
    {
        Debug.Log("초기화 체력 : " + maxHp.ToString());

        MaxHp = maxHp;
        Hp = maxHp;
    }
    public void SetMaxHp(float maxHp)
    {
        float distance = maxHp - this.maxHp;

        MaxHp = maxHp;

        Hp = Mathf.Clamp(hp + distance, 1, maxHp);
    }
    public void SetHealthRegen(float hpRegen)
    {
        Debug.Log("HpRegen : " + hpRegen.ToString());
        this.hpRegen = hpRegen;
    }
    public void Hit(float damage)
    {
        Hp -= damage;

        if (Hp <= 0)
        {
            Dead();
        }
    }
    public void Heal(float heal)
    {
        if (!IsAlive()) return;

        if (maxHp == hp) return;

        Hp += heal;

        if (Hp > maxHp) Hp = maxHp;
    }
    void Dead()
    {
        Debug.Log("사망");
    }

    public void TakeDamage(float damage)
    {
        Hit(damage);
    }

    public void TakeDebuff(float damage, StatusEffect statusEffect)
    {
        Hit(damage);

        playerStatus.GetStatusEffectApply(statusEffect);
    }

    public bool IsAlive()
    {
        return Hp > 0;
    }
    IEnumerator RegenCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            Heal(hpRegen);
        }
    }
}