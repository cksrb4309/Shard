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
    private void Start()
    {
        GameManager.Instance.AddPlayer(transform);
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

        Debug.Log("현재 maxHp:" + this.maxHp.ToString() + " / 설정 maxHp:" + maxHp.ToString());

        MaxHp = maxHp;

        Hp = Mathf.Clamp(hp + distance, 1, maxHp);
    }
    public void Hit(float damage)
    {
        Debug.Log("맞기 전 체력:" + Hp.ToString());

        Hp -= damage;
        Debug.Log("데미지 : " + damage.ToString());
        Debug.Log("Hit 남은 hp:" + Hp.ToString());

        if (Hp <= 0)
        {
            Dead();
        }
    }
    public void Heal(float heal)
    {
        if (!IsAlive()) return;

        Hp += heal;
    }
    void Dead()
    {
        Debug.Log("사망");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage:" + damage.ToString());
        
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
}