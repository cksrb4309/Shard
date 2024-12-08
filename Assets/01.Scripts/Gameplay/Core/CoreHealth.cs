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
        if (defense <= 0) return 0f;                                // 0�� ��
        if (defense <= 5) return defense * 8f;                      // 0 ~ 5 ����: 1�� 8%�� ����
        if (defense <= 10) return 40f + (defense - 5) * 5f;         // 5 ~ 10 ����: 1�� 5%�� ����
        if (defense <= 15) return 65f + (defense - 10) * 3f;        // 10 ~ 15 ����: 1�� 3%�� ����
        if (defense <= 20) return 80f + (defense - 15) * 2;         // 15 ~ 20 ����: 1�� 2%�� ����
        return 65f + (defense - 20) * 1f;                           // 20 �̻��� 1�� 1%�� ����
    }

    Coroutine notificationDelay = null;
    public void TakeDamage(float damage)
    {
        SoundManager.SheildAttackSoundPlay();

        // ������ ���¸�ŭ ���̱�
        damage -= (damage * 0.01f * GetDamageReduction());

        // ������ �����ϱ�
        currentHealth -= damage;
        
        // ������ �ؽ�Ʈ ����
        DamageTextController.OnDamageText(transform.position, damage, true);

        // ���� ������ ����
        coreMaterial.SetFloat("_DamageLevel", 1 - currentHealth / maxHealth);

        if (notificationDelay == null)
        {
            notificationDelay = StartCoroutine(DelayCoroutine());
            RealtimeCanvasUI.Notification(IconType.Warning, transform.position, "����ü�� ���� �ް� �ֽ��ϴ� !");
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
    void Dead() // TODO ����ü�� ü���� ��� �޾��� ��
    {
        // �浹ü ��Ȱ��ȭ
        myCollider.enabled = false;

        // �����ְ� �ִ� �� ������
        sheild.SetActive(false);

        // �μ����� �ִϸ��̼� ���� �ִ� �� Ű��
        brokenSheild.SetActive(true);

        // �Ѵ� ������ ���� ������ �ھ� ���� Ȱ��ȭ �ϱ�
        coreHeart.OnCollider();
    }



    [Header("�ھ� �ֺ� �� Ž���� �ʿ��� ����")]
    public LayerMask layerMask;
    public float radius;
    WaitForSeconds delay = new WaitForSeconds(0.3f);
    bool isShowSheild = false;
    float alpha = 0;
    float waitSecond = 0;
    Coroutine fadeInCoroutine = null; // ���İ� ���ҵ� ���� �ڷ�ƾ
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
