using System.Collections;
using UnityEngine;

public class CoreHeart : MonoBehaviour, IDamageable
{
    public Collider cd;
    public Material material;

    private void Awake()
    {
        material.SetFloat("_DamageLevel", 0);
    }

    bool isHit = false;

    public bool IsAlive()
    {
        return !isHit;
    }

    public void OnCollider()
    {
        cd.enabled = true;
    }
    public void TakeDamage(float damage)
    {
        isHit = true;

        GameOver();
    }
    public void TakeDebuff(float damage, StatusEffect statusEffect)
    {
        TakeDamage(damage);
    }

    public void GameOver() // TODO 게임 오버 구현 해야함
    {
        cd.enabled = false;

        StartCoroutine(GameOverCoroutine());


        //ScreenTransition.Play("씬전환시작효과", "씬전환종료효과", Color.black, Color.black, "Title", 0, 1f);
    }
    IEnumerator GameOverCoroutine()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime;

            if (t >= 1f) t = 1f;

            material.SetFloat("_DamageLevel", t);

            yield return null;
        }

        GameManager.PlayerKill();

        yield return new WaitForSeconds(1f);
    }
}
