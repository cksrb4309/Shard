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
        StartCoroutine(RegenCoroutine());
    }
    float MaxHp
    {
        get { return maxHp; }
        set
        {
            maxHp = value;
            maxSheild = value;

            if (hpSlider != null) hpSlider.maxValue = value;
            if (sheildSlider != null) sheildSlider.maxValue = value;
        }
    }
    float Hp
    {
        set
        {
            hp = value;

            if (hpSlider != null) hpSlider.value = hp;

            if (hpText != null) hpText.text = hp.ToString("F0") + " / " + maxHp.ToString("F0");
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
        // 만약 보호막이 없을 때는 damage를 그대로 반환
        if (sheild == 0) 
        {
            return damage;
        }

        // 보호막에서 데미지를 차감한 distance를 저장
        float distance = sheild - damage;

        // 만약 distance가 0보다 작다면 sheild로 다 못 막은 데미지를 반환
        if (distance < 0)
        {
            DamageTextController.OnDamageText(transform.position, damage - sheild, true);

            // 다 못막았기 때문에 sheild를 0으로 바꿈
            Sheild = 0;

            // 보호막을 초과한 데미지를 양수로 전달 (5의 보호막 10의 데미지 distance:(-5) -> 5 반환)
            return -distance; 
        }
        else
        {
            // sheild로 차감이 가능했다면 0을 반환

            DamageTextController.OnDamageText(transform.position, damage, true);

            Sheild = distance;

            return 0;
        }

    }
    public void Heal(float heal)
    {
        if (!IsAlive()) return;

        if (hp >= maxHp) return;

        Hp = hp + heal;

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

        sheildCoroutine = null;

        Sheild = 0;
    }
    void Dead()
    {
        // 파티클 효과 재생
        ParticleManager.Play(transform.position, HitEffectName.PlayerDeadParticle);

        // 게임 매니저에 사망 처리 올림
        GameManager.PlayerDie();

        // 오브젝트 비활성화
        gameObject.SetActive(false); 
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
    public void Connect(Slider hpSlider, TMP_Text hpText, Slider sheildSlider)
    {
        this.hpSlider = hpSlider;
        this.hpText = hpText;
        this.sheildSlider = sheildSlider;
    }
}