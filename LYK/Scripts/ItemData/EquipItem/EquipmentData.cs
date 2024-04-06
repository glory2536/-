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
    public EquipType equipType;//���Ÿ��
    public Grade equipGrade;//�����
    public GameObject equipPrefab;//���������
    public List<StatModifier> equipStatList = new List<StatModifier>();//����ϴ� ��������(���� �� = 0 ����)
}
