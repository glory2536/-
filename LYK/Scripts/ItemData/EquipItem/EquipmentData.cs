using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipType
{
    EquipHead,
    EquipHand,
    EquipBody,
    EquipShoes,
    EquipWeapon,
    WoodWeapon,
    MiningWeapon
}

[Serializable]
public abstract class EquipmentData : ItemData
{
    public EquipType equipType;//장비타입
    public Grade equipGrade;//장비등급
    public GameObject equipPrefab;//장비프리팹
    public List<StatModifier> equipStatList = new List<StatModifier>();//사용하는 고유스탯(스탯 값 = 0 제외)
}
