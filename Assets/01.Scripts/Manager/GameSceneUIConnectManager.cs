using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUIConnectManager : MonoBehaviour
{
    static GameSceneUIConnectManager instance = null;

    // 플레이어에게 필요한 UI 요소
    public Slider hpSlider; // 체력 바 
    public TMP_Text hpText; // 체력 텍스트
    public Slider sheildSlider; // 실드 바

    public Image normalAttackImage; // 일반 공격 SkillImage
    public SkillSlot subSkillSlot; // 서브 스킬 슬롯
    public SkillSlot mainSkillSlot; // 메인 스킬 슬롯

    public UpgradePanel[] upgradePanels; // 코어 업그레이드 패널들

    public CoreInteractUI coreInteractUI; // 인벤토리 UI

    int upgradePanelIndex = 0;

    private void Awake()
    {
        instance = this;
    }
    public static Slider GetHpSlider() => instance.hpSlider;
    public static TMP_Text GetHpText() => instance.hpText;
    public static Slider GetSheildSlider() => instance.sheildSlider;
    public static Image GetNormalAttackImage() => instance.normalAttackImage;
    public static SkillSlot GetSubSkillSlot() => instance.subSkillSlot;
    public static SkillSlot GetMainSkillSlot() => instance.mainSkillSlot;
    public static UpgradePanel GetUpgradePanel()
    {
        instance.upgradePanels[instance.upgradePanelIndex].gameObject.SetActive(true);

        return instance.upgradePanels[instance.upgradePanelIndex++];
    }
    public static CoreInteractUI GetCoreInteractUI() => instance.coreInteractUI;
}
