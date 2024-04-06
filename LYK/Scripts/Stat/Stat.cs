using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//���Ȱ���
[Serializable]
public class Stat
{
    public int statIndex;//�����ε���
    public string statName;//�����̸�
    public float baseValue;//���� ���۰�

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

    /// <summary>���� �⺻��(baseValue) </summary>
    public Stat(int _statIndex, string _statName, float _baseValue)
    {
        statIndex = _statIndex;
        statName = _statName;
        baseValue = _baseValue;
    }

    /// <summary> ���� ���� ��ġ �����ֱ� </summary>
    public void AddModifier(StatModifier mod)
    {
        if (mod.statValue != 0)
            statModifiers.Add(mod);
        //Debug.Log(statModifiers.Count);
    }

    /// <summary> ���� ���� ��ġ ���� </summary>
    public bool RemoveModifier(StatModifier mod)
    {
        return statModifiers.Remove(mod);
    }

    /// <summary>���� ���� �� ��ȯ </summary>
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