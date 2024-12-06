using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUIConnectManager : MonoBehaviour
{
    static GameSceneUIConnectManager instance = null;

    // �÷��̾�� �ʿ��� UI ���
    public Slider hpSlider; // ü�� �� 
    public TMP_Text hpText; // ü�� �ؽ�Ʈ
    public Slider sheildSlider; // �ǵ� ��

    public Image normalAttackImage; // �Ϲ� ���� SkillImage
    public SkillSlot subSkillSlot; // ���� ��ų ����
    public SkillSlot mainSkillSlot; // ���� ��ų ����

    public UpgradePanel[] upgradePanels; // �ھ� ���׷��̵� �гε�

    public CoreInteractUI coreInteractUI; // �κ��丮 UI

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
