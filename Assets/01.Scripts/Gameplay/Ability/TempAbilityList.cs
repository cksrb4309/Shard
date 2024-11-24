using System;
using UnityEngine;

public class TempAbilityList : MonoBehaviour
{
    static TempAbilityList instance = null;

    public TempAbilityPair[] abilityArray;
    private void Awake()
    {
        instance = this;
    }

    public static TempAbility GetAbility(string abilityTypeName) => instance.Get(abilityTypeName);
    TempAbility Get(string abilityTypeName)
    {
        for (int i = 0; i < abilityArray.Length; i++)
            if (abilityArray[i].abilityName.Equals(abilityTypeName))
                return abilityArray[i].ability;
                // return (TempAbility)Activator.CreateInstance(abilityArray[i].ability.GetType());
        Debug.Log("어빌리티 찾지 못함 : " + abilityTypeName);
        return null;
    }
}
[System.Serializable]
public class TempAbilityPair
{
    public TempAbility ability;
    public string abilityName;
}