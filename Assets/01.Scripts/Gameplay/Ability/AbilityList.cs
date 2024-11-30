using System;
using UnityEngine;

public class AbilityList : MonoBehaviour
{
    static AbilityList instance = null;

    public AbilityPair[] abilityArray;
    private void Awake()
    {
        instance = this;
    }

    public static Ability GetAbility(string abilityTypeName) => instance.Get(abilityTypeName);
    Ability Get(string abilityTypeName)
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
public class AbilityPair
{
    public Ability ability;
    public string abilityName;
}