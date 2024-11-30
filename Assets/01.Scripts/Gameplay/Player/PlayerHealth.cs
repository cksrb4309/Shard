using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    static PlayerHealth instance = null;

    public PlayerStatus playerStatus;
    public TMP_Text hpText;
    public Slider hpSlider;
    public Slider sheildSlider;

    float maxHp;
    float hp;
    float hpRegen = 0;

    float sheild = 0;
    float maxSheild;

    Coroutine sheildCoroutine = null;
    private void Awake()
    {
        instance = this;
    }
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
            maxSheild = value;

            hpSlider.maxValue = value;
            sheildSlider.maxValue = value;
        }
    }
    float Hp
    {
        set
        {
            hp = value;

            hpSlider.value = hp;

            hpText.text = hp.ToString("F0") + " / " + maxHp.ToString("F0");
        }
    }
    float Sheild
    {
        set
        {
            sheild = value;

            sheildSlider.value = sheild;

            hpText.text = (hp + sheild).ToString("F0") + " / " + maxHp.ToString("F0");
        }
    }
    public void InitializeHp(float maxHp)
    {
        MaxHp = maxHp;
        Hp = maxHp;
    }
    public void SetMaxHp(float maxHp)
    {
        float distance = maxHp - this.maxHp;

        MaxHp = maxHp;

        if (IsAlive())
            Hp = Mathf.Clamp(hp + distance, 1, maxHp);
    }
    public void SetHealthRegen(float hpRegen)
    {
        this.hpRegen = hpRegen;
    }
    public void Hit(float damage)
    {
        damage = UseSheild(damage);

        if (damage == 0) return;

        Hp = hp - damage;

        DamageTextController.OnDamageText(transform.position, damage, true);

        if (hp <= 0)
        {
            Hp = 0;

            Dead();
        }
    }
    float UseSheild(float damage)
    {
        // ���� ��ȣ���� ���� ���� damage�� �״�� ��ȯ
        if (sheild == 0) 
        {
            return damage;
        }

        // ��ȣ������ �������� ������ distance�� ����
        float distance = sheild - damage;

        // ���� distance�� 0���� �۴ٸ� sheild�� �� �� ���� �������� ��ȯ
        if (distance < 0)
        {
            DamageTextController.OnDamageText(transform.position, damage - sheild, true);

            // �� �����ұ� ������ sheild�� 0���� �ٲ�
            Sheild = 0;

            // ��ȣ���� �ʰ��� �������� ����� ���� (5�� ��ȣ�� 10�� ������ distance:(-5) -> 5 ��ȯ)
            return -distance; 
        }
        else
        {
            // sheild�� ������ �����ߴٸ� 0�� ��ȯ

            DamageTextController.OnDamageText(transform.position, damage, true);

            Sheild = distance;

            return 0;
        }

    }
    public void Heal(float heal)
    {
        if (!IsAlive()) return;

        if (hp >= maxHp) return;

        hp += heal;

        if (hp > maxHp) Hp = maxHp;
    }
    void GetSheildApply(float sheild)
    {
        if (!IsAlive()) return;

        this.sheild += sheild;

        if (this.sheild > maxSheild) this.sheild = maxSheild;

        Sheild = this.sheild;

        if (sheildCoroutine == null) 
        {
            sheildCoroutine = StartCoroutine(SheildCoroutine());
            Debug.Log("�ǵ� �ڷ�ƾ ����");
        }
    }
    public static void GetSheild(float sheild) => instance.GetSheildApply(sheild);
    IEnumerator SheildCoroutine()
    {
        float amount = maxSheild * 0.01f;

        while (sheild > 0)
        {
            yield return new WaitForSeconds(0.1f);

            Sheild = sheild - amount;
        }

        Debug.Log("�ǵ� �ڷ�ƾ ����");

        sheildCoroutine = null;

        Sheild = 0;
    }
    void Dead()
    {
        Debug.Log("���");
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
        return hp > 0;
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