using System.Collections.Generic;
using UnityEngine;

public class BaseBlock : MonoBehaviour, IAttackable
{
    static readonly int DamageLevelShaderId = Shader.PropertyToID("_DamageLevel");

    public static bool isSetting = false;

    public bool isNearest = false;

    public Transform coreTransform = null;

    public BlockSettingEffect blockOnEffect;

    public float maxHp = 1000f;
    public float minMaxHp = 1000f;
    public float maxMaxHp = 10000f;
    public float maxLength = 80f;
    public float minLength = 9f;
    public int reward = 5;
    public int minReward = 5;
    public int maxReward = 30;

    float hp;

    MeshRenderer blockRenderer;
    MaterialPropertyBlock blockPropertyBlock;

    List<(string, Coroutine)> statusEffectList = new List<(string, Coroutine)>();

    Vector3 pos;

    GameObject blockObj;

    Collider cd;
    private void Start()
    {
        pos = transform.position + new Vector3(0.39194f, 0, 0);
        hp = maxHp;
        blockObj = transform.GetChild(0).gameObject;
        blockRenderer = blockObj.GetComponent<MeshRenderer>();
        blockPropertyBlock = new MaterialPropertyBlock();
        cd = GetComponent<Collider>();

        SetDamageLevel(0f);
    }
    public bool IsAlive()
    {
        return hp > 0;
    }
    public void ReceiveHit(float damage, bool isCritical = false)
    {
        if (!IsAlive()) return;

        if (isSetting) return;

        SoundManager.BlockAttackSoundPlay();

        hp -= damage;

        if (isCritical)
            DamageTextController.OnCriticalDamageText(pos, damage);
        else
            DamageTextController.OnDamageText(pos, damage);

        GameManager.SetLastHit(this);

        if (!IsAlive())
        {
            Dead();
        }
        else
        {
            SetDamageLevel(1f - hp / maxHp);
        }
    }
    void Dead()
    {
        BreakBaseBlock breakBlock = PoolingManager.Instance.GetObject<BreakBaseBlock>("BreakBaseBlock");
        if (breakBlock != null)
            breakBlock.Play(pos);

        RewardManager.BaseBlockDrop(reward);

        SpawnManager.currentBlockCount--;

        blockObj.SetActive(false);

        cd.enabled = false;

        SetDamageLevel(0f);
    }
    public Vector3 GetPosition()
    {
        return pos;
    }
    public void ReceiveDebuff(StatusEffect effect, float damage)
    {
        ReceiveHit(damage);
    }
    public void Setting()
    {
        Vector3 p = transform.position + new Vector3(0.39194f, 0, 0);

        p.Set(p.x, p.y, p.z * 1.9f);

        float length = (p - coreTransform.position).magnitude;

        maxHp = Mathf.Lerp(minMaxHp, maxMaxHp, Mathf.InverseLerp(minLength, maxLength, length));
        reward = (int)Mathf.Lerp(minReward, maxReward, Mathf.InverseLerp(minLength, maxLength, length));
    }
    public void ReSetting()
    {
        if (isNearest) blockOnEffect.Play(pos);

        blockObj.SetActive(true); // 보이도록 활성화

        cd.enabled = true; // 죽어있었다면 콜라이더가 비활성화 되어있으므로 활성화 시킴

        // 블럭의 체력 또한 다음 스테이지에 대한 값으로 바꿔야한다
        maxHp *= 3f; // 우선은 최대 체력을 스테이지 넘어갈 때마다 *3을 적용한다

        hp = maxHp;

        reward *= 3; // 보상 또한 3배로 늘림

        SetDamageLevel(0f); // 데미지 정도도 수정한다
    }

    void SetDamageLevel(float damageLevel)
    {
        if (blockRenderer == null) return;

        blockRenderer.GetPropertyBlock(blockPropertyBlock);
        blockPropertyBlock.SetFloat(DamageLevelShaderId, damageLevel);
        blockRenderer.SetPropertyBlock(blockPropertyBlock);
    }

}
