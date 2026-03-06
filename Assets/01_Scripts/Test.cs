using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] List<AbilityPair> abilities;
    private void Start()
    {
        foreach (var abilityPair in abilities)
        {
            for (int i = 0; i < abilityPair.count; i++)
            {
                AbilityManager.Instance.RegisterAbility(abilityPair.ability);
            }
        }
    }
}

[System.Serializable]
public class AbilityPair
{
    public Ability ability;
    public int count;
}