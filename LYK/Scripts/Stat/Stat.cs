using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//스탯관리
[Serializable]
public class Stat
{
    public int statIndex;//스탯인덱스
    public string statName;//스탯이름
    public float baseValue;//스탯 시작값

    [SerializeField] private float _value;
    [SerializeField] private List<StatModifier> statModifiers = new List<StatModifier>();

    public float GetValue
    {
        get
        {
            _value = CalculateFinalValue();
            return _value;
        }
    }

    /// <summary>스탯 기본값(baseValue) </summary>
    public Stat(int _statIndex, string _statName, float _baseValue)
    {
        statIndex = _statIndex;
        statName = _statName;
        baseValue = _baseValue;
    }

    /// <summary> 같은 스탯 수치 더해주기 </summary>
    public void AddModifier(StatModifier mod)
    {
        if (mod.statValue != 0)
            statModifiers.Add(mod);
        //Debug.Log(statModifiers.Count);
    }

    /// <summary> 같은 스탯 수치 제거 </summary>
    public bool RemoveModifier(StatModifier mod)
    {
        return statModifiers.Remove(mod);
    }

    /// <summary>스탯 최종 값 반환 </summary>
    private float CalculateFinalValue()
    {
        float finalValue = baseValue;
        float percentValue = 100;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            //finalValue += statModifiers[i].Value;

            StatModifier mod = statModifiers[i];

            if (mod.statType == StatModType.Float)
            {
                finalValue += mod.statValue;
            }
            else if (mod.statType == StatModType.Percent)
            {
                //110% -> 1.1%
                //finalValue *= 1 + mod.Value;
                percentValue += mod.statValue;
            }
        }

        percentValue = percentValue * 0.01f;

        finalValue = finalValue * percentValue;

        //12.0001f != 12f
        //return (float)Math.Round(finalValue, 4);
        return finalValue;
    }
}