using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatModType
{
    Float,//값
    Percent,//퍼센트
}

[Serializable]
public class StatModifier
{
    public float statValue;
    public StatModType statType;
    public int statIndex;

    public StatModifier(float _value, StatModType _type, int _statIndex)
    {
        statValue = _value;
        statType = _type;
        statIndex = _statIndex;
    }
}
