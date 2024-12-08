using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

public class CoreHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 1000f;
    float currentHealth;

    public float defense = 0;

    public float healthRegen = 1;

    public Material coreMaterial;

    public Collider myCollider;

    public GameObject sheild;
    public GameObject brokenSheild;

    public CoreHeart coreHeart;

    private void Start()
    {
        currentHealth = maxHealth;

        coreMaterial.SetFloat("_DamageLevel", 0);
        coreMaterial.SetFloat("_Alpha", 0);

        StartCoroutine(HealthRegenCoroutine());

        StartCoroutine(SearchCoroutine());
    }
    IEnumerator HealthRegenCoroutine()
    {
        while(IsAlive())
        {
            currentHealth += healthRegen;

            coreMaterial.SetFloat("_DamageLevel", 1 - currentHealth / maxHealth);

            yield return new WaitForSeconds(1f);
        }
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
    float GetDamageReduction()
    {
        if (defense <= 0) return 0f;                                // 0일 때
        if (defense <= 5) return defense * 8f;                      // 0 ~ 5 구간: 1당 8%씩 증가
        if (defense <= 10) return 40f + (defense - 5) * 5f;         // 5 ~ 10 구간: 1당 5%씩 증가
        if (defense <= 15) return 65f + (defense - 10) * 3f;        // 10 ~ 15 구간: 1당 3%씩 증가
        if (defense <= 20) return 80f + (defense - 15) * 2;         // 15 ~ 20 구간: 1당 2%씩 증가
        return 65f + (defense - 20) * 1f;                           // 20 이상은 1당 1%씩 증가
    }

    Coroutine notificationDelay = null;
    public void TakeDamage(float damage)
    {
        SoundManager.SheildAttackSoundPlay();

        // 데미지 방어력만큼 줄이기
        damage -= (damage * 0.01f * GetDamageReduction());

        // 데미지 적용하기
        currentHealth -= damage;
        
        // 데미지 텍스트 띄우기
        DamageTextController.OnDamageText(transform.position, damage, true);

        // 재질 데미지 적용
        coreMaterial.SetFloat("_DamageLevel", 1 - currentHealth / maxHealth);

        if (notificationDelay == null)
        {
            notificationDelay = StartCoroutine(DelayCoroutine());
            RealtimeCanvasUI.Notification(IconType.Warning, transform.position, "결정체가 공격 받고 있습니다 !");
        }

        if (currentHealth < 0) currentHealth = 0;

        if (isShowSheild) waitSecond = 5;

        else StartCoroutine(ShowCoroutine());

        if (!IsAlive()) 
        {
            Dead();
        }
    }
    IEnumerator DelayCoroutine()
    {
        yield return new WaitForSeconds(2f);

        notificationDelay = null;
    }
    public void TakeDebuff(float damage, StatusEffect statusEffect)
    {
        TakeDamage(damage);
    }
    public bool IsAlive() => currentHealth > 0;
    void Dead() // TODO 결정체의 체력이 모두 달았을 때
    {
        // 충돌체 비활성화
        myCollider.enabled = false;

        // 막아주고 있던 방어막 가리기
        sheild.SetActive(false);

        // 부서지는 애니메이션 갖고 있는 방어막 키기
        brokenSheild.SetActive(true);

        // 한대 맞으면 게임 터지는 코어 심장 활성화 하기
        coreHeart.OnCollider();
    }



    [Header("코어 주변 적 탐색에 필요한 변수")]
    public LayerMask layerMask;
    public float radius;
    WaitForSeconds delay = new WaitForSeconds(0.3f);
    bool isShowSheild = false;
    float alpha = 0;
    float waitSecond = 0;
    Coroutine fadeInCoroutine = null; // 알파가 값소될 때의 코루틴
    IEnumerator SearchCoroutine()
    {
        while (true)
        {
            while (isShowSheild) yield return null;

            if (Physics.CheckSphere(transform.position, radius, layerMask))
            {
                if (!isShowSheild)
                {
                    if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);

                    StartCoroutine(ShowCoroutine());
                }
            }

            yield return delay;
        }
    }
    IEnumerator ShowCoroutine()
    {
        isShowSheild = true;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime;

            coreMaterial.SetFloat("_Alpha", alpha);

            yield return null;
        }

        alpha = 1f;
        coreMaterial.SetFloat("_Alpha", alpha);

        waitSecond = 5;

        while (waitSecond > 0f)
        {
            waitSecond -= Time.deltaTime;

            yield return null;
        }

        isShowSheild = false;

        fadeInCoroutine = StartCoroutine(HideCoroutine());
    }
    IEnumerator HideCoroutine()
    {
        while (alpha > 1f)
        {
            alpha -= Time.deltaTime;

            coreMaterial.SetFloat("_Alpha", alpha);

            yield return null;
        }
        alpha = 0;

        coreMaterial.SetFloat("_Alpha", alpha);
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;

    //    Gizmos.DrawSphere(transform.position, radius);
    //}
}
